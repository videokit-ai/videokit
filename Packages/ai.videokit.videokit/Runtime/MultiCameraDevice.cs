/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
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

    /// <summary>
    /// Multi camera device.
    /// This is a device that allows for streaming pixel buffers from multiple camera devices simultaneously.
    /// </summary>
    internal sealed class MultiCameraDevice : MediaDevice { // INCOMPLETE

        #region --Client API--
        /// <summary>
        /// System pressure level.
        /// https://developer.apple.com/documentation/avfoundation/avcapturesystempressurelevel
        /// </summary>
        public enum SystemPressureLevel : int {
            Nominal     = 0,
            Fair        = 1,
            Serious     = 2,
            Critical    = 3,
            Shutdown    = 4
        }

        /// <summary>
        /// Camera devices that comprise this multi-camera device.
        /// </summary>
        public readonly CameraDevice[] cameras;

        /// <summary>
        /// Get the multi-camera device normalized hardware cost in range [0.0, 1.0].
        /// </summary>
        public float hardwareCost => 0f;

        /// <summary>
        /// Get the multi-camera device normalized system pressure cost in range [0.0, 1.0].
        /// </summary>
        public float systemPressureCost => 0f;

        /// <summary>
        /// Event raised when the system pressure level changes.
        /// </summary>
        public event Action<SystemPressureLevel>? onSystemPressureChange;

        /// <summary>
        /// Start running.
        /// </summary>
        /// <param name="handler">Delegate to receive preview frames.</param>
        public void StartRunning (Action<PixelBuffer, CameraDevice> handler) { // INCOMPLETE

        }

        /// <summary>
        /// Check if a given camera in the multi-camera device is streaming.
        /// </summary>
        /// <param name="cameraDevice">Camera device. MUST be a member of this multi-camera device.</param>
        public bool IsPaused (CameraDevice camera) => false; // INCOMPLETE

        /// <summary>
        /// Pause or resume the camera preview from a given camera in the multi-camera device.
        /// </summary>
        /// <param name="cameraDevice">Camera device. MUST be a member of this multi-camera device.</param>
        /// <param name="active">Whether the camera preview should be active.</param>
        public void SetPaused (CameraDevice camera, bool paused) { // INCOMPLETE
            
        }
        #endregion


        #region --Discovery--
        /// <summary>
        /// Discover available multi-camera devices.
        /// </summary>
        public static async Task<MultiCameraDevice[]> Discover () { // INCOMPLETE
            // Check session
            await VideoKitClient.Instance!.CheckSession();
            // Discover
            var tcs = new TaskCompletionSource<MultiCameraDevice[]>();
            var handle = GCHandle.Alloc(tcs, GCHandleType.Normal);
            try {
                //VideoKit.DiscoverCameraDevices(OnDiscoverDevices, (IntPtr)handle).Throw();
                return await tcs.Task;
            } catch {
                handle.Free();
                throw;
            }
        }
        #endregion


        #region --Operations--

        internal MultiCameraDevice (IntPtr device) : base(device) { // INCOMPLETE
            this.cameras = new CameraDevice[0];
        }

        [MonoPInvokeCallback(typeof(VideoKit.MultiCameraDeviceSystemPressureHandler))]
        private static void OnSystemPressureChange (IntPtr context, SystemPressureLevel level) {
            var handle = (GCHandle)context;
            var device = handle.Target as MultiCameraDevice; // weak
            device?.onSystemPressureChange?.Invoke(level);
        }

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
                    .Select(idx => new MultiCameraDevice(((IntPtr*)devices)[idx]))
                    .ToArray();
                tcs?.SetResult(cameras);
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }
        #endregion
    }
}