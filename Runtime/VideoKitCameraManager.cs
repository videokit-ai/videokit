/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Events;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using NatML;
    using NatML.Features;
    using AI;
    using Devices;
    using Devices.Outputs;
    using Internal;

    /// <summary>
    /// VideoKit camera manager for streaming video from camera devices.
    /// </summary>
    [Tooltip(@"VideoKit camera manager for streaming video from camera devices.")]
    [HelpURL(@"https://docs.videokit.ai/videokit/api/videokitcameramanager")]
    [DisallowMultipleComponent]
    public sealed class VideoKitCameraManager : VideoKitDeviceManager<CameraDevice> {

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
            Depth           = 0b0001,
            /// <summary>
            /// Ensure that the camera preview data can be used for AI predictions.
            /// Enabling this adds a minimal performance impact so it should be disabled when not needed.
            /// This flag is always enabled on WebGL.
            /// </summary>
            AI              = 0b00010,
            /// <summary>
            /// Generate a human texture from the camera preview stream.
            /// This flag adds a variable performance cost, so enable it only when necessary.
            /// NOTE: This requires an active VideoKit AI plan.
            /// </summary>
            HumanTexture    = AI | 0b00100,
            /// <summary>
            /// Detect human poses in the camera preview stream.
            /// This flag adds a variable performance cost, so enable it only when necessary.
            /// NOTE: This requires an active VideoKit AI plan.
            /// </summary>
            PoseDetection   = AI | 0b01000,
            /// <summary>
            /// Detect faces in the camera preview stream.
            /// This flag adds a variable performance cost, so enable it only when necessary.
            /// NOTE: This requires an active VideoKit AI plan.
            /// </summary>
            FaceDetection   = AI | 0b10000,
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
        /// Event raised when a new camera frame is available in the camera manager.
        /// The preview texture, human texture, and image feature will contain the latest camera image.
        /// </summary>
        [Tooltip(@"Event raised when a new camera frame is available.")]
        public UnityEvent OnCameraFrame;
        #endregion


        #region --Client API--
        /// <summary>
        /// Get or set the camera device used for streaming.
        /// </summary>
        public override CameraDevice device {
            get => _device;
            set {
                // Switch cameras without disposing output
                // We deliberately skip configuring the camera like we do in `StartRunning`
                if (running) {
                    _device.StopRunning();
                    _device = value;
                    _device?.StartRunning(output);
                }
                // Handle trivial case
                else
                    _device = value;
            }
        }

        /// <summary>
        /// Get or set the desired camera facing.
        /// </summary>
        public Facing facing {
            get => _facing;
            set {
                if (_facing == value)
                    return;
                _facing = value;
                device = GetDefaultCameraDevice(_facing);
            }
        }

        /// <summary>
        /// Get the latest camera image that has been processed by the camera manager.
        /// </summary>
        public CameraImage image => output?.image ?? default;

        /// <summary>
        /// Get the camera preview texture.
        /// </summary>
        public Texture texture => output switch {
            TextureOutput x         => x.texture,
            RenderTextureOutput x   => x.texture,
            _                       => null,
        };

        /// <summary>
        /// Get the camera preview pixel buffer.
        /// The pixel buffer always has the `RGBA8888` layout.
        /// NOTE: This requires the `AI` capability to be enabled.
        /// </summary>
        public NativeArray<byte> pixelBuffer => output switch {
            TextureOutput x => x.texture.GetRawTextureData<byte>(),
            _               => default,  
        };

        /// <summary>
        /// Get the camera preview image feature for AI predictions.
        /// NOTE: This requires the `AI` capability to be enabled.
        /// </summary>
        public MLImageFeature imageFeature => output is TextureOutput textureOutput ? new MLImageFeature(textureOutput.texture) : null;

        /// <summary>
        /// Get the camera human texture.
        /// NOTE: This requires the `HumanTexture` capability to be enabled.
        /// </summary>
        public Texture2D humanTexture => matteKit?.humanTexture;

        /// <summary>
        /// Whether the camera is running.
        /// </summary>
        public override bool running => output != null;

        /// <summary>
        /// Event raised when a new camera image is provided by the camera device.
        /// NOTE: This event is usually raised on the camera thread, not the Unity main thread.
        /// </summary>
        public event Action<CameraImage> OnCameraImage;

        /// <summary>
        /// Start the camera preview.
        /// </summary>
        public override async Task StartRunning () {
            // Check
            if (!isActiveAndEnabled)
                throw new InvalidOperationException(@"VideoKit: Camera manager failed to start running because component is disabled");
            // Check
            if (running)
                return;
            // Request camera permissions
            var permissions = await CameraDevice.CheckPermissions(request: true);
            if (permissions != MediaDevice.PermissionStatus.Authorized)
                throw new InvalidOperationException(@"VideoKit: User did not grant camera permissions");
            // Discover devices
            devices = await CameraDevice.Discover();
            // Check device
            _device ??= GetDefaultCameraDevice(_facing);
            if (_device == null)
                throw new InvalidOperationException(@"VideoKit: Camera manager failed to start running because no camera device is available");
            // Configure camera
            if (resolution != Resolution.Default)
                _device.previewResolution = frameSize;
            if (frameRate != FrameRate.Default)
                _device.frameRate = (int)frameRate;
            if (_device.FocusModeSupported(focusMode))
                _device.focusMode = focusMode;
            if (_device.ExposureModeSupported(exposureMode))
                _device.exposureMode = exposureMode;
            // Check capabilities
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                capabilities |= Capabilities.AI;
            // Create MatteKit predictor
            if (capabilities.HasFlag(Capabilities.HumanTexture))
                matteKit ??= await MatteKitPredictor.Create(configuration: matteKitConfiguration);
            // Create output
            if (capabilities.HasFlag(Capabilities.AI)) {
                var textureOutput = new TextureOutput();
                textureOutput.OnFrame += OnCameraTexture;
                output = textureOutput;
            } else {
                var renderTextureOutput = new RenderTextureOutput();
                renderTextureOutput.OnFrame += OnCameraTexture;
                output = renderTextureOutput;
            }
            // Start running
            _device.StartRunning(UpdateCameraImage);
        }

        /// <summary>
        /// Stop the camera preview.
        /// </summary>
        public override void StopRunning () {
            _device?.StopRunning();
            output?.Dispose();
            output = null;
        }
        #endregion


        #region --Operations--
        private CameraDevice[] devices;
        private CameraDevice _device;
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

        private async void Awake () {
            if (playOnAwake)
                await StartRunning();
        }

        private void UpdateCameraImage (CameraImage image) {
            output.Update(image);
            OnCameraImage?.Invoke(image);
        }

        private void OnCameraTexture (TextureOutput output) {
            matteKit?.Predict(imageFeature);
            OnCameraFrame?.Invoke();
        }

        private void OnCameraTexture (RenderTextureOutput output) => OnCameraFrame?.Invoke();

        private void OnDestroy () {
            StopRunning();
            matteKit?.Dispose();
        }

        internal (int width, int height) GetPreviewSize () => output switch {
            TextureOutput o         => (o.texture.width, o.texture.height),
            RenderTextureOutput o   => (o.texture.width, o.texture.height),
            _                       => default,
        };

        private CameraDevice GetDefaultCameraDevice (Facing facing) {
            // Get fallback device
            var fallback = !facing.HasFlag(Facing.RequireUser);
            var fallbackDevice = fallback ? devices?.FirstOrDefault() : null;
            // Find device
            var frontFacing = !facing.HasFlag(Facing.PreferWorld);
            var device = devices?.FirstOrDefault(d => frontFacing && d.frontFacing) ?? fallbackDevice;
            // Return
            return device;
        }
        #endregion
    }
}