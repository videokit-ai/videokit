/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
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
        /// <param name="recorder">Media recorder to receive pixel buffers</param>
        /// <param name="clock">Clock for generating pixel buffer timestamps.</param>
        /// <exception cref="ArgumentException"></exception>
        public CameraViewSource (
            VideoKitCameraView view,
            MediaRecorder recorder,
            IClock? clock = null
        ) : this(view, recorder.Append, clock) {
            if (recorder.width != view.texture!.width || recorder.height != view.texture.height)
                throw new ArgumentException("Cannot create camera view source because camera preview resolution does not match recorder resolution");
        }

        /// <summary>
        /// Create a camera device source.
        /// </summary>
        /// <param name="view">Camera view to capture pixel buffers from.</param>
        /// <param name="handler"></param>
        /// <param name="clock">Clock for generating pixel buffer timestamps.</param>
        /// <exception cref="ArgumentException">Raised when the camera preview is not running.</exception>
        public CameraViewSource (
            VideoKitCameraView view,
            Action<PixelBuffer> handler,
            IClock? clock = null
        ) {
            if (view.texture == null)
                throw new ArgumentException("Cannot create camera view source because camera manager is not running");
            this.handler = handler;
            this.clock = clock;
            this.view = view;
            view.OnCameraFrame?.AddListener(OnFrame);
        }

        public void Dispose () => view.OnCameraFrame?.RemoveListener(OnFrame);
        #endregion


        #region --Operations--
        private readonly Action<PixelBuffer> handler;
        private readonly IClock? clock;
        private readonly VideoKitCameraView view;
        private int frameIdx;

        private void OnFrame () {
            // Check frame index
            if (frameIdx++ % (frameSkip + 1) != 0)
                return;
            // Invoke
            var texture = view.texture!;
            using var pixelBuffer = new PixelBuffer(
                texture.width,
                texture.height,
                PixelBuffer.Format.RGBA8888,
                texture.GetRawTextureData(),
                timestamp: clock?.timestamp ?? 0L
            );
            handler?.Invoke(pixelBuffer);
        }
        #endregion
    }
}