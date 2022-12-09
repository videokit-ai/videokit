/* 
*   VideoKit
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.Inputs {

    using UnityEngine;
    using NatML.Recorders;
    using NatML.Recorders.Inputs;

    /// <summary>
    /// Recorder input for recording video frames from textures with a watermark.
    /// </summary>
    public sealed class WatermarkTextureInput : TextureInput {

        #region --Client API--
        /// <inheritdoc />
        public override (int width, int height) frameSize => input.frameSize;

        /// <summary>
        /// Watermark image.
        /// If `null`, no watermark will be rendered.
        /// </summary>
        public Texture watermark;

        /// <summary>
        /// Watermark display rect in pixel coordinates of the recorder.
        /// </summary>
        public RectInt rect;

        /// <summary>
        /// Create a watermark texture input.
        /// </summary>
        /// <param name="recorder">Media recorder to receive watermarked frames.</param>
        public WatermarkTextureInput (IMediaRecorder recorder) : this(CreateDefault(recorder)) { }

        /// <summary>
        /// Create a watermark texture input.
        /// </summary>
        /// <param name="input">Texture input to receive watermarked frames.</param>
        public WatermarkTextureInput (TextureInput input) : base(null) => this.input = input;

        /// <summary>
        /// Commit a video frame from a texture.
        /// </summary>
        /// <param name="texture">Source texture.</param>
        /// <param name="timestamp">Frame timestamp in nanoseconds.</param>
        public override void CommitFrame (Texture texture, long timestamp) {
            // Check
            if (!watermark) {
                input.CommitFrame(texture, timestamp);
                return;
            }
            // Create frame buffer
            var (width, height) = frameSize;
            var descriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32, 0);
            var frameBuffer = RenderTexture.GetTemporary(descriptor);
            // Render
            var drawRect = new Rect(rect.x, height - rect.max.y, rect.width, rect.height);
            var prevActive = RenderTexture.active;
            RenderTexture.active = frameBuffer;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, width, height, 0);
            Graphics.Blit(texture, frameBuffer);
            Graphics.DrawTexture(drawRect, watermark);
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