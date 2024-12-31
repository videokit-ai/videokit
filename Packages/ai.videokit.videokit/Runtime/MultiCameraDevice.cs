/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit {
    
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using UnityEngine;
    using AOT;
    using Internal;
    using Status = Internal.VideoKit.Status;

    /// <summary>
    /// Multi camera device.
    /// This is a device that allows for streaming pixel buffers from multiple camera devices simultaneously.
    /// </summary>
    public sealed class MultiCameraDevice : MediaDevice {

        #region --Client API--
        /// <summary>
        /// Camera devices that comprise this multi-camera device.
        /// </summary>
        public readonly CameraDevice[] cameras;

        /// <summary>
        /// Get the multi-camera device normalized hardware cost in range [0.0, 1.0].
        /// </summary>
        public float? hardwareCost => device.GetMultiCameraDeviceHardwareCost(out var cost) == Status.Ok ? cost : default;

        /// <summary>
        /// Get the multi-camera device normalized system pressure cost in range [0.0, 1.0].
        /// </summary>
        public float? systemPressureCost => device.GetMultiCameraDeviceSystemPressureCost(out var cost) == Status.Ok ? cost : default;

        /// <summary>
        /// Event raised when the system pressure level changes.
        /// NOTE: This event is invoked on a dedicated camera thread, not on the Unity main thread.
        /// </summary>
        public event Action? onSystemPressureChange;

        /// <summary>
        /// Check whether a given camera in the multi-camera device is running.
        /// </summary>
        /// <param name="camera">Camera device. MUST be a member of this multi-camera device.</param>
        public bool IsRunning (CameraDevice camera) => device
            .GetMultiCameraDeviceIsRunning(camera, out var running)
            .Throw() == Status.Ok && running;

        /// <summary>
        /// Start running.
        /// </summary>
        /// <param name="handler">Delegate to receive preview frames.</param>
        public void StartRunning (Action<CameraDevice, PixelBuffer> handler) => StartRunning((IntPtr sampleBuffer) => {
            sampleBuffer.GetMultiCameraPixelBufferCamera(out var rawCamera).Throw();
            var camera = cameras.First(cam => cam == rawCamera);
            var pixelBuffer = new PixelBuffer(sampleBuffer);
            handler(camera, pixelBuffer);
        });

        /// <summary>
        /// Start the camera preview from a given camera in the multi-camera device.
        /// </summary>
        /// <param name="camera">Camera device. MUST be a member of this multi-camera device.</param>
        public void StartRunning (CameraDevice camera) => device.StartRunning(camera).Throw();

        /// <summary>
        /// Stop the camera preview from a specific camera in the multi-camera device.
        /// </summary>
        /// <param name="camera">Camera device. MUST be a member of this multi-camera device.</param>
        public void StopRunning (CameraDevice camera) => device.StopRunning(camera).Throw();
        #endregion


        #region --Discovery--
        /// <summary>
        /// Check the current camera permission status.
        /// This is an alias for `CameraDevice.CheckPermissions`.
        /// </summary>
        /// <param name="request">Request permissions if the user has not yet been asked.</param>
        /// <returns>Camera permission status.</returns>
        public static Task<PermissionStatus> CheckPermissions (bool request = true) => CameraDevice.CheckPermissions(request);

        /// <summary>
        /// Discover available multi-camera devices.
        /// </summary>
        public static async Task<MultiCameraDevice[]> Discover () {
            // Check session
            await VideoKitClient.Instance!.CheckSession();
            // Discover
            var tcs = new TaskCompletionSource<MultiCameraDevice[]>();
            var handle = GCHandle.Alloc(tcs, GCHandleType.Normal);
            try {
                VideoKit.DiscoverMultiCameraDevices(OnDiscoverDevices, (IntPtr)handle).Throw();
                return await tcs.Task;
            } catch {
                handle.Free();
                throw;
            }
        }
        #endregion


        #region --Operations--

        internal MultiCameraDevice (IntPtr device) : base(device) {
            device.GetMultiCameraDeviceCameraCount(out var count).Throw();
            this.cameras = Enumerable.Range(0, count)
                .Select(idx => device.GetMultiCameraDeviceCamera(idx, out var camera).Throw() == Status.Ok ? camera : default)
                .Select(camera => new CameraDevice(camera, strong: false))
                .ToArray();
            device.SetMultiCameraDeviceSystemPressureChangeHandler(OnSystemPressureChange, (IntPtr)weakSelf);
        }

        public override string ToString () => $"MultiCameraDevice(uniqueId=\"{uniqueId}\", name=\"{name}\")";

        [MonoPInvokeCallback(typeof(VideoKit.MediaDeviceDiscoveryHandler))]
        private static unsafe void OnDiscoverDevices (IntPtr context, IntPtr devices, int count) {
            try {
                // Check
                if (!VideoKit.IsAppDomainLoaded)
                    return;
                // Get tcs
                var handle = (GCHandle)context;
                var tcs = handle.Target as TaskCompletionSource<MultiCameraDevice[]>;
                handle.Free();
                // Complete task
                var cameras = Enumerable
                    .Range(0, count)
                    .Select(idx => ((IntPtr*)devices)[idx])
                    .Select(device => new MultiCameraDevice(device))
                    .ToArray();
                tcs?.SetResult(cameras);
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        [MonoPInvokeCallback(typeof(VideoKit.MultiCameraDeviceSystemPressureHandler))]
        private static void OnSystemPressureChange (IntPtr context) {
            var handle = (GCHandle)context;
            var device = handle.Target as MultiCameraDevice; // weak
            device?.onSystemPressureChange?.Invoke();
        }
        #endregion
    }
}