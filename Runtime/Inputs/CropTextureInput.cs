/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Recorders.Inputs {

    using UnityEngine;

    /// <summary>
    /// Recorder input for recording a cropped region of committed video frames.
    /// The crop texture input always preserves the aspect of the cropped image.
    /// </summary>
    public sealed class CropTextureInput : TextureInput, ITextureInput {

        #region --Client API--
        /// <inheritdoc />
        public override (int width, int height) frameSize => input.frameSize;

        /// <summary>
        /// Crop rect in pixel coordinates of the recorder.
        /// </summary>
        public RectInt rect;

        /// <summary>
        /// Create a crop texture input.
        /// </summary>
        /// <param name="recorder">Media recorder to receive watermarked frames.</param>
        public CropTextureInput (MediaRecorder recorder) : this(CreateDefault(recorder)) { }

        /// <summary>
        /// Create a crop texture input.
        /// </summary>
        /// <param name="input">Texture input to receive cropped frames.</param>
        public CropTextureInput (TextureInput input) : base(null) {
            this.input = input;
            this.rect = new RectInt(0, 0, input.frameSize.width, input.frameSize.height);
        }

        /// <summary>
        /// Commit a video frame from a texture.
        /// </summary>
        /// <param name="texture">Source texture.</param>
        /// <param name="timestamp">Frame timestamp in nanoseconds.</param>
        public override void CommitFrame (Texture texture, long timestamp) {
            // Create frame buffer
            var (width, height) = frameSize;
            var descriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32, 0);
            var frameBuffer = RenderTexture.GetTemporary(descriptor);
            // Render
            (this as ITextureInput).CommitFrame(texture, frameBuffer);
            // Commit
            input.CommitFrame(frameBuffer, timestamp);
            RenderTexture.ReleaseTemporary(frameBuffer);
        }

        /// <summary>
        /// Stop the recorder input and release resources.
        /// </summary>
        public override void Dispose () {
            input.Dispose();
            base.Dispose();
        }
        #endregion


        #region --Operations--
        private readonly TextureInput input;

        void ITextureInput.CommitFrame (Texture source, RenderTexture destination) {
            // Compute crop scale
            var frameSize = new Vector2(destination.width, destination.height);
            var ratio = new Vector2(frameSize.x / rect.width, frameSize.y / rect.height);
            var scale = Mathf.Max(ratio.x, ratio.y);
            // Compute draw rect
            var pixelSize = scale * frameSize;
            var minPoint = 0.5f * frameSize - scale * rect.center;
            var maxPoint = minPoint + pixelSize;
            var drawRect = new Rect(minPoint.x, destination.height - maxPoint.y, pixelSize.x, pixelSize.y);
            // Render
            var prevActive = RenderTexture.active;
            RenderTexture.active = destination;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, destination.width, destination.height, 0);
            Graphics.DrawTexture(drawRect, source);
            GL.PopMatrix();
            RenderTexture.active = prevActive;
        }
        #endregion
    }
}