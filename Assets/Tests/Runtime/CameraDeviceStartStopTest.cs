/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using UnityEngine;
    using Newtonsoft.Json;

    internal sealed class CameraDeviceStartStopTest : MonoBehaviour {

        [SerializeField]
        private VideoKitCameraManager cameraManager;

        public void ToggleCamera() {
            if (!cameraManager.running)
                cameraManager.StartRunning();
            else
                cameraManager.StopRunning();
        }
    }
}