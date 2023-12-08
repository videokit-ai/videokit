/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Devices {

    using AOT;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using Internal;
    using Utilities;
    using DeviceFlags = Internal.VideoKit.DeviceFlags;
    
    /// <summary>
    /// Hardware camera device.
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
        /// Is this camera front facing?
        /// </summary>
        public bool frontFacing => device.Flags().HasFlag(DeviceFlags.FrontFacing);

        /// <summary>
        /// Is flash supported for photo capture?
        /// </summary>
        public bool flashSupported => device.Flags().HasFlag(DeviceFlags.Flash);

        /// <summary>
        /// Is torch supported?
        /// </summary>
        public bool torchSupported => device.Flags().HasFlag(DeviceFlags.Torch);
        
        /// <summary>
        /// Is setting the exposure point supported?
        /// </summary>
        public bool exposurePointSupported => device.Flags().HasFlag(DeviceFlags.ExposurePoint);

        /// <summary>
        /// Is setting the focus point supported?
        /// </summary>
        public bool focusPointSupported => device.Flags().HasFlag(DeviceFlags.FocusPoint);

        /// <summary>
        /// Is depth streaming supported?
        /// </summary>
        internal bool depthStreamingSupported => device.Flags().HasFlag(DeviceFlags.Depth);

        /// <summary>
        /// Field of view in degrees.
        /// </summary>
        public (float width, float height) fieldOfView {
            get {
                device.FieldOfView(out var width, out var height);
                return (width, height);
            }
        }

        /// <summary>
        /// Exposure bias range in EV.
        /// </summary>
        public (float min, float max) exposureBiasRange {
            get {
                device.ExposureBiasRange(out var min, out var max);
                return (min, max);
            }
        }

        /// <summary>
        /// Exposure duration range in seconds.
        /// </summary>
        public (float min, float max) exposureDurationRange {
            get {
                device.ExposureDurationRange(out var min, out var max);
                return (min, max);
            }
        }

        /// <summary>
        /// Sensor sensitivity range.
        /// </summary>
        public (float min, float max) ISORange {
            get {
                device.ISORange(out var min, out var max);
                return (min, max);
            }
        }

        /// <summary>
        /// Zoom ratio range.
        /// </summary>
        public (float min, float max) zoomRange {
            get {
                device.ZoomRange(out var min, out var max);
                return (min, max);
            }
        }

        /// <summary>
        /// Get or set the preview resolution.
        /// </summary>
        public (int width, int height) previewResolution {
            get { device.PreviewResolution(out var width, out var height); return (width, height); }
            set => device.SetPreviewResolution(value.width, value.height);
        }

        /// <summary>
        /// Get or set the photo resolution.
        /// </summary>
        public (int width, int height) photoResolution {
            get { device.PhotoResolution(out var width, out var height); return (width, height); }
            set => device.SetPhotoResolution(value.width, value.height);
        }

        /// <summary>
        /// Get or set the preview framerate.
        /// </summary>
        public int frameRate {
            get => device.FrameRate();
            set => device.SetFrameRate(value);
        }

        /// <summary>
        /// Get or set the exposure mode.
        /// If the requested exposure mode is not supported, the camera device will ignore.
        /// </summary>
        public ExposureMode exposureMode {
            get => device.ExposureMode();
            set => device.SetExposureMode(value);
        }

        /// <summary>
        /// Get or set the exposure bias.
        /// This value must be in the range returned by `exposureRange`.
        /// </summary>
        public float exposureBias {
            get => device.ExposureBias();
            set => device.SetExposureBias(value);
        }

        /// <summary>
        /// Get or set the current exposure duration in seconds.
        /// </summary>
        public float exposureDuration => device.ExposureDuration();

        /// <summary>
        /// Get or set the current exposure sensitivity.
        /// </summary>
        public float ISO => device.ISO();

        /// <summary>
        /// Get or set the photo flash mode.
        /// </summary>
        public FlashMode flashMode {
            get => device.FlashMode();
            set => device.SetFlashMode(value);
        }

        /// <summary>
        /// Get or set the focus mode.
        /// </summary>
        public FocusMode focusMode {
            get => device.FocusMode();
            set => device.SetFocusMode(value);
        }

        /// <summary>
        /// Get or set the torch mode.
        /// </summary>
        public TorchMode torchMode {
            get => device.TorchMode();
            set => device.SetTorchMode(value);
        }

        /// <summary>
        /// Get or set the white balance mode.
        /// </summary>
        public WhiteBalanceMode whiteBalanceMode {
            get => device.WhiteBalanceMode();
            set => device.SetWhiteBalanceMode(value);
        }

        /// <summary>
        /// Get or set the video stabilization mode.
        /// </summary>
        public VideoStabilizationMode videoStabilizationMode {
            get => device.VideoStabilizationMode();
            set => device.SetVideoStabilizationMode(value);
        }

        /// <summary>
        /// Get or set the zoom ratio.
        /// This value must be in the range returned by `zoomRange`.
        /// </summary>
        public float zoomRatio {
            get => device.ZoomRatio();
            set => device.SetZoomRatio(value);
        }
        #endregion


        #region --Controls--
        /// <summary>
        /// Check if a given exposure mode is supported by the camera device.
        /// </summary>
        /// <param name="mode">Exposure mode.</param>
        public bool ExposureModeSupported (ExposureMode mode) => mode switch {
            ExposureMode.Continuous => device.Flags().HasFlag(DeviceFlags.ExposureContinuous),
            ExposureMode.Locked     => device.Flags().HasFlag(DeviceFlags.ExposureLock),
            ExposureMode.Manual     => device.Flags().HasFlag(DeviceFlags.ExposureManual),
            _                       => false
        };

        /// <summary>
        /// Check if a given focus mode is supported by the camera device.
        /// </summary>
        /// <param name="mode">Focus mode.</param>
        public bool FocusModeSupported (FocusMode mode) => mode switch {
            FocusMode.Continuous    => device.Flags().HasFlag(DeviceFlags.FocusContinuous),
            FocusMode.Locked        => device.Flags().HasFlag(DeviceFlags.FocusLock),
            _                       => false,
        };

        /// <summary>
        /// Check if a given white balance mode is supported by the camera device.
        /// </summary>
        /// <param name="mode">White balance mode.</param>
        public bool WhiteBalanceModeSupported (WhiteBalanceMode mode) => mode switch {
            WhiteBalanceMode.Continuous => device.Flags().HasFlag(DeviceFlags.WhiteBalanceContinuous),
            WhiteBalanceMode.Locked     => device.Flags().HasFlag(DeviceFlags.WhiteBalanceLock),
            _                           => false
        };

        /// <summary>
        /// Check if a given video stabilization mode is supported by the camera device.
        /// </summary>
        /// <param name="mode">Video stabilization mode.</param>
        public bool VideoStabilizationModeSupported (VideoStabilizationMode mode) => mode switch {
            VideoStabilizationMode.Off      => true,
            VideoStabilizationMode.Standard => device.Flags().HasFlag(DeviceFlags.VideoStabilization),
            _                               => false
        };

        /// <summary>
        /// Set manual exposure.
        /// </summary>
        /// <param name="duration">Exposure duration in seconds. MUST be in `exposureDurationRange`.</param>
        /// <param name="ISO">Sensor sensitivity ISO value. MUST be in `ISORange`.</param>
        public void SetExposureDuration (float duration, float ISO) => device.SetExposureDuration(duration, ISO);

        /// <summary>
        /// Set the exposure point of interest.
        /// The point is specified in normalized coordinates in range [0.0, 1.0].
        /// </summary>
        /// <param name="x">Normalized x coordinate.</param>
        /// <param name="y">Normalized y coordinate.</param>
        public void SetExposurePoint (float x, float y) => device.SetExposurePoint(x, y);

        /// <summary>
        /// Set the focus point of interest.
        /// The point is specified in normalized coordinates in range [0.0, 1.0].
        /// </summary>
        /// <param name="x">Normalized x coordinate.</param>
        /// <param name="y">Normalized y coordinate.</param>
        public void SetFocusPoint (float x, float y) => device.SetFocusPoint(x, y);
        #endregion


        #region --Streaming--
        /// <summary>
        /// Start the camera preview.
        /// </summary>
        /// <param name="handler">Delegate to receive preview image frames.</param>
        public void StartRunning (Action<CameraImage> handler) {
            Action<IntPtr> wrapper = sampleBuffer => handler?.Invoke(new CameraImage(this, sampleBuffer));
            previewHandle = GCHandle.Alloc(wrapper, GCHandleType.Normal);
            lifecycleHelper = LifecycleHelper.Create();
            lifecycleHelper.onQuit += StopRunning;
            try {
                device.StartRunning(OnPreviewImage, (IntPtr)previewHandle).CheckStatus();
            } catch (Exception) {
                previewHandle.Free();
                previewHandle = default;
                throw;
            }
        }

        /// <summary>
        /// Start the camera preview with depth streaming.
        /// </summary>
        /// <param name="handler">Delegate to receive preview image and depth frames.</param>
        internal void StartRunning (Action<CameraImage, CameraImage> handler) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Stop the camera preview.
        /// </summary>
        public override void StopRunning () {
            device.StopRunning().CheckStatus();
            lifecycleHelper?.Dispose();
            lifecycleHelper = default;
            if (previewHandle != default)
                previewHandle.Free();
            previewHandle = default;
        }

        /// <summary>
        /// Capture a photo.
        /// </summary>
        /// <param name="handler">Delegate to receive high-resolution photo.</param>
        public void CapturePhoto (Action<CameraImage> handler) {
            Action<IntPtr> wrapper = sampleBuffer => handler?.Invoke(new CameraImage(this, sampleBuffer));
            var handle = GCHandle.Alloc(wrapper, GCHandleType.Normal);
            device.CapturePhoto(OnPhotoImage, (IntPtr)handle);
        }
        #endregion


        #region --Discovery--
        /// <summary>
        /// Check the current camera permission status.
        /// </summary>
        /// <param name="request">Request permissions if the user has not yet been asked.</param>
        /// <returns>Camera permission status.</returns>
        public static Task<PermissionStatus> CheckPermissions (bool request = true) => CheckPermissions(VideoKit.PermissionType.Camera, request);

        /// <summary>
        /// Discover available camera devices.
        /// </summary>
        public static async Task<CameraDevice[]> Discover () {
            // Check session
            await VideoKitSettings.Instance.CheckSession();
            // Discover
            var devices = await DiscoverNative();
            // Return
            return devices;
        }
        #endregion


        #region --Operations--
        private GCHandle previewHandle;
        private LifecycleHelper lifecycleHelper;
        
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

        internal CameraDevice (IntPtr device) : base(device) { }

        public override string ToString () => $"camera:{uniqueID}";

        private static Task<CameraDevice[]> DiscoverNative () {
            // Discover
            var tcs = new TaskCompletionSource<CameraDevice[]>();
            var handle = GCHandle.Alloc(tcs, GCHandleType.Normal);
            try {
                VideoKit.DiscoverCameras(OnDiscoverCameras, (IntPtr)handle).CheckStatus();
            } catch (Exception ex) {
                handle.Free();
                tcs.SetException(ex);
            }
            // Return
            return tcs.Task;
        }

        [MonoPInvokeCallback(typeof(VideoKit.DeviceDiscoveryHandler))]
        private static unsafe void OnDiscoverCameras (IntPtr context, IntPtr devices, int count) {
            // Check
            if (!VideoKit.IsAppDomainLoaded)
                return;
            // Get tcs
            TaskCompletionSource<CameraDevice[]> tcs;
            try {
                var handle = (GCHandle)context;
                tcs = handle.Target as TaskCompletionSource<CameraDevice[]>;
                handle.Free();
            } catch (Exception ex) {
                Debug.LogException(ex);
                return;
            }
            // Invoke
            var cameras = Enumerable
                .Range(0, count)
                .Select(idx => new CameraDevice(((IntPtr*)devices)[idx]))
                .OrderBy(camera => camera.priority)
                .ToArray();
            tcs?.SetResult(cameras);
        }

        [MonoPInvokeCallback(typeof(VideoKit.SampleBufferHandler))]
        private static unsafe void OnPreviewImage (IntPtr context, IntPtr sampleBuffer) {
            // Check
            if (!VideoKit.IsAppDomainLoaded)
                return;
            try {
                // Invoke
                var handle = (GCHandle)context;
                var handler = handle.Target as Action<IntPtr>;
                handler?.Invoke(sampleBuffer);
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        [MonoPInvokeCallback(typeof(VideoKit.SampleBufferHandler))]
        private static unsafe void OnPhotoImage (IntPtr context, IntPtr sampleBuffer) {
            // Check
            if (!VideoKit.IsAppDomainLoaded)
                return;
            try {
                // Invoke
                var handle = (GCHandle)context;
                var handler = handle.Target as Action<IntPtr>;
                handle.Free();
                handler?.Invoke(sampleBuffer);
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }
        #endregion
    }
}