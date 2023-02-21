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
    [Tooltip(@"VideoKit camera manager for streaming video from camera devices."), DisallowMultipleComponent]
    public sealed partial class VideoKitCameraManager : MonoBehaviour, IVideoKitDeviceManager<CameraDevice> {

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
                    cameraDevice = device;
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
                if (device.FocusModeSupported(focusMode))
                    cameraDevice.focusMode = focusMode;
                if (device.ExposureModeSupported(exposureMode))
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