/* 
*   VideoKit
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.Inputs {

    using System;
    using NatML.Devices;
    using NatML.Recorders;
    using NatML.Recorders.Clocks;

    /// <summary>
    /// Recorder input for recoridng audio frames from an `AudioDevice`.
    /// </summary>
    public sealed class AudioDeviceInput : IDisposable {

        #region --Client API--
        /// <summary>
        /// Create an audio device input.
        /// </summary>
        /// <param name="recorder">Media recorder to receive audio frames.</param>
        /// <param name="clock">Clock for generating timestamps.</param>
        /// <param name="audioDevice">Audio device to record from.</param>
        public AudioDeviceInput (IMediaRecorder recorder, IClock clock, AudioDevice audioDevice) {
            this.recorder = recorder;
            this.clock = clock;
            this.audioDevice = audioDevice;
            audioDevice.StartRunning(OnAudioBuffer);
        }

        /// <summary>
        /// Create an audio device input.
        /// </summary>
        /// <param name="recorder">Media recorder to receive audio frames.</param>
        /// <param name="audioDevice">Audio device to record from.</param>
        public AudioDeviceInput (IMediaRecorder recorder, AudioDevice audioDevice) : this(recorder, default, audioDevice) { }

        /// <summary>
        /// Stop the recorder input and release resources.
        /// </summary>
        public void Dispose () => audioDevice.StopRunning();
        #endregion


        #region --Operations--
        private readonly IMediaRecorder recorder;
        private readonly IClock clock;
        private readonly AudioDevice audioDevice;

        private void OnAudioBuffer (AudioBuffer audioBuffer) => recorder.CommitSamples(audioBuffer.sampleBuffer, clock?.timestamp ?? 0L);
        #endregion
    }
}