/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.Events;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Function;
    using Function.Types;
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
            /// NOTE: This requires an active VideoKit AI plan.
            /// </summary>
            HumanTexture    = 0b00110,
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
        public UnityEvent? OnCameraFrame;
        #endregion


        #region --Client API--
        /// <summary>
        /// Get or set the camera device used for streaming.
        /// </summary>
        public CameraDevice? device {
            get => _device;
            set {
                // Switch cameras without disposing output
                // We deliberately skip configuring the camera like we do in `StartRunning`
                if (running) {
                    _device!.StopRunning();
                    _device = value;
                    _device?.StartRunning(OnCameraPixelBuffer);
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
        /// Get the camera preview texture.
        /// </summary>
        public Texture2D? texture { get; private set; }

        /// <summary>
        /// Get the camera human texture.
        /// This texture has the same size as the preview texture.
        /// NOTE: This requires the `HumanTexture` capability to be enabled.
        /// </summary>
        public Texture2D? humanTexture { get; private set; }

        /// <summary>
        /// Get the camera preview rotation to become upright.
        /// </summary>
        public PixelBuffer.Rotation rotation { get; private set; }

        /// <summary>
        /// Whether the camera is running.
        /// </summary>
        public bool running => _device?.running ?? false;

        /// <summary>
        /// Event raised when a new camera image is provided by the camera device.
        /// NOTE: This event is usually raised on the camera thread, not the Unity main thread.
        /// </summary>
        public event Action<PixelBuffer>? OnPixelBuffer;

        /// <summary>
        /// Start the camera preview.
        /// </summary>
        public async void StartRunning () {
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
            devices = await CameraDevice.Discover();
            _device ??= GetDefaultCameraDevice(_facing);
            if (_device == null)
                throw new InvalidOperationException(@"VideoKit: Camera manager failed to start running because no camera device is available");
            // Configure camera
            if (resolution != Resolution.Default)
                _device.previewResolution = GetResolutionFrameSize(resolution);
            if (frameRate != FrameRate.Default)
                _device.frameRate = (int)frameRate;
            if (_device.IsFocusModeSupported(focusMode))
                _device.focusMode = focusMode;
            if (_device.IsExposureModeSupported(exposureMode))
                _device.exposureMode = exposureMode;
            // Preload human texture predictor
            if (capabilities.HasFlag(Capabilities.HumanTexture))
                await fxn.Predictions.Create(HumanTextureTag);
            // Create preview texture
            rotation = GetPreviewRotation(Screen.orientation);
            var (cameraWidth, cameraHeight) = _device.previewResolution;
            var (previewWidth, previewHeight) = GetPreviewTextureSize(cameraWidth, cameraHeight, rotation);
            previewBuffer = new NativeArray<byte>(previewWidth * previewHeight * 4, Allocator.Persistent);
            texture = new Texture2D(previewWidth, previewHeight, TextureFormat.RGBA32, false);
            humanTexture = capabilities.HasFlag(Capabilities.HumanTexture) ?
                new Texture2D(previewWidth, previewHeight, TextureFormat.RGBA32, false) :
                null;
            // Start running
            _device.StartRunning(OnCameraPixelBuffer);
            VideoKitEvents.Instance.onFrame += OnFrame;
        }

        /// <summary>
        /// Stop the camera preview.
        /// </summary>
        public void StopRunning () {
            if (VideoKitEvents.OptionalInstance != null)
                VideoKitEvents.OptionalInstance.onFrame -= OnFrame;
            _device?.StopRunning();
            Texture2D.Destroy(texture);
            Texture2D.Destroy(humanTexture);
            if (previewBuffer.IsCreated)
                previewBuffer.Dispose();
            previewBuffer = default;
            texture = null;
            humanTexture = null;
            rotation = 0;
        }
        #endregion


        #region --Operations--
        private CameraDevice[]? devices;
        private CameraDevice? _device;
        private NativeArray<byte> previewBuffer;
        private readonly object previewFence = new object();
        private static readonly List<RuntimePlatform> OrientationSupport = new List<RuntimePlatform> {
            RuntimePlatform.Android,
            RuntimePlatform.IPhonePlayer
        };
        public const string HumanTextureTag = @"@videokit/human-texture";

        private void Awake () {
            if (playOnAwake)
                StartRunning();
        }

        private unsafe void OnCameraPixelBuffer (PixelBuffer srcBuffer) {
            // Copy
            var (width, height) = GetPreviewTextureSize(srcBuffer.width, srcBuffer.height, rotation);
            lock (previewFence) {
                using var dstBuffer = new PixelBuffer(
                    width,
                    height,
                    PixelBuffer.Format.RGBA8888,
                    (byte*)previewBuffer.GetUnsafePtr()
                );
                srcBuffer.CopyTo(dstBuffer, rotation: rotation);
            }
            // Invoke event
            OnPixelBuffer?.Invoke(srcBuffer);
        }

        private void OnFrame () {
            // Preview texture
            lock (previewFence)
                texture!.GetRawTextureData<byte>().CopyFrom(previewBuffer);
            texture.Apply();
            // Human texture
            if (capabilities.HasFlag(Capabilities.HumanTexture)) {                
                var prediction = fxn.Predictions.Create(
                    tag: HumanTextureTag,
                    inputs: new () { ["image"] = texture.ToImage() }
                ).Result.Throw();
                var image = (Image)prediction.results![0]!;
                image.ToTexture(humanTexture);
            }
            // Invoke event
            OnCameraFrame?.Invoke();
        }

        private void OnDestroy () => StopRunning();

        private CameraDevice? GetDefaultCameraDevice (Facing facing) {
            // Check that devices have been discovered
            if (devices == null)
                return null;
            // Get fallback device
            var fallback = !facing.HasFlag(Facing.RequireUser);
            var fallbackDevice = fallback ? devices.FirstOrDefault() : null;
            // Find device
            var frontFacing = !facing.HasFlag(Facing.PreferWorld);
            var device = devices.FirstOrDefault(d => frontFacing && d.frontFacing) ?? fallbackDevice;
            // Return
            return device;
        }
        #endregion


        #region --Utilties--
        private static Function fxn => VideoKitClient.Instance!.fxn;

        private static PixelBuffer.Rotation GetPreviewRotation (ScreenOrientation orientation) => orientation switch {
            var _ when !OrientationSupport.Contains(Application.platform)   => PixelBuffer.Rotation._0,
            ScreenOrientation.LandscapeLeft                                 => PixelBuffer.Rotation._0,
            ScreenOrientation.Portrait                                      => PixelBuffer.Rotation._90,
            ScreenOrientation.LandscapeRight                                => PixelBuffer.Rotation._180,
            ScreenOrientation.PortraitUpsideDown                            => PixelBuffer.Rotation._270,
            _                                                               => PixelBuffer.Rotation._0
        };

        private static (int width, int height) GetResolutionFrameSize (Resolution resolution) => resolution switch {
            Resolution.Lowest       => (176, 144),
            Resolution._640x480     => (640, 480),
            Resolution._1280x720    => (1280, 720),
            Resolution._1920x1080   => (1920, 1080),
            Resolution._2560x1440   => (2560, 1440),
            Resolution._3840x2160   => (3840, 2160),
            Resolution.Highest      => (5120, 2880),
            _                       => (1280, 720),
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (int width, int height) GetPreviewTextureSize (
            int width,
            int height,
            PixelBuffer.Rotation rotation
        ) => rotation == PixelBuffer.Rotation._90 || rotation == PixelBuffer.Rotation._270 ?
            (height, width) :
            (width, height);
        #endregion
    }
}