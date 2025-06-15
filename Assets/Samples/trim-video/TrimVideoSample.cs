/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Samples {

    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.Video;
    using Unity.Collections;

    public class TrimVideoSample : MonoBehaviour {

        [Header(@"Trimming")]
        [Tooltip(@"Relative video path in `StreamingAssets` folder.")]
        public string videoPath;
        [Tooltip(@"Trim start time in seconds."), Range(0f, 10f)]
        public float trimStart = 5f;
        [Tooltip(@"Trim duration time in seconds."), Range(0f, 20f)]
        public float trimDuration = 10f;

        [Header(@"Playback")]
        public VideoPlayer videoPlayer;
        public RawImage rawImage;
        public AspectRatioFitter aspectFitter;

        private async void Start () {
            // Load video
            var asset = await MediaAsset.FromStreamingAssets(videoPath);
            // Calculate start and end times in nanoseconds
            var startTimeNs = (long)(trimStart * 1e+9f);
            var endTimeNs = startTimeNs + (long)(trimDuration * 1e+9f);
            // Create a recorder
            var recorder = await MediaRecorder.Create(
                format: MediaRecorder.Format.MP4,
                width: asset.width,
                height: asset.height,
                frameRate: asset.frameRate
            );
            // Enumerate and copy frames
            await Awaitable.BackgroundThreadAsync();
            using var pixelData = new NativeArray<byte>(
                length: asset.width * asset.height * 4,
                allocator: Allocator.Persistent
            );
            foreach (var pixelBuffer in asset.Read<PixelBuffer>()) {
                // Check start time
                if (pixelBuffer.timestamp < startTimeNs)
                    continue;
                // Check end time
                if (pixelBuffer.timestamp > endTimeNs)
                    break;
                // Convert pixel buffer to `RGBA8888`
                // This conversion is required because media recorders currently support `RGBA8888` only.
                // In the future, media recorders will support more formats which will improve performance.
                var timestamp = pixelBuffer.timestamp - startTimeNs;
                using var rgbaBuffer = new PixelBuffer(
                    width: pixelBuffer.width,
                    height: pixelBuffer.height,
                    format: PixelBuffer.Format.RGBA8888,
                    data: pixelData,
                    timestamp: timestamp
                );
                pixelBuffer.CopyTo(rgbaBuffer);
                // Append to the recorder
                recorder.Append(rgbaBuffer);
            }
            // Finish writing
            var trimmedAsset = await recorder.FinishWriting();
            // Playback the trimmed video
            await Awaitable.MainThreadAsync();
            videoPlayer.url = trimmedAsset.path;
            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared)
                await Awaitable.NextFrameAsync();
            videoPlayer.Play();
            rawImage.texture = videoPlayer.texture;
            aspectFitter.aspectRatio = (float)trimmedAsset.width / trimmedAsset.height;
        }
    }
}