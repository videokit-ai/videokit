/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Sources {

    using System;
    using Clocks;

    /// <summary>
    /// Media source for recording audio buffers from a `VideoKitAudioManager`.
    /// </summary>
    internal sealed class AudioManagerSource : IDisposable {

        #region --Client API--
        public AudioManagerSource (MediaRecorder recorder, IClock? clock, VideoKitAudioManager audioManager) {
            this.recorder = recorder;
            this.clock = clock;
            this.audioManager = audioManager;
            audioManager.OnAudioBuffer += OnAudioBuffer;
        }

        public void Dispose () => audioManager.OnAudioBuffer -= OnAudioBuffer;
        #endregion


        #region --Operations--
        private readonly MediaRecorder recorder;
        private readonly IClock? clock;
        private readonly VideoKitAudioManager audioManager;

        private void OnAudioBuffer (AudioBuffer srcBuffer) {
            using var audioBuffer = new AudioBuffer(
                srcBuffer.sampleRate,
                srcBuffer.channelCount,
                srcBuffer.sampleBuffer,
                clock?.timestamp ?? 0L
            );
            recorder.Append(audioBuffer);
        }
        #endregion
    }
}