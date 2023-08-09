/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Devices.Outputs {

    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// Camera device output that streams camera images into a `Texture2D`.
    /// The texture output uses the `PixelBufferOutput` to convert camera images to `RGBA8888` before uploading to the GPU.
    /// The rendered texture data is accessible on the CPU using the `Texture2D` data access methods.
    /// </summary>
    public sealed class TextureOutput : CameraOutput {

        #region --Client API--
        /// <summary>
        /// Texture conversion options.
        /// </summary>
        public class ConversionOptions : PixelBufferOutput.ConversionOptions { }

        /// <summary>
        /// Texture containing the latest camera image.
        /// This is `null` when no image has been processed by the output.
        /// </summary>
        public readonly Texture2D texture;

        /// <summary>
        /// Get or set the texture orientation.
        /// </summary>
        public ScreenOrientation orientation {
            get => pixelBufferOutput.orientation;
            set => pixelBufferOutput.orientation = value;
        }

        /// <summary>
        /// Event raised when a new camera image is available in the texture output.
        /// </summary>
        public event Action<TextureOutput> OnFrame;

        /// <summary>
        /// Create a texture output.
        /// </summary>
        public TextureOutput () {
            this.texture = new Texture2D(16, 16, TextureFormat.RGBA32, false, false);
            this.pixelBufferOutput = new PixelBufferOutput();
            this.taskCompletionSource = new TaskCompletionSource<Texture2D>();
            this.fence = new object();
            pixelBufferOutput.lifecycleHelper.onUpdate += OnPixelBuffer;
        }

        /// <summary>
        /// Update the output with a new camera image.
        /// </summary>
        /// <param name="image">Camera image.</param>
        public override void Update (CameraImage image) => Update(image, null);

        /// <summary>
        /// Update the output with a new camera image.
        /// </summary>
        /// <param name="image">Camera image.</param>
        /// <param name="options">Conversion options.</param>
        public void Update (CameraImage image, ConversionOptions options) {
            lock (fence)
                pixelBufferOutput.Update(image, options);
        }

        /// <summary>
        /// Get a task that completes when the next frame is available.
        /// </summary>
        public Task<Texture2D> NextFrame () => taskCompletionSource.Task;

        /// <summary>
        /// Dispose the texture output and release resources.
        /// </summary>
        public override void Dispose () {
            lock (fence) {
                pixelBufferOutput.Dispose();
                taskCompletionSource.TrySetCanceled();
                Texture2D.Destroy(texture);
            }
        }
        #endregion


        #region --Operations--
        private readonly PixelBufferOutput pixelBufferOutput;
        private readonly TaskCompletionSource<Texture2D> taskCompletionSource;
        private readonly object fence;

        private void OnPixelBuffer () {
            lock (fence) {
                // Check first frame
                if (pixelBufferOutput.timestamp == 0L)
                    return;
                // Check dirty
                if (timestamp == pixelBufferOutput.timestamp)
                    return;
                // Check texture
                var (width, height) = (pixelBufferOutput.width, pixelBufferOutput.height);
                if (texture.width != width || texture.height != height)
                    texture.Reinitialize(pixelBufferOutput.width, pixelBufferOutput.height);
                // Upload
                texture.GetRawTextureData<byte>().CopyFrom(pixelBufferOutput.pixelBuffer);
                texture.Apply();
                // Update timestamp
                image = pixelBufferOutput.image;
            }
            // Notify
            taskCompletionSource.TrySetResult(texture);
            OnFrame?.Invoke(this);
        }
        #endregion
    }
}