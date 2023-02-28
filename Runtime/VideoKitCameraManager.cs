/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Events;
    using Unity.Collections.LowLevel.Unsafe;
    using NatML.Devices;
    using NatML.Devices.Outputs;
    using NatML.Features;
    using ML;
    using Internal;

    /// <summary>
    /// VideoKit camera manager for streaming video from camera devices.
    /// </summary>
    [Tooltip(@"VideoKit camera manager for streaming video from camera devices.")]
    [HelpURL(@"https://docs.videokit.ai/videokit/api/videokitcameramanager")]
    [DisallowMultipleComponent]
    public sealed partial class VideoKitCameraManager : MonoBehaviour, IVideoKitDeviceManager<CameraDevice> {

        #region --Enumerations--
        /// <summary>
        /// Camera manager capabilities.
        /// </summary>
        [Flags]
        public enum Capabilities : int {
            /// <summary>
            /// Stream depth data along with the camera preview data.
            /// This flag adds a minimal performance cost, so enable it only when necessary.
            /// This flag is only supported on iOS and Android.
            /// </summary>
            Depth               = 0b0001,
            /// <summary>
            /// Ensure that the camera preview data can be used for machine learning predictions.
            /// Enabling this adds a minimal performance impact so it should be disabled when not needed.
            /// This flag is always enabled on WebGL.
            /// </summary>
            MachineLearning     = 0b00010,
            /// <summary>
            /// Generate a human texture from the camera preview stream.
            /// This flag adds a variable performance cost, so enable it only when necessary.
            /// </summary>
            HumanTexture        = MachineLearning | 0b00100,
            /// <summary>
            /// Detect human poses in the camera preview stream.
            /// This flag adds a variable performance cost, so enable it only when necessary.
            /// </summary>
            PoseDetection       = MachineLearning | 0b01000,
            /// <summary>
            /// Detect faces in the camera preview stream.
            /// This flag adds a variable performance cost, so enable it only when necessary.
            /// </summary>
            FaceDetection       = MachineLearning | 0b10000,
        }

        /// <summary>
        /// Camera facing.
        /// </summary>
        public enum Facing : int { // bit flags: [prefer/require user/world]
            /// <summary>
            /// Prefer a user-facing camera but enable fallback to any available camera.
            /// </summary>
            PreferUser = 0b00,
            /// <summary>
            /// Prefer a world-facing camera but enable fallback to any available camera.
            /// </summary>
            PreferWorld = 0b01,
            /// <summary>
            /// Require a user-facing camera.
            /// </summary>
            RequireUser = 0b10,
            /// <summary>
            /// Require a world-facing camera.
            /// </summary>
            RequireWorld = 0b11,
        }

        /// <summary>
        /// Camera resolution presets.
        /// </summary>
        public enum Resolution : int {
            /// <summary>
            /// Use the default camera resolution.
            /// With this preset, the camera resolution will not be set.
            /// </summary>
            Default     = 0,
            /// <summary>
            /// Lowest resolution supported by the camera device.
            /// </summary>
            Lowest      = 1,
            /// <summary>
            /// SD resolution.
            /// </summary>
            _640x480    = 2,
            /// <summary>
            /// HD resolution.
            /// </summary>
            _1280x720   = 3,
            /// <summary>
            /// Full HD resolution.
            /// </summary>
            _1920x1080  = 4,
            /// <summary>
            /// 4K UHD resolution.
            /// </summary>
            _4K         = 5,
            /// <summary>
            /// Highest resolution supported by the camera device.
            /// Using this resolution is strongly not recommended.
            /// </summary>
            Highest     = 10
        }

        /// <summary>
        /// </summary>
        public enum FrameRate : int {
            /// <summary>
            /// Use the default camera frame rate.
            /// With this preset, the camera frame rate will not be set.
            /// </summary>
            Default = 0,
            /// <summary>
            /// </summary>
            Lowest  = 1,
            /// <summary>
            /// </summary>
            _15     = 15,
            /// <summary>
            /// </summary>
            _30     = 30,
            /// <summary>
            /// </summary>
            _60     = 60,
            /// <summary>
            /// </summary>
            _120    = 120,
            /// <summary>
            /// </summary>
            _240    = 240
        }
        #endregion


        #region --Inspector--
        [Header(@"Configuration")]
        /// <summary>
        /// Desired camera capabilities.
        /// </summary>
        [Tooltip(@"Desired camera capabilities.")]
        public Capabilities capabilities = 0;

        /// <summary>
        /// Whether to start the camera preview as soon as the component awakes.
        /// </summary>
        [Tooltip(@"Whether to start the camera preview as soon as the component awakes.")]
        public bool playOnAwake = true;

        [Header(@"Camera Settings")]
        /// <summary>
        /// Desired camera facing.
        /// </summary>
        [SerializeField, Tooltip(@"Desired camera facing.")]
        private Facing _facing = Facing.PreferUser;

        /// <summary>
        /// Desired camera resolution.
        /// </summary>
        [Tooltip(@"Desired camera resolution.")]
        public Resolution resolution = Resolution._1280x720;

        /// <summary>
        /// Desired camera frame rate.
        /// </summary>
        [Tooltip(@"Desired camera frame rate.")]
        public FrameRate frameRate = FrameRate._30;

        /// <summary>
        /// Desired camera focus mode.
        /// </summary>
        [Tooltip(@"Desired camera focus mode.")]
        public CameraDevice.FocusMode focusMode = CameraDevice.FocusMode.Continuous;

        /// <summary>
        /// Desired camera exposure mode.
        /// </summary>
        [Tooltip(@"Desired camera exposure mode.")]
        public CameraDevice.ExposureMode exposureMode = CameraDevice.ExposureMode.Continuous;

        [Header(@"Events")]
        /// <summary>
        /// Event raised when a new camera frame is available.
        /// </summary>
        [Tooltip(@"Event raised when a new camera frame is available.")]
        public UnityEvent<CameraFrame> OnCameraFrame;
        #endregion


        #region --Client API--
        /// <summary>
        /// Get or set the camera device used for streaming.
        /// </summary>
        public CameraDevice device {
            get => cameraDevice;
            set {
                // Switch cameras without disposing output
                // We deliberately skip configuring the camera like we do in `StartRunning`
                if (running) {
                    cameraDevice.StopRunning();
                    cameraDevice = value;
                    if (cameraDevice != null)
                        cameraDevice.StartRunning(output);
                    else {
                        StopRunning();
                        Debug.LogError(@"VideoKit: Camera manager failed to start running because camera device is null");
                    }
                }
                // Handle trivial case
                else
                    cameraDevice = value;
            }
        }

        /// <summary>
        /// Get or set the desired camera facing.
        /// </summary>
        public Facing facing {
            get => _facing;
            set {
                if (_facing != value)
                    device = GetDefaultCameraDevice(_facing = value);
            }
        }

        /// <summary>
        /// Whether the camera is running.
        /// </summary>
        public bool running => output != null;

        /// <summary>
        /// Start the camera preview.
        /// </summary>
        public async void StartRunning () {
            // Check
            if (!isActiveAndEnabled) {
                Debug.LogError(@"VideoKit: Camera manager failed to start running because component is disabled");
                return;
            }
            // Check
            if (running)
                return;
            // Request camera permissions
            var permissionStatus = await MediaDeviceQuery.RequestPermissions<CameraDevice>();
            if (permissionStatus != PermissionStatus.Authorized) {
                Debug.LogWarning(@"VideoKit: User did not grant camera permissions");
                return;
            }
            // Check device
            if (cameraDevice == null) {
                // Get default device
                cameraDevice = GetDefaultCameraDevice(_facing);
                if (cameraDevice == null) {
                    Debug.LogError(@"VideoKit: Camera manager failed to start running because no camera device is available");
                    return;
                }
                // Configure camera
                if (resolution != Resolution.Default)
                    cameraDevice.previewResolution = frameSize;
                if (frameRate != FrameRate.Default)
                    cameraDevice.frameRate = (int)frameRate;
                if (cameraDevice.FocusModeSupported(focusMode))
                    cameraDevice.focusMode = focusMode;
                if (cameraDevice.ExposureModeSupported(exposureMode))
                    cameraDevice.exposureMode = exposureMode;
            }
            // Check capabilities
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                capabilities |= Capabilities.MachineLearning;
            // Create MatteKit predictor
            if (capabilities.HasFlag(Capabilities.HumanTexture))
                matteKit ??= await MatteKitPredictor.Create(configuration: matteKitConfiguration);
            // Create output
            if (capabilities.HasFlag(Capabilities.MachineLearning)) {
                var textureOutput = new TextureOutput();
                textureOutput.OnFrame += OnCameraTexture;
                output = textureOutput;
            } else {
                var renderTextureOutput = new RenderTextureOutput();
                renderTextureOutput.OnFrame += OnCameraTexture;
                output = renderTextureOutput;
            }
            // Start running
            cameraDevice.StartRunning(output);
        }

        /// <summary>
        /// Stop the camera preview.
        /// </summary>
        public void StopRunning () {
            cameraDevice?.StopRunning();
            output?.Dispose();
            output = null;
        }
        #endregion


        #region --Operations--
        private CameraDevice cameraDevice;
        private CameraOutput output;
        private MatteKitPredictor matteKit;
        private readonly MLEdgeModel.Configuration matteKitConfiguration = new () {
            computeTarget = MLEdgeModel.ComputeTarget.CPU // don't ask, trust
        };

        private (int width, int height) frameSize => resolution switch {
            VideoKitCameraManager.Resolution.Lowest     => (176, 144),
            VideoKitCameraManager.Resolution._640x480   => (640, 480),
            VideoKitCameraManager.Resolution._1280x720  => (1280, 720),
            VideoKitCameraManager.Resolution._1920x1080 => (1920, 1080),
            VideoKitCameraManager.Resolution._4K        => (3840, 2160),
            VideoKitCameraManager.Resolution.Highest    => (5120, 2880),
            _                                           => (1280, 720),
        };

        private void Awake () {
            if (playOnAwake)
                StartRunning();
        }

        private void OnCameraTexture (TextureOutput output) {
            var feature = new MLImageFeature(output.texture);
            matteKit?.Predict(feature);
            var frame = new CameraFrame(output.image, output.texture, feature, matteKit?.humanTexture);
            OnCameraFrame?.Invoke(frame);
        }

        private void OnCameraTexture (RenderTextureOutput output) {
            var frame = new CameraFrame(output.image, output.texture, null, null);
            OnCameraFrame?.Invoke(frame);
        }

        private void OnDestroy () {
            StopRunning();
            matteKit?.Dispose();
        }

        internal (int width, int height) GetPreviewSize () => output switch {
            TextureOutput o         => (o.texture.width, o.texture.height),
            RenderTextureOutput o   => (o.texture.width, o.texture.height),
            _                       => default,
        };

        private static CameraDevice GetDefaultCameraDevice (Facing facing) {
            // Create query
            MediaDeviceQuery.ConfigureAudioSession = false;
            var query = new MediaDeviceQuery(MediaDeviceCriteria.CameraDevice);
            // Discover camera
            var frontFacing = !facing.HasFlag(Facing.PreferWorld);
            var fallback = !facing.HasFlag(Facing.RequireUser);
            var fallbackDevice = fallback ? query.current : null;
            var device = query.FirstOrDefault(device => device is CameraDevice camera && camera.frontFacing == frontFacing) ?? fallbackDevice;
            // Return
            return device as CameraDevice;
        }
        #endregion
    }
}