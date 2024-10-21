/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Sources {

    using System;
    using Clocks;

    /// <summary>
    /// Media source for generating pixel buffers from a camera device.
    /// </summary>
    internal sealed class CameraManagerSource : IDisposable {

        #region --Client API--
        public CameraManagerSource (
            VideoKitCameraManager cameraManager,
            MediaRecorder recorder,
            IClock? clock = null
        ) : this(cameraManager, recorder.Append, clock) {
            if (recorder.width != width || recorder.height != height)
                throw new ArgumentException("Cannot create camera manager source because camera preview resolution does not match recorder resolution");
        }

        public CameraManagerSource (
            VideoKitCameraManager cameraManager,
            Action<PixelBuffer> handler,
            IClock? clock = null
        ) {
            if (cameraManager.texture == null)
                throw new ArgumentException("Cannot create camera manager source because camera manager is not running");
            this.handler = handler;
            this.clock = clock;
            this.cameraManager = cameraManager;
            this.width = cameraManager.texture!.width;
            this.height = cameraManager.texture!.height;
            cameraManager.OnPixelBuffer += OnPixelBuffer;
        }

        public void Dispose () => cameraManager.OnPixelBuffer -= OnPixelBuffer;
        #endregion


        #region --Operations--
        private readonly Action<PixelBuffer> handler;
        private readonly IClock? clock;
        private readonly VideoKitCameraManager cameraManager;
        private readonly int width;
        private readonly int height;

        private void OnPixelBuffer (PixelBuffer srcBuffer) {
            using var pixelBuffer = new PixelBuffer(
                width,
                height,
                PixelBuffer.Format.RGBA8888,
                cameraManager.previewData,
                timestamp: clock?.timestamp ?? 0L
            );
            handler?.Invoke(pixelBuffer);
        }
        #endregion
    }
}