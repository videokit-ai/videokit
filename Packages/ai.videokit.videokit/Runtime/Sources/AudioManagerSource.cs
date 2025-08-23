/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
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
        /// <summary>
        /// Create an audio manager source.
        /// </summary>
        /// <param name="audioManager">Audio manager.</param>
        /// <param name="handler">Handler to receive audio buffers.</param>
        /// <param name="clock">Clock for generating timestamps.</param>
        public AudioManagerSource(
            VideoKitAudioManager audioManager,
            Action<AudioBuffer> handler,
            IClock? clock = null
        ) {
            this.audioManager = audioManager;
            this.handler = handler;
            this.clock = clock;
            audioManager.OnAudioBuffer += OnAudioBuffer;
        }

        public void Dispose() => audioManager.OnAudioBuffer -= OnAudioBuffer;
        #endregion


        #region --Operations--
        private readonly VideoKitAudioManager audioManager;
        private readonly Action<AudioBuffer> handler;
        private readonly IClock? clock;

        private void OnAudioBuffer(AudioBuffer srcBuffer) {
            using var audioBuffer = new AudioBuffer(
                srcBuffer.sampleRate,
                srcBuffer.channelCount,
                srcBuffer.data,
                clock?.timestamp ?? 0L
            );
            handler(audioBuffer);
        }
        #endregion
    }
}