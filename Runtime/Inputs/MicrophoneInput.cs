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
    /// Recorder input for recoridng audio frames from a microphone.
    /// </summary>
    public sealed class MicrophoneInput : IDisposable {

        #region --Client API--
        /// <summary>
        /// Create a microphone input.
        /// </summary>
        /// <param name="recorder">Media recorder to receive audio frames.</param>
        /// <param name="clock">Clock for generating timestamps. Can be `null` if recorder does not require timestamps.</param>
        /// <param name="audioDevice">Audio device to record from.</param>
        public MicrophoneInput (IMediaRecorder recorder, IClock clock, AudioDevice audioDevice) {
            this.recorder = recorder;
            this.clock = clock;
            this.audioDevice = audioDevice;
            audioDevice.StartRunning(OnAudioBuffer);
        }

        /// <summary>
        /// Create a microphone input.
        /// </summary>
        /// <param name="recorder">Media recorder to receive audio frames.</param>
        /// <param name="audioDevice">Audio device to record from.</param>
        public MicrophoneInput (IMediaRecorder recorder, AudioDevice audioDevice) : this(recorder, default, audioDevice) { }

        /// <summary>
        /// Stop recorder input and release resources.
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