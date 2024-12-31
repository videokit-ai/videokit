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
        /// <param name="handler">Handler to receive audio buffers.</param>
        /// <param name="listener">Audio listener to record.</param>
        public AudioComponentSource (Action<AudioBuffer> handler, AudioListener listener) : this(
            handler,
            default,
            listener.gameObject
        ) { }

        /// <summary>
        /// Create an audio buffer source from an AudioSource.
        /// </summary>
        /// <param name="handler">Handler to receive audio buffers.</param>
        /// <param name="source">Audio source to record.</param>
        public AudioComponentSource (Action<AudioBuffer> handler, AudioSource source) : this(
            handler,
            default,
            source.gameObject
        ) { }

        /// <summary>
        /// Create an audio buffer source from an AudioListener.
        /// </summary>
        /// <param name="handler">Handler to receive audio buffers.</param>
        /// <param name="clock">Clock for generating timestamps. Can be `null` if recorder does not require timestamps.</param>
        /// <param name="listener">Audio listener for the current scene.</param>
        public AudioComponentSource (Action<AudioBuffer> handler, IClock? clock, AudioListener listener) : this(
            handler,
            clock,
            listener.gameObject
        ) { }

        /// <summary>
        /// Create an audio buffer source from an AudioSource.
        /// </summary>
        /// <param name="handler">Handler to receive audio buffers.</param>
        /// <param name="clock">Clock for generating timestamps. Can be `null` if recorder does not require timestamps.</param>
        /// <param name="source">Audio source to record.</param>
        public AudioComponentSource (Action<AudioBuffer> handler, IClock? clock, AudioSource source) : this(
            handler,
            clock,
            source.gameObject
        ) { }

        /// <summary>
        /// Close the media source and release resources.
        /// </summary>
        public void Dispose () => AudioSourceAttachment.DestroyImmediate(attachment);
        #endregion


        #region --Operations--
        private readonly AudioSourceAttachment attachment;

        private AudioComponentSource (Action<AudioBuffer> handler, IClock? clock, GameObject gameObject) {
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

            private void OnAudioFilterRead (float[] data, int channels) {
                sampleBufferDelegate?.Invoke(data, channels);
            }
        }
        #endregion
    }
}