/* 
*   VideoKit
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.Internal {

    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;
    using NatML.Devices;
    using NatML.Sharing;

    internal static class VideoKitUser {

        #region --Media Devices--

        public static async Task<AudioDevice> CreateAudioDevice () {
            // Request microphone permissions
            var permissionStatus = await MediaDeviceQuery.RequestPermissions<AudioDevice>();
            if (permissionStatus != PermissionStatus.Authorized) {
                Debug.LogError(@"VideoKit: User did not grant microphone permissions");
                return null;
            }
            // Discover the front camera
            var query = new MediaDeviceQuery(MediaDeviceCriteria.AudioDevice);
            var device = query.current;
            // Check
            if (device == null)
                Debug.LogError(@"VideoKit: Failed to discover an available audio device");
            // Return
            return device as AudioDevice;
        }

        public static async Task<CameraDevice> CreateCameraDevice () {
            // Request camera permissions
            var permissionStatus = await MediaDeviceQuery.RequestPermissions<CameraDevice>();
            if (permissionStatus != PermissionStatus.Authorized) {
                Debug.LogError(@"VideoKit: User did not grant camera permissions");
                return null;
            }
            // Discover the front camera
            var query = new MediaDeviceQuery(MediaDeviceCriteria.CameraDevice);
            var device = query.FirstOrDefault(device => device is CameraDevice camera && camera.frontFacing) ?? query.current;
            // Check
            if (device == null)
                Debug.LogError(@"VideoKit: Failed to discover an available camera device");
            // Return
            return device as CameraDevice;
        }
        #endregion


        #region --Social--

        public static async Task<string> Share (string path) {
            // Request permissions
            var granted = await SavePayload.RequestPermissions();
            if (!granted) {
                Debug.LogError(@"VideoKit recorder failed to share recording because user did not grant permissions");
                return null;
            }
            // Share
            var payload = new SharePayload();
            payload.AddMedia(path);
            var receiver = await payload.Share();
            return receiver;
        }

        public static async Task<bool> SaveToCameraRoll (string path) {
            // Request permissions
            var granted = await SavePayload.RequestPermissions();
            if (!granted) {
                Debug.LogError(@"VideoKit recorder failed to save recording to camera roll because user did not grant permissions");
                return false;
            }
            // Save
            var payload = new SavePayload();
            payload.AddMedia(path);
            bool success = await payload.Save();
            return success;
        }
        #endregion
    }
}