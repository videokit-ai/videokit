/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using UnityEngine;

    internal sealed class RecordEmptyVideoTest : MonoBehaviour {

        private async void Start() {
            var recorder = await MediaRecorder.Create(
                format: MediaRecorder.Format.MP4,
                width: 1280,
                height: 720,
                frameRate: 30
            );
            var asset = await recorder.FinishWriting();
            Debug.Log(asset.path);
        }
    }
}