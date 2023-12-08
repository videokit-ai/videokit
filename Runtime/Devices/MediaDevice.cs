/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Devices {

    using AOT;
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Android;
    using Internal;
    using Utilities;
    using DeviceFlags = Internal.VideoKit.DeviceFlags;
    using PermissionType = Internal.VideoKit.PermissionType;

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
        public enum PermissionStatus : int {
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
        public string uniqueID { get; protected set; }

        /// <summary>
        /// Display friendly device name.
        /// </summary>
        public string name { get; protected set; }

        /// <summary>
        /// Device location.
        /// </summary>
        public virtual Location location => (Location)((int)device.Flags() & 0x3);

        /// <summary>
        /// Device is the default device for its media type.
        /// </summary>
        public virtual bool defaultForMediaType => device.Flags().HasFlag(DeviceFlags.Default);
        #endregion


        #region --Events--
        /// <summary>
        /// Event raised when the device is disconnected.
        /// </summary>
        public event Action onDisconnected;
        #endregion


        #region --Streaming--
        /// <summary>
        /// Whether the device is running.
        /// </summary>
        public virtual bool running => device.Running();

        /// <summary>
        /// Stop running.
        /// </summary>
        public abstract void StopRunning ();
        #endregion


        #region --Operations--
        protected readonly IntPtr device;
        private readonly GCHandle weakSelf;

        internal MediaDevice (IntPtr device) {
            this.device = device;
            this.weakSelf = GCHandle.Alloc(this, GCHandleType.Weak);
            // Cache UID
            var uidBuilder = new StringBuilder(2048);
            device.UniqueID(uidBuilder);
            this.uniqueID = uidBuilder.ToString();
            // Cache name
            var nameBuilder = new StringBuilder(2048);
            device.Name(nameBuilder);
            this.name = nameBuilder.ToString();
            // Register handlers
            device.SetDisconnectHandler(OnDeviceDisconnect, (IntPtr)weakSelf);
        }

        ~MediaDevice () {
            device.ReleaseDevice();
            weakSelf.Free();
        }

        public static implicit operator IntPtr (MediaDevice device) => device.device;

        protected static Task<PermissionStatus> CheckPermissions (PermissionType type, bool request) => Application.platform switch {
            RuntimePlatform.Android => CheckPermissionsAndroid(type, request),
            _                       => CheckPermissionsNative(type, request),
        };

        private static Task<PermissionStatus> CheckPermissionsNative (PermissionType type, bool request) {
            var tcs = new TaskCompletionSource<PermissionStatus>();
            var handle = GCHandle.Alloc(tcs, GCHandleType.Normal);
            try {
                VideoKit.CheckPermissions(type, request, OnPermissionResult, (IntPtr)handle).CheckStatus();
            } catch (Exception ex) {
                handle.Free();
                tcs.SetException(ex);
            }
            return tcs.Task;
        }

        private static Task<PermissionStatus> CheckPermissionsAndroid (PermissionType type, bool request) {
            var permission = PermissionTypeToString(type);
            // Check status
            var status = GetPermissionStatus(permission);
            if (!request || status == PermissionStatus.Authorized)
                return Task.FromResult(status);
            // Request
            var tcs = new TaskCompletionSource<PermissionStatus>();
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += _ => tcs.SetResult(PermissionStatus.Authorized);
            callbacks.PermissionDenied += _ => tcs.SetResult(PermissionStatus.Denied);
            callbacks.PermissionDeniedAndDontAskAgain += _ => tcs.SetResult(PermissionStatus.Denied);
            Permission.RequestUserPermission(permission, callbacks);
            // Return
            return tcs.Task;
            // Utilities
            static PermissionStatus GetPermissionStatus (string type) => Permission.HasUserAuthorizedPermission(type) switch {
                true    => MediaDevice.PermissionStatus.Authorized,
                false   => MediaDevice.PermissionStatus.Unknown
            };
            static string PermissionTypeToString (PermissionType type) => type switch {
                PermissionType.Microphone   => Permission.Microphone,
                PermissionType.Camera       => Permission.Camera,
                _                           => null
            };
        }
        #endregion


        #region --Callbacks--

        [MonoPInvokeCallback(typeof(VideoKit.DeviceDisconnectHandler))]
        private static void OnDeviceDisconnect (IntPtr context) {
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

        [MonoPInvokeCallback(typeof(VideoKit.PermissionResultHandler))]
        private static void OnPermissionResult (IntPtr context, PermissionStatus status) {
            // Check
            if (!VideoKit.IsAppDomainLoaded)
                return;
            // Get tcs
            TaskCompletionSource<PermissionStatus> tcs;
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
        #endregion
    }
}