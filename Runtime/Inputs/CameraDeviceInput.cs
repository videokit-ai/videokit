/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Recorders.Inputs {

    using System;
    using Clocks;
    using Devices;

    /// <summary>
    /// Recorder input for recording video frames from a camera device.
    /// </summary>
    public sealed class CameraDeviceInput : IDisposable {

        #region --Client API--
        /// <summary>
        /// Number of successive camera frames to skip while recording.
        /// This is very useful for GIF recording, which typically has a lower framerate appearance.
        /// </summary>
        public int frameSkip;
    
        /// <summary>
        /// Create a camera device input.
        /// </summary>
        /// <param name="recorder">Media recorder to receive camera frames.</param>
        /// <param name="clock">Clock for generating timestamps.</param>
        /// <param name="cameraManager">Camera manager with running camera.</param>
        public CameraDeviceInput (MediaRecorder recorder, IClock clock, VideoKitCameraManager cameraManager) : this(TextureInput.CreateDefault(recorder), clock, cameraManager) { }

        /// <summary>
        /// Create a camera device input.
        /// </summary>
        /// <param name="recorder">Media recorder to receive camera frames</param>
        /// <param name="cameraManager">Camera manager with running camera.</param>
        public CameraDeviceInput (MediaRecorder recorder, VideoKitCameraManager cameraManager) : this(recorder, default, cameraManager) { }

        /// <summary>
        /// Create a camera device input.
        /// </summary>
        /// <param name="input">Texture input to receive camera frames.</param>
        /// <param name="clock">Clock for generating timestamps.</param>
        /// <param name="cameraManager">Camera manager with running camera.</param>
        public CameraDeviceInput (TextureInput input, IClock clock, VideoKitCameraManager cameraManager) {
            this.input = input;
            this.clock = clock;
            this.cameraManager = cameraManager;
            this.frameIdx = 0;
            cameraManager.OnCameraFrame.AddListener(OnCameraFrame);
        }

        /// <summary>
        /// Create a camera device input.
        /// </summary>
        /// <param name="input">Texture input to receive camera frames.</param>
        /// <param name="cameraManager">Camera manager with running camera.</param>
        public CameraDeviceInput (TextureInput input, VideoKitCameraManager cameraManager) : this(input, default, cameraManager) { }

        /// <summary>
        /// Stop the recorder input and release resources.
        /// </summary>
        public void Dispose () {
            cameraManager.OnCameraFrame.RemoveListener(OnCameraFrame);
            input.Dispose();
        }
        #endregion


        #region --Operations--
        private readonly TextureInput input;
        private readonly IClock clock;
        private readonly VideoKitCameraManager cameraManager;
        private int frameIdx;

        private void OnCameraFrame () {
            // Check index
            if (frameIdx++ % (frameSkip + 1) != 0)
                return;
            // Commit
            input.CommitFrame(cameraManager.texture, clock?.timestamp ?? 0L);
        }
        #endregion
    }
}