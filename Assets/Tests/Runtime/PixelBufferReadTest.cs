/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.UI;

    internal sealed class PixelBufferReadTest : MonoBehaviour {

        [Header(@"UI")]
        [SerializeField] private RawImage rawImage;
        [SerializeField] private AspectRatioFitter aspectFitter;

        private async void Start () {
            // Load asset
            var asset = await MediaAsset.FromStreamingAssets(@"rain.mp4");
            Debug.Log($"{asset.width}x{asset.height} @{asset.frameRate}Hz {asset.duration}s");
            // Create and display texture
            var texture = new Texture2D(asset.width, asset.height, TextureFormat.RGBA32, false);
            rawImage.texture = texture;
            aspectFitter.aspectRatio = (float)texture.width / texture.height;
            // Read pixel buffers
            foreach (var pixelBuffer in asset.Read<PixelBuffer>()) {
                // Copy pixel data into the texture
                using var textureBuffer = new PixelBuffer(texture);
                pixelBuffer.CopyTo(textureBuffer);
                // Upload the texture data to the GPU
                texture.Apply();
                // Wait for next frame
                await Task.Yield();
            }
            // Log
            Debug.Log($"Finished reading frames");
        }
    }
}