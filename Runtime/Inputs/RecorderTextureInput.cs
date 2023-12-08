/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Recorders.Inputs {

    using UnityEngine;

    /// <summary>
    /// Texture input that supports stacking other texture inputs.
    /// </summary>
    internal sealed class RecorderTextureInput : TextureInput {

        #region --Client API--
        /// <summary>
        /// Texture inputs.
        /// </summary>
        public readonly ITextureInput[] inputs;

        /// <summary>
        /// Create a recorder texture input.
        /// </summary>
        /// <param name="recorder">Media recorder to receive video frames.</param>
        /// <param name="inputs">Inputs to apply to video frames.</param>
        public RecorderTextureInput (MediaRecorder recorder, params ITextureInput[] inputs) : base(recorder) {
            this.input = CreateDefault(recorder);
            this.inputs = inputs;
        }

        /// <summary>
        /// Commit a video frame from a texture.
        /// </summary>
        /// <param name="texture">Source texture.</param>
        /// <param name="timestamp">Frame timestamp in nanoseconds.</param>
        public override void CommitFrame (Texture texture, long timestamp) {
            var (width, height) = frameSize;
            var descriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32, 0);
            var result = RenderTexture.GetTemporary(descriptor);
            Graphics.Blit(texture, result);
            foreach (var textureInput in inputs) {
                var source = RenderTexture.GetTemporary(descriptor);
                Graphics.Blit(result, source);
                textureInput.CommitFrame(source, result);
                RenderTexture.ReleaseTemporary(source);
            }
            input.CommitFrame(result, timestamp);
            RenderTexture.ReleaseTemporary(result);
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