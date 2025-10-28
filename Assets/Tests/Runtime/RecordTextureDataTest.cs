/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using System;
    using System.Threading.Tasks;
    using UnityEngine;
    using VideoKit.Clocks;

    internal sealed class RecordTextureDataTest : MonoBehaviour {

        [SerializeField] private Texture2D texture;

        private async void Start() {
            var recorder = await MediaRecorder.Create(MediaRecorder.Format.MP4, texture.width, texture.height, 30);
            var clock = new FixedClock(30);
            var pixelData = new byte[texture.width * texture.height * 4];
            Debug.Log("Started recording");
            for (var i = 0; i < 4 * 30; ++i) {
                using var pixelBuffer = new PixelBuffer(
                    texture.width,
                    texture.height,
                    PixelBuffer.Format.RGBA8888,
                    pixelData,
                    timestamp: clock.timestamp
                );
                recorder.Append(pixelBuffer);
                await Task.Yield();
            }
            await recorder.FinishWriting();
            Debug.Log("Finished recording");
        }
    }
}