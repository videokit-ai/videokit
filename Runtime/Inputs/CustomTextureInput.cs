/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.Inputs {

    using System;
    using System.Collections;
    using UnityEngine;
    using NatML.Recorders;
    using NatML.Recorders.Clocks;
    using NatML.Recorders.Inputs;

    /// <summary>
    /// Recorder input for recording video frames from a texture.
    /// Unlike the standard `TextureInput` family, this class serves primarily as a
    /// way to connect Unity's `Update` loop to a texture input's `CommitFrame` method.
    /// </summary>
    internal sealed class CustomTextureInput : IDisposable {

        #region --Client API--
        /// <summary>
        /// Number of successive camera frames to skip while recording.
        /// This is very useful for GIF recording, which typically has a lower framerate appearance.
        /// </summary>
        public int frameSkip;

        /// <summary>
        /// Create a custom texture input.
        /// </summary>
        /// <param name="recorder">Media recorder to receive video frames.</param>
        /// <param name="clock">Clock for generating timestamps.</param>
        /// <param name="generator">Generator that produces textures to commit on each frame.</param>
        public CustomTextureInput (IMediaRecorder recorder, IClock clock, Func<int, Texture> generator) : this(TextureInput.CreateDefault(recorder), clock, generator) { }

        /// <summary>
        /// Create a custom texture input.
        /// </summary>
        /// <param name="recorder">Media recorder to receive video frames.</param>
        /// <param name="generator">Generator that produces textures to commit on each frame.</param>
        public CustomTextureInput (IMediaRecorder recorder, Func<int, Texture> generator) : this(recorder, default, generator) { }

        /// <summary>
        /// Create a custom texture input.
        /// </summary>
        /// <param name="input">Texture input to receive video frames.</param>
        /// <param name="clock">Clock for generating timestamps.</param>
        /// <param name="generator">Generator that produces textures to commit on each frame.</param>
        public CustomTextureInput (TextureInput input, IClock clock, Func<int, Texture> generator) {
            this.input = input;
            this.clock = clock;
            this.generator = generator;
            this.attachment = new GameObject(@"VideoKitCustomTextureInputAttachment").AddComponent<CustomTextureInputAttachment>();
            this.frameIdx = 0;
            // Start recording
            attachment.StartCoroutine(CommitFrames());
        }

        /// <summary>
        /// Create a custom texture input.
        /// </summary>
        /// <param name="input">Texture input to receive video frames.</param>
        /// <param name="generator">Generator that produces textures to commit on each frame.</param>
        public CustomTextureInput (TextureInput input, Func<int, Texture> generator) : this(input, default, generator) { }

        /// <summary>
        /// Stop recorder input and release resources.
        /// </summary>
        public void Dispose () {
            GameObject.DestroyImmediate(attachment.gameObject);
            input.Dispose();
        }
        #endregion


        #region --Operations--
        private readonly TextureInput input;
        private readonly IClock clock;
        private readonly Func<int, Texture> generator;
        private readonly CustomTextureInputAttachment attachment;
        private int frameIdx;

        private IEnumerator CommitFrames () {
            var yielder = new WaitForEndOfFrame();
            for (;;) {
                // Check frame index
                yield return yielder;
                if (frameIdx++ % (frameSkip + 1) != 0)
                    continue;
                // Get texture
                var texture = generator(frameIdx);
                if (!texture)
                    continue;
                // Commit
                input.CommitFrame(texture, clock?.timestamp ?? 0L);
            }
        }

        private sealed class CustomTextureInputAttachment : MonoBehaviour { }
        #endregion
    }
}