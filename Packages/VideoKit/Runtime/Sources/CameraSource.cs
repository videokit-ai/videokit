/* 
*   VideoKit
*   Copyright © 2024 NatML Inc. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Sources {

    using System;
    using UnityEngine;
    using Clocks;
    using Internal;

    /// <summary>
    /// Media source for generating pixel buffers from one or more scene cameras.
    /// </summary>
    public sealed class CameraSource : IDisposable {

        #region --Client API--
        /// <summary>
        /// Cameras being recorded from.
        /// </summary>
        public readonly Camera[] cameras;

        /// <summary>
        /// Texture source used to create pixel buffers from the camera texture.
        /// </summary>
        public readonly TextureSource textureSource;

        /// <summary>
        /// Clear color to use before rendering cameras to a texture.
        /// </summary>
        public Color clearColor = Color.black;

        /// <summary>
        /// Control number of successive camera frames to skip while recording.
        /// This is very useful for GIF recording, which typically has a lower framerate appearance.
        /// </summary>
        public int frameSkip;

        /// <summary>
        /// Create a pixel buffer source from one or more game cameras.
        /// </summary>
        /// <param name="recorder">Media recorder to receive images.</param>
        /// <param name="cameras">Game cameras to capture images from.</param>
        public CameraSource (MediaRecorder recorder, params Camera[] cameras) : this(
            recorder,
            default,
            cameras
        ) { }

        /// <summary>
        /// Create a pixel buffer source from one or more game cameras.
        /// </summary>
        /// <param name="recorder">Media recorder to receive images.</param>
        /// <param name="clock">Clock for generating image timestamps.</param>
        /// <param name="cameras">Game cameras to capture images from.</param>
        public CameraSource (MediaRecorder recorder, IClock? clock, params Camera[] cameras) : this(
            recorder.width,
            recorder.height,
            recorder.Append,
            clock,
            cameras
        ) { }

        /// <summary>
        /// Create a pixel buffer source from one or more game cameras.
        /// </summary>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="handler">Handler to receive images.</param>
        /// <param name="cameras">Game cameras to capture images from.</param>
        public CameraSource (int width, int height, Action<PixelBuffer> handler, params Camera[] cameras) : this(
            width,
            height,
            handler,
            default,
            cameras
        ) { }

        /// <summary>
        /// Create a pixel buffer source from one or more game cameras.
        /// </summary>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="handler">Handler to receive images.</param>
        /// <param name="clock">Clock for generating image timestamps.</param>
        /// <param name="cameras">Game cameras to capture images from.</param>
        public CameraSource (
            int width,
            int height,
            Action<PixelBuffer> handler,
            IClock? clock,
            params Camera[] cameras
        ) {
            Array.Sort(cameras, (a, b) => (int)(100 * (a.depth - b.depth)));
            this.cameras = cameras;
            this.clock = clock;
            this.descriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGBHalf, 24) {
                sRGB = true,
                msaaSamples = Mathf.Max(QualitySettings.antiAliasing, 1)
            };
            this.textureSource = new TextureSource(width, height, handler);
            VideoKitEvents.Instance.onFrame += OnFrame;
        }

        /// <summary>
        /// Stop the media source and release resources.
        /// </summary>
        public void Dispose () {
            VideoKitEvents.Instance.onFrame -= OnFrame;
            textureSource.Dispose();
        }
        #endregion


        #region --Operations--
        private readonly IClock? clock;
        private readonly RenderTextureDescriptor descriptor;
        private int frameIdx;

        private void OnFrame () {
            // Check frame index
            if (frameIdx++ % (frameSkip + 1) != 0)
                return;
            // Clear framebuffer
            var frameBuffer = RenderTexture.GetTemporary(descriptor);
            var prevActive = RenderTexture.active;
            RenderTexture.active = frameBuffer;
            GL.Clear(true, true, clearColor);
            RenderTexture.active = prevActive;
            // Render cameras
            for (var i = 0; i < cameras.Length; i++) {
                var camera = cameras[i];
                var prevTarget = camera.targetTexture;
                camera.targetTexture = frameBuffer;
                camera.Render();
                camera.targetTexture = prevTarget;
            }
            // Append
            textureSource.Append(frameBuffer, clock?.timestamp ?? 0L);
            // Release framebuffer
            RenderTexture.ReleaseTemporary(frameBuffer);
        }
        #endregion
    }
}