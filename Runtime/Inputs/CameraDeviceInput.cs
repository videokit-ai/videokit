/* 
*   VideoKit
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.Inputs {

    using System;
    using NatML.Recorders;
    using NatML.Recorders.Clocks;
    using NatML.Recorders.Inputs;

    /// <summary>
    /// Recorder input for recording video frames from a camera device.
    /// </summary>
    public sealed class CameraDeviceInput : IDisposable { // INCOMPLETE // Scaling

        #region --Client API--
        /// <summary>
        /// Create a camera device input.
        /// </summary>
        /// <param name="recorder">Media recorder to receive camera frames.</param>
        /// <param name="clock">Clock for generating timestamps.</param>
        /// <param name="cameraManager">Camera manager with running camera.</param>
        public CameraDeviceInput (IMediaRecorder recorder, IClock clock, VideoKitCameraManager cameraManager) {
            this.recorder = recorder;
            this.clock = clock;
            this.cameraManager = cameraManager;
            this.input = TextureInput.CreateDefault(recorder);
            cameraManager.OnFrame.AddListener(OnCameraFrame);
        }

        /// <summary>
        /// Create a camera device input.
        /// </summary>
        /// <param name="recorder">Media recorder to receive camera frames</param>
        /// <param name="cameraManager">Camera manager with running camera.</param>
        public CameraDeviceInput (IMediaRecorder recorder, VideoKitCameraManager cameraManager) : this(recorder, default, cameraManager) { }

        /// <summary>
        /// Stop the recorder input and release resources.
        /// </summary>
        public void Dispose () => cameraManager.OnFrame.RemoveListener(OnCameraFrame);
        #endregion


        #region --Operations--
        private readonly IMediaRecorder recorder;
        private readonly IClock clock;
        private readonly VideoKitCameraManager cameraManager;
        private readonly TextureInput input;

        private void OnCameraFrame (CameraFrame frame) => input.CommitFrame(frame, clock?.timestamp ?? 0L);
        #endregion
    }
}