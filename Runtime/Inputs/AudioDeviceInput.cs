/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Recorders.Inputs {

    using System;
    using Devices;
    using Clocks;

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
        public AudioDeviceInput (MediaRecorder recorder, IClock clock, VideoKitAudioManager audioManager) {
            this.recorder = recorder;
            this.clock = clock;
            this.audioManager = audioManager;
            audioManager.OnAudioBuffer += OnAudioBuffer;
        }

        /// <summary>
        /// Create an audio device input.
        /// </summary>
        /// <param name="recorder">Media recorder to receive audio frames.</param>
        /// <param name="audioManager">Audio manager with running device.</param>
        public AudioDeviceInput (MediaRecorder recorder, VideoKitAudioManager audioManager) : this(recorder, default, audioManager) { }

        /// <summary>
        /// Stop the recorder input and release resources.
        /// </summary>
        public void Dispose () => audioManager.OnAudioBuffer -= OnAudioBuffer;
        #endregion


        #region --Operations--
        private readonly MediaRecorder recorder;
        private readonly IClock clock;
        private readonly VideoKitAudioManager audioManager;

        private void OnAudioBuffer (AudioBuffer audioBuffer) => recorder.CommitSamples(
            audioBuffer.sampleBuffer,
            clock?.timestamp ?? 0L
        );
        #endregion
    }
}