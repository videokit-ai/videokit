/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Sources {

    using System;
    using Clocks;
    using UI;

    /// <summary>
    /// Media source for generating pixel buffers from a camera device.
    /// </summary>
    internal sealed class CameraViewSource : IDisposable {

        #region --Client API--
        /// <summary>
        /// Control number of successive camera frames to skip while recording.
        /// This is very useful for GIF recording, which typically has a lower framerate appearance.
        /// </summary>
        public int frameSkip;

        /// <summary>
        /// Create a camera device source.
        /// </summary>
        /// <param name="view">Camera view to capture pixel buffers from.</param>
        /// <param name="handler"></param>
        /// <param name="clock">Clock for generating pixel buffer timestamps.</param>
        /// <exception cref="ArgumentException">Raised when the camera preview is not running.</exception>
        public CameraViewSource(
            VideoKitCameraView view,
            Action<PixelBuffer> handler,
            IClock? clock = null
        ) {
            if (view.texture == null)
                throw new ArgumentException("Cannot create camera view source because camera manager is not running");
            this.handler = handler;
            this.clock = clock;
            this.view = view;
            view.OnPixelBuffer += OnPixelBuffer;
        }

        /// <summary>
        /// Stop the camera view source and release resources.
        /// </summary>
        public void Dispose() => view.OnPixelBuffer -= OnPixelBuffer;
        #endregion


        #region --Operations--
        private readonly Action<PixelBuffer> handler;
        private readonly IClock? clock;
        private readonly VideoKitCameraView view;
        private int frameIdx;

        private void OnPixelBuffer(PixelBuffer pixelBuffer) {
            if (frameIdx++ % (frameSkip + 1) != 0)
                return;
            using var outputBuffer = new PixelBuffer(
                width: pixelBuffer.width,
                height: pixelBuffer.height,
                format: pixelBuffer.format,
                data: pixelBuffer.data,
                rowStride: pixelBuffer.rowStride,
                timestamp: clock?.timestamp ?? 0L,
                mirrored: pixelBuffer.verticallyMirrored
            );
            handler(outputBuffer);
        }
        #endregion
    }
}