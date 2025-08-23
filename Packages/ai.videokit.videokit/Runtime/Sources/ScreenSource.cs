/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Sources {

    using System;
    using UnityEngine;
    using Clocks;
    using Internal;

    /// <summary>
    /// Media source for generating images from the screen.
    /// </summary>
    public sealed class ScreenSource : IDisposable {

        #region --Client API--
        /// <summary>
        /// Texture source used to create pixel buffers from the screen texture.
        /// </summary>
        public readonly TextureSource textureSource;

        /// <summary>
        /// Control number of successive frames to skip while generating images.
        /// This is very useful for GIF recording, which typically has a lower framerate appearance.
        /// </summary>
        public int frameSkip;

        /// <summary>
        /// Create a screen source.
        /// </summary>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="handler">Handler to receive images.</param>
        /// <param name="clock">Clock for generating image timestamps.</param>
        /// <param name="useLateUpdate">Whether to use` LateUpdate` instead of end of frame for capturing frames. See #52.</param>
        public ScreenSource(
            int width,
            int height,
            Action<PixelBuffer> handler,
            IClock? clock = null,
            bool useLateUpdate = false
        ) {
            // Create texture source
            this.clock = clock;
            this.descriptor = new(width, height, RenderTextureFormat.ARGBHalf, 0);
            this.textureSource = new(width, height, handler);
            // Listen for frame events
            if (useLateUpdate)
                VideoKitEvents.Instance.onLateUpdate += OnFrame;
            else
                VideoKitEvents.Instance.onFrame += OnFrame;
        }

        /// <summary>
        /// Stop the screen source and release resources.
        /// </summary>
        public void Dispose() {
            textureSource.Dispose();
            VideoKitEvents.Instance.onLateUpdate -= OnFrame;
            VideoKitEvents.Instance.onFrame -= OnFrame;
        }
        #endregion


        #region --Operations--
        private readonly IClock? clock;
        private readonly RenderTextureDescriptor descriptor;
        private int frameIdx;

        private void OnFrame() {
            // Check frame index
            if (frameIdx++ % (frameSkip + 1) != 0)
                return;
            // Capture screen
            var screenBuffer = RenderTexture.GetTemporary(
                Screen.width,
                Screen.height,
                0,
                RenderTextureFormat.ARGBHalf
            );
            var frameBuffer = RenderTexture.GetTemporary(descriptor);
            ScreenCapture.CaptureScreenshotIntoRenderTexture(screenBuffer);
            Graphics.Blit(
                screenBuffer,
                frameBuffer,
                SystemInfo.graphicsUVStartsAtTop ? new Vector2(1, -1) : Vector2.one,
                SystemInfo.graphicsUVStartsAtTop ? Vector2.up : Vector2.zero
            );
            // Append
            textureSource.Append(frameBuffer, clock?.timestamp ?? 0L);
            RenderTexture.ReleaseTemporary(frameBuffer);
            RenderTexture.ReleaseTemporary(screenBuffer);
        }
        #endregion


        #region --Deprecated--
        [Obsolete(@"Deprecated in VideoKit 0.0.23. Use the ScreenSource(width, height, handler, clock) constructor instead.", false)]
        public ScreenSource(
            MediaRecorder recorder,
            IClock? clock = null,
            bool useLateUpdate = false
        ) : this(recorder.width, recorder.height, recorder.Append, clock, useLateUpdate) { }
        #endregion
    }
}