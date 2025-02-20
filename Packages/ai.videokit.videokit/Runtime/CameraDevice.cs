/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit {

    using AOT;
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using UnityEngine;
    using Internal;
    using MediaDeviceFlags = Internal.VideoKit.MediaDeviceFlags;
    using Status = Internal.VideoKit.Status;

    /// <summary>
    /// Camera device.
    /// </summary>
    public sealed class CameraDevice : MediaDevice {

        #region --Types--
        /// <summary>
        /// Exposure mode.
        /// </summary>
        public enum ExposureMode : int {
            /// <summary>
            /// Continuous auto exposure.
            /// </summary>
            Continuous  = 0,
            /// <summary>
            /// Locked auto exposure.
            /// </summary>
            Locked      = 1,
            /// <summary>
            /// Manual exposure.
            /// </summary>
            Manual      = 2
        }

        /// <summary>
        /// Photo flash mode.
        /// </summary>
        public enum FlashMode : int {
            /// <summary>
            /// Never use flash.
            /// </summary>
            Off     = 0,
            /// <summary>
            /// Always use flash.
            /// </summary>
            On      = 1,
            /// <summary>
            /// Let the sensor detect if it needs flash.
            /// </summary>
            Auto    = 2
        }

        /// <summary>
        /// Focus mode.
        /// </summary>
        public enum FocusMode : int {
            /// <summary>
            /// Continuous autofocus.
            /// </summary>
            Continuous  = 0,
            /// <summary>
            /// Locked focus.
            /// </summary>
            Locked      = 1,
        }

        /// <summary>
        /// Torch mode.
        /// </summary>
        public enum TorchMode : int {
            /// <summary>
            /// Disabled torch.
            /// </summary>
            Off     = 0,
            /// <summary>
            /// Maximum supported torch level.
            /// </summary>
            Maximum = 100
        }

        /// <summary>
        /// Video stabilization mode.
        /// </summary>
        public enum VideoStabilizationMode : int {
            /// <summary>
            /// Disabled video stabilization.
            /// </summary>
            Off         = 0,
            /// <summary>
            /// Standard video stabilization.
            /// </summary>
            Standard    = 1
        }

        /// <summary>
        /// White balance mode.
        /// </summary>
        public enum WhiteBalanceMode : int {
            /// <summary>
            /// Continuous auto white balance.
            /// </summary>
            Continuous  = 0,
            /// <summary>
            /// Locked auto white balance.
            /// </summary>
            Locked      = 1,
        }
        #endregion


        #region --Properties--
        /// <summary>
        /// Whether this camera is front facing.
        /// </summary>
        public bool frontFacing => 
            device.GetMediaDeviceFlags(out var flags).Throw() == Status.Ok &&
            flags.HasFlag(MediaDeviceFlags.FrontFacing);

        /// <summary>
        /// Whether setting the flash mode for photo capture is supported.
        /// </summary>
        public bool flashSupported =>
            device.GetMediaDeviceFlags(out var flags).Throw() == Status.Ok &&
            flags.HasFlag(MediaDeviceFlags.Flash);

        /// <summary>
        /// Whether setting the torch level is supported.
        /// </summary>
        public bool torchSupported =>
            device.GetMediaDeviceFlags(out var flags).Throw() == Status.Ok &&
            flags.HasFlag(MediaDeviceFlags.Torch);

        /// <summary>
        /// Whether setting the exposure point is supported.
        /// </summary>
        public bool exposurePointSupported =>
            device.GetMediaDeviceFlags(out var flags).Throw() == Status.Ok &&
            flags.HasFlag(MediaDeviceFlags.ExposurePoint);

        /// <summary>
        /// Whether setting the focus point is supported.
        /// </summary>
        public bool focusPointSupported =>
            device.GetMediaDeviceFlags(out var flags).Throw() == Status.Ok &&
            flags.HasFlag(MediaDeviceFlags.FocusPoint);

        /// <summary>
        /// Whether depth streaming is supported.
        /// </summary>
        public bool depthStreamingSupported =>
            device.GetMediaDeviceFlags(out var flags).Throw() == Status.Ok &&
            flags.HasFlag(MediaDeviceFlags.Depth);

        /// <summary>
        /// Field of view in degrees.
        /// </summary>
        public (float width, float height) fieldOfView => device.GetCameraDeviceFieldOfView(out var x, out var y) == Status.Ok ? (x, y) : default;

        /// <summary>
        /// Exposure bias range in EV.
        /// </summary>
        public (float min, float max) exposureBiasRange => device.GetCameraDeviceExposureBiasRange(out var min, out var max) == Status.Ok ? (min, max) : default;

        /// <summary>
        /// Exposure duration range in seconds.
        /// </summary>
        public (float min, float max) exposureDurationRange => device.GetCameraDeviceExposureDurationRange(out var min, out var max) == Status.Ok ? (min, max) : default;

        /// <summary>
        /// Sensor sensitivity range.
        /// </summary>
        public (float min, float max) ISORange => device.GetCameraDeviceISORange(out var min, out var max) == Status.Ok ? (min, max) : default;

        /// <summary>
        /// Zoom ratio range.
        /// </summary>
        public (float min, float max) zoomRange => device.GetCameraDeviceZoomRange(out var min, out var max) == Status.Ok ? (min, max) : (1f, 1f);

        /// <summary>
        /// Get or set the preview resolution.
        /// </summary>
        public (int width, int height) previewResolution {
            get => device.GetCameraDevicePreviewResolution(out var w, out var h).Throw() == Status.Ok ? (w, h) : default;
            set => device.SetCameraDevicePreviewResolution(value.width, value.height);
        }

        /// <summary>
        /// Get or set the photo resolution.
        /// </summary>
        public (int width, int height) photoResolution {
            get => device.GetCameraDevicePhotoResolution(out var w, out var h) == Status.Ok ? (w, h) : default;
            set => device.SetCameraDevicePhotoResolution(value.width, value.height); // don't throw
        }

        /// <summary>
        /// Get or set the preview framerate.
        /// </summary>
        public float frameRate {
            get => device.GetCameraDeviceFrameRate(out var frameRate) == Status.Ok ? frameRate : default;
            set => device.SetCameraDeviceFrameRate(value);
        }

        /// <summary>
        /// Get or set the exposure mode.
        /// If the requested exposure mode is not supported, the camera device will ignore.
        /// </summary>
        public ExposureMode exposureMode {
            get => device.GetCameraDeviceExposureMode(out var mode) == Status.Ok ? mode : default;
            set => device.SetCameraDeviceExposureMode(value).Throw();
        }

        /// <summary>
        /// Get or set the exposure bias.
        /// This value must be in the range returned by `exposureRange`.
        /// </summary>
        public float exposureBias {
            get => device.GetCameraDeviceExposureBias(out var bias) == Status.Ok ? bias : default;
            set => device.SetCameraDeviceExposureBias(value).Throw();
        }

        /// <summary>
        /// Get or set the current exposure duration in seconds.
        /// </summary>
        public float exposureDuration => device.GetCameraDeviceExposureDuration(out var duration) == Status.Ok ? duration : default;

        /// <summary>
        /// Get or set the current exposure sensitivity.
        /// </summary>
        public float ISO => device.GetCameraDeviceISO(out var ISO) == Status.Ok ? ISO : default;

        /// <summary>
        /// Get or set the photo flash mode.
        /// </summary>
        public FlashMode flashMode {
            get => device.GetCameraDeviceFlashMode(out var mode) == Status.Ok ? mode : default;
            set => device.SetCameraDeviceFlashMode(value).Throw();
        }

        /// <summary>
        /// Get or set the focus mode.
        /// </summary>
        public FocusMode focusMode {
            get => device.GetCameraDeviceFocusMode(out var mode) == Status.Ok ? mode : default;
            set => device.SetCameraDeviceFocusMode(value).Throw();
        }

        /// <summary>
        /// Get or set the torch mode.
        /// </summary>
        public TorchMode torchMode {
            get => device.GetCameraDeviceTorchMode(out var mode) == Status.Ok ? mode : default;
            set => device.SetCameraDeviceTorchMode(value).Throw();
        }

        /// <summary>
        /// Get or set the white balance mode.
        /// </summary>
        public WhiteBalanceMode whiteBalanceMode {
            get => device.GetCameraDeviceWhiteBalanceMode(out var mode) == Status.Ok ? mode : default;
            set => device.SetCameraDeviceWhiteBalanceMode(value).Throw();
        }

        /// <summary>
        /// Get or set the video stabilization mode.
        /// </summary>
        public VideoStabilizationMode videoStabilizationMode {
            get => device.GetCameraDeviceVideoStabilizationMode(out var mode) == Status.Ok ? mode : default;
            set => device.SetCameraDeviceVideoStabilizationMode(value).Throw();
        }

        /// <summary>
        /// Get or set the zoom ratio.
        /// This value must be in the range returned by `zoomRange`.
        /// </summary>
        public float zoomRatio {
            get => device.GetCameraDeviceZoomRatio(out var mode) == Status.Ok ? mode : default;
            set => device.SetCameraDeviceZoomRatio(value).Throw();
        }
        #endregion


        #region --Controls--
        /// <summary>
        /// Check if a given exposure mode is supported by the camera device.
        /// </summary>
        /// <param name="mode">Exposure mode.</param>
        public bool IsExposureModeSupported (ExposureMode mode) => mode switch {
            ExposureMode.Continuous => device.GetMediaDeviceFlags(out var flags).Throw() == Status.Ok && flags.HasFlag(MediaDeviceFlags.ExposureContinuous),
            ExposureMode.Locked     => device.GetMediaDeviceFlags(out var flags).Throw() == Status.Ok && flags.HasFlag(MediaDeviceFlags.ExposureLock),
            ExposureMode.Manual     => device.GetMediaDeviceFlags(out var flags).Throw() == Status.Ok && flags.HasFlag(MediaDeviceFlags.ExposureManual),
            _                       => false
        };

        /// <summary>
        /// Check if a given focus mode is supported by the camera device.
        /// </summary>
        /// <param name="mode">Focus mode.</param>
        public bool IsFocusModeSupported (FocusMode mode) => mode switch {
            FocusMode.Continuous    => device.GetMediaDeviceFlags(out var flags).Throw() == Status.Ok && flags.HasFlag(MediaDeviceFlags.FocusContinuous),
            FocusMode.Locked        => device.GetMediaDeviceFlags(out var flags).Throw() == Status.Ok && flags.HasFlag(MediaDeviceFlags.FocusLock),
            _                       => false,
        };

        /// <summary>
        /// Check if a given white balance mode is supported by the camera device.
        /// </summary>
        /// <param name="mode">White balance mode.</param>
        public bool IsWhiteBalanceModeSupported (WhiteBalanceMode mode) => mode switch {
            WhiteBalanceMode.Continuous => device.GetMediaDeviceFlags(out var flags).Throw() == Status.Ok && flags.HasFlag(MediaDeviceFlags.WhiteBalanceContinuous),
            WhiteBalanceMode.Locked     => device.GetMediaDeviceFlags(out var flags).Throw() == Status.Ok && flags.HasFlag(MediaDeviceFlags.WhiteBalanceLock),
            _                           => false
        };

        /// <summary>
        /// Check if a given video stabilization mode is supported by the camera device.
        /// </summary>
        /// <param name="mode">Video stabilization mode.</param>
        public bool IsVideoStabilizationModeSupported (VideoStabilizationMode mode) => mode switch {
            VideoStabilizationMode.Off      => true,
            VideoStabilizationMode.Standard => device.GetMediaDeviceFlags(out var flags).Throw() == Status.Ok && flags.HasFlag(MediaDeviceFlags.VideoStabilization),
            _                               => false
        };

        /// <summary>
        /// Set manual exposure.
        /// </summary>
        /// <param name="duration">Exposure duration in seconds. MUST be in `exposureDurationRange`.</param>
        /// <param name="ISO">Sensor sensitivity ISO value. MUST be in `ISORange`.</param>
        public void SetExposureDuration (float duration, float ISO) => device.SetCameraDeviceExposureDuration(duration, ISO).Throw();

        /// <summary>
        /// Set the exposure point of interest.
        /// The point is specified in normalized coordinates in range [0.0, 1.0].
        /// </summary>
        /// <param name="x">Normalized x coordinate.</param>
        /// <param name="y">Normalized y coordinate.</param>
        public void SetExposurePoint (float x, float y) => device.SetCameraDeviceExposurePoint(x, y).Throw();

        /// <summary>
        /// Set the focus point of interest.
        /// The point is specified in normalized coordinates in range [0.0, 1.0].
        /// </summary>
        /// <param name="x">Normalized x coordinate.</param>
        /// <param name="y">Normalized y coordinate.</param>
        public void SetFocusPoint (float x, float y) => device.SetCameraDeviceFocusPoint(x, y).Throw();
        #endregion


        #region --Streaming--
        /// <summary>
        /// Start the camera preview.
        /// </summary>
        /// <param name="handler">Delegate to receive preview image frames. Note that this delegate is invoked on a dedicated thread.</param>
        public void StartRunning (Action<PixelBuffer> handler) => StartRunning((IntPtr sampleBuffer) => {
            handler(new PixelBuffer(sampleBuffer));
        });

        /// <summary>
        /// Capture a photo.
        /// </summary>
        /// <param name="handler">Delegate to receive high-resolution photo. Note that this delegate is invoked on a dedicated thread.</param>
        public void CapturePhoto (Action<PixelBuffer> handler) {
            var handle = GCHandle.Alloc(handler, GCHandleType.Normal);
            device.CapturePhoto(OnCapturePhoto, (IntPtr)handle).Throw();
        }
        #endregion


        #region --Discovery--
        /// <summary>
        /// Check the current camera permission status.
        /// </summary>
        /// <param name="request">Request permissions if the user has not yet been asked.</param>
        /// <returns>Camera permission status.</returns>
        public static Task<PermissionStatus> CheckPermissions (bool request = true) => CheckPermissions(
            VideoKit.PermissionType.Camera,
            request
        );

        /// <summary>
        /// Discover available camera devices.
        /// </summary>
        public static async Task<CameraDevice[]> Discover () {
            // Check session
            await VideoKitClient.Instance!.CheckSession();
            // Discover
            var tcs = new TaskCompletionSource<CameraDevice[]>();
            var handle = GCHandle.Alloc(tcs, GCHandleType.Normal);
            try {
                VideoKit.DiscoverCameraDevices(OnDiscoverDevices, (IntPtr)handle).Throw();
                return await tcs.Task;
            } catch {
                handle.Free();
                throw;
            }
        }
        #endregion


        #region --Operations--

        private int priority { // #24
            get {
                var order = 0;
                if (!defaultForMediaType)
                    order += 1;
                if (location == Location.External)
                    order += 10;
                if (location == Location.Unknown)
                    order += 100;
                return order;
            }
        }

        internal CameraDevice (IntPtr device, bool strong = true) : base(device, strong: strong) { }

        public override string ToString () => $"CameraDevice(uniqueId=\"{uniqueId}\", name=\"{name}\")";

        [MonoPInvokeCallback(typeof(VideoKit.MediaDeviceDiscoveryHandler))]
        private static unsafe void OnDiscoverDevices (IntPtr context, IntPtr devices, int count) {            
            try {
                // Check
                if (!VideoKit.IsAppDomainLoaded)
                    return;
                // Get tcs
                var handle = (GCHandle)context;
                var tcs = handle.Target as TaskCompletionSource<CameraDevice[]>;
                handle.Free();
                // Complete
                var cameras = Enumerable
                    .Range(0, count)
                    .Select(idx => ((IntPtr*)devices)[idx])
                    .Select(device => new CameraDevice(device, strong: true))
                    .OrderBy(device => device.priority)
                    .ToArray();
                tcs?.SetResult(cameras);
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        [MonoPInvokeCallback(typeof(VideoKit.SampleBufferHandler))]
        private static unsafe void OnCapturePhoto (IntPtr context, IntPtr sampleBuffer) {
            try {
                // Check
                if (!VideoKit.IsAppDomainLoaded)
                    return;
                // Get handler
                var handle = (GCHandle)context;
                var handler = handle.Target as Action<PixelBuffer>;
                handle.Free();
                // Invoke
                handler?.Invoke(new PixelBuffer(sampleBuffer));
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }
        #endregion
    }
}