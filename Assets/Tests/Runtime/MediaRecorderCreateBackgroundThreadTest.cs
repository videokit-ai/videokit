/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using UnityEngine;

    internal sealed class MediaRecorderCreateBackgroundThreadTest : MonoBehaviour {
        
        private async void Start() {
            await VideoKit.Internal.VideoKitClient.Instance.CheckSession();
            await Awaitable.BackgroundThreadAsync();
            var recorder = await MediaRecorder.Create(
                format: MediaRecorder.Format.MP4,
                width: 1280,
                height: 720,
                frameRate: 30
            );
            var data = new byte[recorder.width * recorder.height * 4];
            using var pixelBuffer = new PixelBuffer(
                width: recorder.width,
                height: recorder.height,
                format: PixelBuffer.Format.RGBA8888,
                data: data
            );
            recorder.Append(pixelBuffer);
            var asset = await recorder.FinishWriting();
            Debug.Log($"Recorded video to path: {asset.path}");
        }
    }
}