/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using UnityEngine;

    internal sealed class VideoKitCameraManagerSwitchCameraTest : MonoBehaviour {

        [SerializeField] private VideoKitCameraManager cameraManager;

        public void SwitchCamera() {
            var newFacing = cameraManager.facing switch {
                VideoKitCameraManager.Facing.User => VideoKitCameraManager.Facing.World,
                _ => VideoKitCameraManager.Facing.User
            };
            cameraManager.facing = newFacing;
            Debug.Log($"Switched camera to: {newFacing}");
        }
    }
}