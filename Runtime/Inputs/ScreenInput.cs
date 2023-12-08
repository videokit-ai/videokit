/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Recorders.Inputs {

    using System;
    using System.Collections;
    using UnityEngine;
    using Clocks;

    /// <summary>
    /// Recorder input for recording video frames from the screen.
    /// Unlike the `CameraInput`, this recorder input is able to record overlay UI canvases.
    /// </summary>
    public sealed class ScreenInput : IDisposable {

        #region --Client API--
        /// <summary>
        /// Control number of successive camera frames to skip while recording.
        /// This is very useful for GIF recording, which typically has a lower framerate appearance.
        /// </summary>
        public int frameSkip;

        /// <summary>
        /// Create a video recording input from the screen.
        /// </summary>
        /// <param name="recorder">Media recorder to receive video frames.</param>
        /// <param name="clock">Recording clock for generating timestamps.</param>
        public ScreenInput (MediaRecorder recorder, IClock clock = default) : this(
            TextureInput.CreateDefault(recorder),
            clock
        ) { }

        /// <summary>
        /// Create a video recording input from the screen.
        /// </summary>
        /// <param name="input">Texture input to receive video frames.</param>
        /// <param name="clock">Recording clock for generating timestamps.</param>
        public ScreenInput (TextureInput input, IClock clock = default) {
            this.input = input;
            this.clock = clock;
            this.descriptor = new RenderTextureDescriptor(input.frameSize.width, input.frameSize.height, RenderTextureFormat.ARGBHalf, 0);
            // Start recording
            attachment = new GameObject(@"NatCorder ScreenInputAttachment").AddComponent<ScreenInputAttachment>();
            attachment.StartCoroutine(CommitFrames());
        }

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
        private readonly RenderTextureDescriptor descriptor;
        private readonly ScreenInputAttachment attachment;
        private int frameCount;

        private IEnumerator CommitFrames () {
            var yielder = new WaitForEndOfFrame();
            for (;;) {
                // Check frame index
                yield return yielder;
                if (frameCount++ % (frameSkip + 1) != 0)
                    continue;
                // Capture screen
                var frameBuffer = RenderTexture.GetTemporary(descriptor);
                if (SystemInfo.graphicsUVStartsAtTop) {
                    var tempBuffer = RenderTexture.GetTemporary(descriptor);
                    ScreenCapture.CaptureScreenshotIntoRenderTexture(tempBuffer);
                    Graphics.Blit(tempBuffer, frameBuffer, new Vector2(1, -1), Vector2.up);
                    RenderTexture.ReleaseTemporary(tempBuffer);
                } else
                    ScreenCapture.CaptureScreenshotIntoRenderTexture(frameBuffer);
                // Commit
                input.CommitFrame(frameBuffer, clock?.timestamp ?? 0L);
                RenderTexture.ReleaseTemporary(frameBuffer);
            }
        }

        private sealed class ScreenInputAttachment : MonoBehaviour { }
        #endregion
    }
}