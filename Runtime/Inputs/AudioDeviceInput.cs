/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.Inputs {

    using System;
    using NatML.Devices;
    using NatML.Recorders;
    using NatML.Recorders.Clocks;

    /// <summary>
    /// Recorder input for recoridng audio sample buffers from an audio device.
    /// </summary>
    public sealed class AudioDeviceInput : IDisposable {

        #region --Client API--
        /// <summary>
        /// Create an audio device input.
        /// </summary>
        /// <param name="recorder">Media recorder to receive audio frames.</param>
        /// <param name="clock">Clock for generating timestamps.</param>
        /// <param name="audioManager">Audio manager with running device.</param>
        public AudioDeviceInput (IMediaRecorder recorder, IClock clock, VideoKitAudioManager audioManager) {
            this.recorder = recorder;
            this.clock = clock;
            this.audioManager = audioManager;
            audioManager.OnSampleBuffer.AddListener(OnSampleBuffer);
        }

        /// <summary>
        /// Create an audio device input.
        /// </summary>
        /// <param name="recorder">Media recorder to receive audio frames.</param>
        /// <param name="audioManager">Audio manager with running device.</param>
        public AudioDeviceInput (IMediaRecorder recorder, VideoKitAudioManager audioManager) : this(recorder, default, audioManager) { }

        /// <summary>
        /// Stop the recorder input and release resources.
        /// </summary>
        public void Dispose () => audioManager.OnSampleBuffer.RemoveListener(OnSampleBuffer);
        #endregion


        #region --Operations--
        private readonly IMediaRecorder recorder;
        private readonly IClock clock;
        private readonly VideoKitAudioManager audioManager;

        private void OnSampleBuffer (SampleBuffer sampleBuffer) => recorder.CommitSamples(
            sampleBuffer.audioBuffer.sampleBuffer,
            clock?.timestamp ?? 0L
        );
        #endregion
    }
}