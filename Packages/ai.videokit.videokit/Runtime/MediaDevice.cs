/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit {

    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using AOT;
    using Internal;
    using MediaDeviceFlags = Internal.VideoKit.MediaDeviceFlags;
    using PermissionType = Internal.VideoKit.PermissionType;
    using Status = Internal.VideoKit.Status;

    /// <summary>
    /// Media device which provides media sample buffers.
    /// </summary>
    public abstract class MediaDevice {

        #region --Enumerations--
        /// <summary>
        /// Device location.
        /// </summary>
        public enum Location : int { // CHECK // Must match `VideoKit.h`
            /// <summary>
            /// Device type is unknown.
            /// </summary>
            Unknown = 0,
            /// <summary>
            /// Device is internal.
            /// </summary>
            Internal = 1 << 0,
            /// <summary>
            /// Device is external.
            /// </summary>
            External = 1 << 1,
        }

        /// <summary>
        /// Device permissions status.
        /// </summary>
        public enum PermissionStatus : int { // CHECK // Must match `VideoKit.h`
            /// <summary>
            /// User has not authorized or denied access to media device.
            /// </summary>
            Unknown = 0,
            /// <summary>
            /// User has denied access to media device.
            /// </summary>
            Denied = 2,
            /// <summary>
            /// User has authorized access to media device.
            /// </summary>
            Authorized = 3
        }
        #endregion


        #region --Properties--
        /// <summary>
        /// Device unique ID.
        /// </summary>
        public string uniqueId { get; protected set; }

        /// <summary>
        /// Display friendly device name.
        /// </summary>
        public string name { get; protected set; }

        /// <summary>
        /// Device location.
        /// </summary>
        public virtual Location location => device.GetMediaDeviceFlags(out var flags).Throw() == Status.Ok ? (Location)((int)flags & 0x3) : default;

        /// <summary>
        /// Device is the default device for its media type.
        /// </summary>
        public virtual bool defaultForMediaType => device.GetMediaDeviceFlags(out var flags).Throw() == Status.Ok ? flags.HasFlag(MediaDeviceFlags.Default) : default;
        #endregion


        #region --Events--
        /// <summary>
        /// Event raised when the device is disconnected.
        /// </summary>
        public event Action? onDisconnected;
        #endregion


        #region --Streaming--
        /// <summary>
        /// Whether the device is running.
        /// </summary>
        public virtual bool running => device.GetMediaDeviceIsRunning(out var running).Throw() == Status.Ok ? running : default;

        /// <summary>
        /// Stop running.
        /// </summary>
        public virtual void StopRunning () {
            if (running)
                device.StopRunning().Throw();
            if (streamHandle != default)
                streamHandle.Free();
            streamHandle = default;
        }
        #endregion


        #region --Operations--
        protected readonly IntPtr device;
        protected readonly GCHandle weakSelf;
        private readonly bool strong;
        private GCHandle streamHandle;

        internal MediaDevice (IntPtr device, bool strong = true) {
            this.device = device;
            this.strong = strong;
            this.weakSelf = GCHandle.Alloc(this, GCHandleType.Weak);
            // Cache UID
            var buffer = new StringBuilder(2048);
            device.GetMediaDeviceUniqueID(buffer, buffer.Capacity);
            this.uniqueId = buffer.ToString();
            // Cache name
            buffer.Clear();
            device.GetMediaDeviceName(buffer, buffer.Capacity);
            this.name = buffer.ToString();
            // Register handlers
            device.SetDisconnectHandler(OnDeviceDisconnect, (IntPtr)weakSelf);
        }

        ~MediaDevice () {
            if (strong)
                device.ReleaseMediaDevice();
            weakSelf.Free();
        }

        protected virtual void StartRunning (Action<IntPtr> handler) {
            streamHandle = GCHandle.Alloc(handler, GCHandleType.Normal);
            try {
                device.StartRunning(OnSampleBuffer, (IntPtr)streamHandle).Throw();
            } catch {
                streamHandle.Free();
                streamHandle = default;
                throw;
            }
        }

        protected static Task<PermissionStatus> CheckPermissions (PermissionType type, bool request) {
            var tcs = new TaskCompletionSource<PermissionStatus>();
            var handle = GCHandle.Alloc(tcs, GCHandleType.Normal);
            try {
                VideoKit.CheckPermissions(type, request, OnPermissionResult, (IntPtr)handle).Throw();
            } catch (Exception ex) {
                handle.Free();
                tcs.SetException(ex);
            }
            return tcs.Task;
        }

        public static implicit operator IntPtr (MediaDevice device) => device.device;
        #endregion


        #region --Callbacks--

        [MonoPInvokeCallback(typeof(VideoKit.MediaDeviceDisconnectHandler))]
        private static void OnDeviceDisconnect (IntPtr context, IntPtr _) {
            // Check
            if (!VideoKit.IsAppDomainLoaded)
                return;
            try {
                // Invoke
                var handle = (GCHandle)context;
                var device = handle.Target as MediaDevice;
                device?.onDisconnected?.Invoke();
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        [MonoPInvokeCallback(typeof(VideoKit.MediaDevicePermissionResultHandler))]
        private static void OnPermissionResult (IntPtr context, PermissionStatus status) {
            // Check
            if (!VideoKit.IsAppDomainLoaded)
                return;
            // Get tcs
            TaskCompletionSource<PermissionStatus>? tcs;
            try {
                var handle = (GCHandle)context;
                tcs = handle.Target as TaskCompletionSource<PermissionStatus>;
                handle.Free();
            } catch (Exception ex) {
                Debug.LogException(ex);
                return;
            }
            // Invoke
            tcs?.SetResult(status);
        }

        [MonoPInvokeCallback(typeof(VideoKit.SampleBufferHandler))]
        private static void OnSampleBuffer (IntPtr context, IntPtr sampleBuffer) {
            // Check
            if (!VideoKit.IsAppDomainLoaded)
                return;
            // Invoke
            try {
                var handle = (GCHandle)context;
                var handler = handle.Target as Action<IntPtr>;
                handler?.Invoke(sampleBuffer);
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }
        #endregion
    }
}