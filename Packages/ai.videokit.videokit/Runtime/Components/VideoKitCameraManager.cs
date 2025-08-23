/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;
    using Internal;

    /// <summary>
    /// VideoKit camera manager for streaming video from camera devices.
    /// </summary>
    [Tooltip(@"VideoKit camera manager for streaming video from camera devices.")]
    [HelpURL(@"https://videokit.ai/reference/videokitcameramanager")]
    [DisallowMultipleComponent]
    public sealed class VideoKitCameraManager : MonoBehaviour {

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
            /// Generate a human texture from the camera preview stream.
            /// This flag adds a variable performance cost, so enable it only when necessary.
            /// </summary>
            HumanTexture    = 0b00110,
        }

        /// <summary>
        /// Camera facing.
        /// </summary>
        [Flags]
        public enum Facing : int {
            /// <summary>
            /// User-facing camera.
            /// </summary>
            User = 0b10,
            /// <summary>
            /// World-facing camera.
            /// </summary>
            World = 0b01,
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
            [InspectorName(@"1280x720 (HD)")]
            _1280x720   = 3,
            /// <summary>
            /// Full HD resolution.
            /// </summary>
            [InspectorName(@"1920x1080 (Full HD)")]
            _1920x1080  = 4,
            /// <summary>
            /// 2K WQHD resolution.
            /// </summary>
            [InspectorName(@"2560x1440 (2K)")]
            _2560x1440 = 6,
            /// <summary>
            /// 4K UHD resolution.
            /// </summary>
            [InspectorName(@"3840x2160 (4K)")]
            _3840x2160  = 5,
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
            /// Use the lowest frame rate supported by the camera.
            /// </summary>
            Lowest  = 1,
            /// <summary>
            /// 15FPS.
            /// </summary>
            _15     = 15,
            /// <summary>
            /// 30FPS.
            /// </summary>
            _30     = 30,
            /// <summary>
            /// 60FPS.
            /// </summary>
            _60     = 60,
            /// <summary>
            /// 120FPS.
            /// </summary>
            _120    = 120,
            /// <summary>
            /// 240FPS.
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

        [Header(@"Camera Selection")]
        /// <summary>
        /// Desired camera facing.
        /// </summary>
        [SerializeField, Tooltip(@"Desired camera facing.")]
        private Facing _facing = Facing.User;

        /// <summary>
        /// Whether the specified facing is required.
        /// When false, the camera manager will fallback to a default camera when a camera with the requested facing is not available.
        /// </summary>
        [Tooltip(@"Whether the specified facing is required. When false, the camera manager will fallback to a default camera when a camera with the requested facing is not available.")]
        public bool facingRequired = false;

        [Header(@"Camera Settings")]
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
        #endregion


        #region --Client API--
        /// <summary>
        /// Get or set the camera device used for streaming.
        /// </summary>
        public MediaDevice? device {
            get => _device;
            set {
                // Switch cameras without disposing output
                // We deliberately skip configuring the camera like we do in `StartRunning`
                if (running) {
                    _device!.StopRunning();
                    _device = value;
                    if (_device != null)
                        StartRunning(_device, OnCameraBuffer);
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
                device = GetDefaultDevice(devices, _facing = value, facingRequired);
            }
        }

        /// <summary>
        /// Whether the camera is running.
        /// </summary>
        public bool running => _device?.running ?? false;

        /// <summary>
        /// Event raised when a new pixel buffer is provided by the camera device.
        /// NOTE: This event is invoked on a dedicated camera thread, not the Unity main thread.
        /// </summary>
        public event Action<CameraDevice, PixelBuffer>? OnPixelBuffer;

        /// <summary>
        /// Start the camera preview.
        /// </summary>
        public async void StartRunning() => await StartRunningAsync();

        /// <summary>
        /// Start the camera preview.
        /// </summary>
        public async Task StartRunningAsync() {
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
            // Check device
            devices = await GetAllDevices();
            _device ??= GetDefaultDevice(devices, _facing, facingRequired);
            if (_device == null)
                throw new InvalidOperationException(@"VideoKit: Camera manager failed to start running because no camera device is available");
            // Configure camera(s)
            foreach (var cameraDevice in EnumerateCameraDevices(_device)) {
                if (resolution != Resolution.Default)
                    cameraDevice.previewResolution = GetResolutionFrameSize(resolution);
                if (frameRate != FrameRate.Default)
                    cameraDevice.frameRate = (int)frameRate;
                if (cameraDevice.IsFocusModeSupported(focusMode))
                    cameraDevice.focusMode = focusMode;
                if (cameraDevice.IsExposureModeSupported(exposureMode))
                    cameraDevice.exposureMode = exposureMode;
            }
            // Preload human texture predictor
            var muna = VideoKitClient.Instance!.muna;
            if (capabilities.HasFlag(Capabilities.HumanTexture)) {
                try { await muna.Predictions.Create(HumanTextureTag, new()); }
                catch { // CHECK // REMOVE
                    var predictorCachePath = Path.Join(Application.persistentDataPath, @"fxn", @"predictors");
                    if (Directory.Exists(predictorCachePath))
                        Directory.Delete(predictorCachePath, recursive: true);
                }
                await muna.Predictions.Create(HumanTextureTag, new());
            }
            // Start running
            StartRunning(_device, OnCameraBuffer);
            // Listen for events
            var events = VideoKitEvents.Instance;
            events.onPause += OnPause;
            events.onResume += OnResume;
        }

        /// <summary>
        /// Stop the camera preview.
        /// </summary>
        public void StopRunning() {
            var events = VideoKitEvents.OptionalInstance;
            if (events != null) {
                events.onPause -= OnPause;
                events.onResume -= OnResume;
            }
            _device?.StopRunning();
        }
        #endregion


        #region --Operations--
        private MediaDevice[]? devices;
        private MediaDevice? _device;
        public const string HumanTextureTag = @"@videokit/human-texture";

        private void Awake() {
            if (playOnAwake)
                StartRunning();
        }

        private static void StartRunning(
            MediaDevice device,
            Action<CameraDevice, PixelBuffer> handler
        ) {
            if (device is CameraDevice cameraDevice)
                cameraDevice.StartRunning(pixelBuffer => handler(cameraDevice, pixelBuffer));
            else if (device is MultiCameraDevice multiCameraDevice)
                multiCameraDevice.StartRunning(handler);
            else
                throw new InvalidOperationException($"Cannot start running because media device has unsupported type: {device.GetType()}");
        }

        private unsafe void OnCameraBuffer(
            CameraDevice cameraDevice,
            PixelBuffer pixelBuffer
        ) => OnPixelBuffer?.Invoke(cameraDevice, pixelBuffer);

        private void OnPause() => _device?.StopRunning();

        private void OnResume() {
            if (_device != null)
                StartRunning(_device, OnCameraBuffer);
        }

        private void OnDestroy() => StopRunning();
        #endregion


        #region --Utilties--

        internal static IEnumerable<CameraDevice> EnumerateCameraDevices(MediaDevice? device) {
            if (device is CameraDevice cameraDevice)
                yield return cameraDevice;
            else if (device is MultiCameraDevice multiCameraDevice)
                foreach (var camera in multiCameraDevice.cameras)
                    yield return camera;
            else
                yield break;
        }

        internal static Facing GetCameraFacing(MediaDevice mediaDevice) => mediaDevice switch {
            CameraDevice cameraDevice           => cameraDevice.frontFacing ? Facing.User : Facing.World,
            MultiCameraDevice multiCameraDevice => multiCameraDevice.cameras.Select(GetCameraFacing).Aggregate((a, b) => a | b),
            _                                   => 0,
        };

        private static async Task<MediaDevice[]> GetAllDevices() {
            var cameraDevices = await CameraDevice.Discover(); // MUST always come before multi-cameras
            var multiCameraDevices = await MultiCameraDevice.Discover();
            var result = cameraDevices.Cast<MediaDevice>().Concat(multiCameraDevices).ToArray();
            return result;
        }

        private static MediaDevice? GetDefaultDevice(
            MediaDevice[]? devices,
            Facing facing,
            bool facingRequired
        ) {
            facing &= Facing.User | Facing.World;
            var fallbackDevice = facingRequired ? null : devices?.FirstOrDefault();
            var requestedDevice = devices?.FirstOrDefault(device => GetCameraFacing(device).HasFlag(facing));
            return requestedDevice ?? fallbackDevice;
        }

        private static (int width, int height) GetResolutionFrameSize(Resolution resolution) => resolution switch {
            Resolution.Lowest       => (176, 144),
            Resolution._640x480     => (640, 480),
            Resolution._1280x720    => (1280, 720),
            Resolution._1920x1080   => (1920, 1080),
            Resolution._2560x1440   => (2560, 1440),
            Resolution._3840x2160   => (3840, 2160),
            Resolution.Highest      => (5120, 2880),
            _                       => (1280, 720),
        };
        #endregion
    }
}