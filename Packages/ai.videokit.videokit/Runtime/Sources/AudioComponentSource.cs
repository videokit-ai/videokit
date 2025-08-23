/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Sources {

    using System;
    using UnityEngine;
    using Clocks;

    /// <summary>
    /// Media source for generating audio buffers from an `AudioSource` or `AudioListener` component.
    /// </summary>
    public sealed class AudioComponentSource : IDisposable {

        #region --Client API--
        /// <summary>
        /// Create an audio buffer source from an AudioListener.
        /// </summary>
        /// <param name="listener">Audio listener for the current scene.</param>
        /// <param name="handler">Handler to receive audio buffers.</param>
        /// <param name="clock">Clock for generating timestamps.</param>
        public AudioComponentSource(
            AudioListener listener,
            Action<AudioBuffer> handler,
            IClock? clock = null
        ) : this(listener.gameObject, handler, clock) { }

        /// <summary>
        /// Create an audio buffer source from an AudioSource.
        /// </summary>
        /// <param name="source">Audio source to record.</param>
        /// <param name="handler">Handler to receive audio buffers.</param>
        /// <param name="clock">Clock for generating timestamps.</param>
        public AudioComponentSource(
            AudioSource source,
            Action<AudioBuffer> handler,
            IClock? clock = null
        ) : this(source.gameObject, handler, clock) { }

        /// <summary>
        /// Close the media source and release resources.
        /// </summary>
        public void Dispose() => AudioSourceAttachment.DestroyImmediate(attachment);
        #endregion


        #region --Operations--
        private readonly AudioSourceAttachment attachment;

        private AudioComponentSource(GameObject gameObject, Action<AudioBuffer> handler, IClock? clock) {
            var sampleRate = AudioSettings.outputSampleRate;
            attachment = gameObject.AddComponent<AudioSourceAttachment>();
            attachment.sampleBufferDelegate = (data, channels) => {
                using var audioBuffer = new AudioBuffer(
                    sampleRate,
                    channels,
                    data,
                    clock?.timestamp ?? 0L
                );
                handler(audioBuffer);
            };
        }

        private sealed class AudioSourceAttachment : MonoBehaviour {

            public Action<float[], int>? sampleBufferDelegate;

            private void OnAudioFilterRead(float[] data, int channels) {
                sampleBufferDelegate?.Invoke(data, channels);
            }
        }
        #endregion
    }
}