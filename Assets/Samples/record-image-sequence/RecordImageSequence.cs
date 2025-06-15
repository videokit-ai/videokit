/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Samples {

    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.Video;
    using VideoKit.Clocks;

    public sealed class RecordImageSequence : MonoBehaviour {

        [Header(@"Recording")]
        public Texture2D[] images;
        [Range(20f, 60f)] public float frameRate = 30f;

        [Header(@"Playback")]
        public VideoPlayer videoPlayer;
        public RawImage videoPanel;
        public AspectRatioFitter videoAspectFitter;

        private async void Start () {
            // Check images
            if (images.Length == 0) {
                Debug.LogError(@"Cannot record image sequence because no images have been provided.");
                return;
            }
            // Check that all images are the same size
            var (width, height) = (images[0].width, images[0].height);
            if (images.Any(image => image.width != width || image.height != height)) {
                Debug.LogError(@"Cannot record image sequence because images must all have the same size.");
                return;
            }
            // Check that all images are RGBA32
            if (images.Any(image => image.format != TextureFormat.RGBA32)) {
                Debug.LogError(@"Cannot record image sequence because images must all have an `RGBA8888` image format.");
                return;
            }
            // Create a recorder
            var clock = new FixedClock(frameRate);
            var recorder = await MediaRecorder.Create(
                format: MediaRecorder.Format.MP4,
                width: width,
                height: height,
                frameRate: frameRate
            );
            // Unity does not allow for accessing any properties on a `Texture2D` object from a worker thread.
            // So we'll get all the `NativeArray` instances containing pixel data for each image ahead of time.
            var imageData = images.Select(image => image.GetRawTextureData<byte>()).ToArray();
            // Append frames in a background thread
            await Awaitable.BackgroundThreadAsync(); // requires Unity 2023.1+
            foreach (var data in imageData) {
                using var pixelBuffer = new PixelBuffer(
                    width: width,
                    height: height,
                    format: PixelBuffer.Format.RGBA8888,
                    data: data,
                    timestamp: clock.timestamp
                );
                recorder.Append(pixelBuffer);
            }
            // Switch back to the main thread to finish writing
            await Awaitable.MainThreadAsync();
            var asset = await recorder.FinishWriting();
            Debug.Log($"Recorded image sequence to {asset.path}");
            // Playback
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = $"file://{asset.path}";
            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared)
                await Task.Yield();
            videoPanel.texture = videoPlayer.texture;
            videoAspectFitter.aspectRatio = (float)videoPlayer.texture.width / videoPlayer.texture.height;
        }
    }
}