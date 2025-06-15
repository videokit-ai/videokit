/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Samples {

    using UnityEngine;
    using Unity.Collections;
    using VideoKit.UI;
    using VideoKit.Clocks;

    public class SlowMotionCaptureSample : MonoBehaviour {

        public VideoKitCameraManager cameraManager;
        public VideoKitCameraView cameraView;

        private MediaRecorder recorder;
        private RealtimeClock clock;
        private NativeArray<byte> pixelData;

        public async void StartRecording () {
            // Get camera device
            if (cameraManager.device is not CameraDevice cameraDevice) {
                Debug.LogError(@"Slow motion capture sample requires a camera device, not a multi-camera device");
                return;
            }
            // Create media recorder and clock
            recorder = await MediaRecorder.Create(
                format: MediaRecorder.Format.MP4,
                width: cameraView.texture.width,
                height: cameraView.texture.height,
                frameRate: cameraDevice.frameRate
            );
            clock = new RealtimeClock();
            // Create RGBA pixel data that will be used for recording
            pixelData = new NativeArray<byte>(
                length: recorder.width * recorder.height * 4,
                allocator: Allocator.Persistent
            );
            // Listen for raw pixel buffers from the camera device
            cameraManager.OnPixelBuffer += OnPixelBuffer;
        }

        public async void StopRecording () {
            // Stop listening for pixel buffers
            cameraManager.OnPixelBuffer -= OnPixelBuffer;
            // Stop recording
            var asset = await recorder.FinishWriting();
            // Save to the camera roll
            await asset.SaveToCameraRoll();
        }

        private void OnPixelBuffer (CameraDevice cameraDevice, PixelBuffer cameraBuffer) {
            // Convert to `RGBA8888`
            using var recorderBuffer = new PixelBuffer(
                recorder.width,
                recorder.height,
                format: PixelBuffer.Format.RGBA8888,
                data: pixelData,
                timestamp: clock.timestamp,
                mirrored: cameraBuffer.verticallyMirrored
            );
            cameraBuffer.CopyTo(recorderBuffer, rotation: cameraView.rotation);
            // Append to recorder
            recorder.Append(recorderBuffer);
        }
    }
}