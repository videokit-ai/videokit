/* 
*   VideoKit
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.Inputs {

    using UnityEngine;
    using NatML.Recorders;
    using NatML.Recorders.Inputs;

    /// <summary>
    /// Recorder input for recording a cropped region of committed video frames.
    /// The crop texture input always preserves the aspect of the cropped image.
    /// </summary>
    public sealed class CropTextureInput : TextureInput {

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
        public CropTextureInput (IMediaRecorder recorder) : this(CreateDefault(recorder)) { }

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
            // Compute crop scale
            var (width, height) = input.frameSize;
            var frameSize = new Vector2(width, height);
            var ratio = new Vector2(frameSize.x / rect.width, frameSize.y / rect.height);
            var scale = Mathf.Max(ratio.x, ratio.y);
            // Compute draw rect
            var pixelSize = scale * frameSize;
            var minPoint = 0.5f * frameSize - scale * rect.center;
            var maxPoint = minPoint + pixelSize;
            var drawRect = new Rect(minPoint.x, height - maxPoint.y, pixelSize.x, pixelSize.y);
            // Create frame buffer
            var descriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32, 0);
            var frameBuffer = RenderTexture.GetTemporary(descriptor);
            // Render
            var prevActive = RenderTexture.active;
            RenderTexture.active = frameBuffer;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, width, height, 0);
            Graphics.DrawTexture(drawRect, texture);
            GL.PopMatrix();
            RenderTexture.active = prevActive;
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
        #endregion
    }
}