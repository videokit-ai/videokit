/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Sources {

    using System;
    using UnityEngine;
    using UnityEngine.Rendering;
    using Clocks;
    using Internal;

    /// <summary>
    /// Media source for generating pixel buffers from textures.
    /// </summary>
    public sealed class TextureSource : IDisposable {

        #region --Client API--
        /// <summary>
        /// Texture to automatically capture pixel buffers from.
        /// When this is set, the source will generate pixel buffers in the application update loop.
        /// To manually capture pixel buffers, set this to `null` and use the `Append` method.
        /// </summary>
        public Texture? texture;

        /// <summary>
        /// Watermark image.
        /// If `null`, no watermark will be rendered.
        /// </summary>
        public Texture? watermark;

        /// <summary>
        /// Watermark display rect in pixel coordinates.
        /// </summary>
        public RectInt watermarkRect;

        /// <summary>
        /// Whether to aspect fit the watermark into the watermark display rect.
        /// Defaults to `true`.
        /// </summary>
        public bool watermarkAspectFit = true;

        /// <summary>
        /// Region of interest to capture in pixel coordinates.
        /// </summary>
        public RectInt regionOfInterest;

        /// <summary>
        /// Control number of successive frames to skip while generating pixel buffers.
        /// This is very useful for GIF recording, which typically has a lower framerate appearance.
        /// </summary>
        public int frameSkip;
        
        /// <summary>
        /// Create a texture source.
        /// </summary>
        /// <param name="recorder">Media recorder to receive pixel buffers.</param>
        /// <param name="clock">Clock for generating pixel buffer timestamps.</param>
        public TextureSource (
            MediaRecorder recorder,
            IClock? clock = null
        ) : this(
            recorder.width,
            recorder.height,
            recorder.Append,
            clock
        ) { }

        /// <summary>
        /// Create a texture source.
        /// </summary>
        /// <param name="width">Pixel buffer width.</param>
        /// <param name="height">Pixel buffer height.</param>
        /// <param name="handler">Handler to receive pixel buffers.</param>
        /// <param name="clock">Clock for generating pixel buffer timestamps.</param>
        public TextureSource (
            int width,
            int height,
            Action<PixelBuffer> handler,
            IClock? clock = null
        ) {
            this.handler = handler;
            this.clock = clock;
            this.descriptor = new(width, height, RenderTextureFormat.ARGB32, 0) {
                sRGB = true
            };
            this.regionOfInterest = new(0, 0, width, height);
            VideoKitEvents.Instance.onFrame += OnFrame;
        }

        /// <summary>
        /// Append a pixel buffer from a texture.
        /// </summary>
        /// <param name="texture">Texture to readback from.</param>
        /// <param name="timestamp">Pixel buffer timestamp in nanoseconds.</param>
        public void Append (Texture texture, long timestamp = 0L) {
            // Check handler
            if (handler == null)
                return;
            // Blit
            var renderTexture = RenderTexture.GetTemporary(descriptor);
            Preprocess(texture, renderTexture);
            // Readback
            if (SystemInfo.supportsAsyncGPUReadback)
                AsyncGPUReadback.Request(renderTexture, 0, TextureFormat.RGBA32, request => {
                    // Check handler
                    if (handler == null)
                        return;
                    // Check error // Thanks Dr. Arth!
                    if (request.hasError) {
                        Debug.LogWarning("VideoKit TextureSource failed to readback texture data");
                        return;
                    }
                    // Invoke handler
                    using var pixelBuffer = new PixelBuffer(
                        request.width,
                        request.height,
                        PixelBuffer.Format.RGBA8888,
                        request.GetData<byte>(),
                        timestamp: timestamp
                    );
                    handler(pixelBuffer);
                });
            else {
                // Readback
                readbackBuffer = readbackBuffer != null ?
                    readbackBuffer :
                    new Texture2D(descriptor.width, descriptor.height, TextureFormat.RGBA32, false);
                var prevActive = RenderTexture.active;
                RenderTexture.active = renderTexture;
                readbackBuffer.ReadPixels(new Rect(0, 0, descriptor.width, descriptor.height), 0, 0, false);
                // Invoke handler
                using var pixelBuffer = new PixelBuffer(
                    descriptor.width,
                    descriptor.height,
                    PixelBuffer.Format.RGBA8888,
                    readbackBuffer.GetRawTextureData<byte>(),
                    timestamp: timestamp
                );
                handler(pixelBuffer);
                // Reassign
                RenderTexture.active = prevActive;
            }
            // Release
            RenderTexture.ReleaseTemporary(renderTexture);            
        }

        /// <summary>
        /// Stop the media source and release resources.
        /// </summary>
        public void Dispose () {
            handler = null;
            VideoKitEvents.Instance.onFrame -= OnFrame;
            Texture2D.Destroy(readbackBuffer);
        }
        #endregion


        #region --Operations--
        private Action<PixelBuffer>? handler;
        private readonly IClock? clock;
        private readonly RenderTextureDescriptor descriptor;
        private int frameIdx;
        private Texture2D? readbackBuffer;

        private void OnFrame () {
            if (texture != null && frameIdx++ % (frameSkip + 1) == 0)
                Append(texture, clock?.timestamp ?? 0L);
        }

        private void Preprocess (Texture source, RenderTexture destination) {
            // Crop
            var cropDest = RenderTexture.GetTemporary(descriptor);
            ExtractRoI(source, cropDest);          
            // Watermark
            var watermarkDest = RenderTexture.GetTemporary(descriptor);
            ApplyWatermark(cropDest, watermarkDest);
            // Blit
            Graphics.Blit(watermarkDest, destination);
            RenderTexture.ReleaseTemporary(cropDest);
            RenderTexture.ReleaseTemporary(watermarkDest);
        }

        private void ExtractRoI (
            Texture source,
            RenderTexture destination
        ) {
            // Compute crop scale
            var frameSize = new Vector2(destination.width, destination.height);
            var ratio = new Vector2(
                frameSize.x / regionOfInterest.width,
                frameSize.y / regionOfInterest.height
            );
            var scale = Mathf.Max(ratio.x, ratio.y);
            // Compute draw rect
            var pixelSize = scale * frameSize;
            var minPoint = 0.5f * frameSize - scale * regionOfInterest.center;
            var maxPoint = minPoint + pixelSize;
            var drawRect = new Rect(minPoint.x, destination.height - maxPoint.y, pixelSize.x, pixelSize.y);
            // Render
            var prevActive = RenderTexture.active;
            RenderTexture.active = destination;
            GL.Clear(true, true, Color.clear);
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, destination.width, destination.height, 0);
            Graphics.DrawTexture(drawRect, source);
            GL.PopMatrix();
            RenderTexture.active = prevActive;
        }

        private void ApplyWatermark (
            Texture source,
            RenderTexture destination
        ) {
            // Render source
            var prevActive = RenderTexture.active;
            RenderTexture.active = destination;
            GL.Clear(true, true, Color.clear);
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, destination.width, destination.height, 0);
            Graphics.Blit(source, destination);
            // Render watermark
            if (watermark != null) {
                var rect = watermarkAspectFit ?
                    AspectFitRect(watermark, watermarkRect) :
                    ToRect(watermarkRect);
                rect.y = destination.height - rect.max.y; // required by `Graphics::DrawTexture`
                Graphics.DrawTexture(rect, watermark);
            }
            // Restore
            GL.PopMatrix();
            RenderTexture.active = prevActive;
        }
        #endregion


        #region --Utility--

        private static Rect AspectFitRect (Texture watermark, RectInt frame) {
            var frameAspect = (float)frame.width / frame.height;
            var textureAspect = (float)watermark.width / watermark.height;
            var fitToWidth = textureAspect > frameAspect;
            var width = fitToWidth ? frame.width : frame.height * textureAspect;
            var height = fitToWidth ? frame.width / textureAspect : frame.height;
            var x = 0.5f * (frame.width - width);
            var y = 0.5f * (frame.height - height);
            return new(frame.x + x, frame.y + y, width, height);
        }

        private static Rect ToRect (RectInt rect) => new(rect.x, rect.y, rect.width, rect.height);
        #endregion
    }
}