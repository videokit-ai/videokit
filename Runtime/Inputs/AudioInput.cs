/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Recorders.Inputs {

    using System;
    using UnityEngine;
    using Clocks;

    /// <summary>
    /// Recorder input for recording audio frames from an `AudioListener` or `AudioSource`.
    /// </summary>
    public sealed class AudioInput : IDisposable {

        #region --Client API--
        /// <summary>
        /// Create an audio recording input from a scene's AudioListener.
        /// </summary>
        /// <param name="recorder">Media recorder to receive audio frames.</param>
        /// <param name="clock">Clock for generating timestamps. Can be `null` if recorder does not require timestamps.</param>
        /// <param name="listener">Audio listener for the current scene.</param>
        public AudioInput (MediaRecorder recorder, IClock clock, AudioListener listener) : this(recorder, clock, listener.gameObject) {}

        /// <summary>
        /// Create an audio recording input from a scene's AudioListener.
        /// </summary>
        /// <param name="recorder">Media recorder to receive audio frames.</param>
        /// <param name="listener">Audio listener for the current scene.</param>
        public AudioInput (MediaRecorder recorder, AudioListener listener) : this(recorder, default, listener) {}

        /// <summary>
        /// Create an audio recording input from an AudioSource.
        /// </summary>
        /// <param name="recorder">Media recorder to receive audio frames.</param>
        /// <param name="clock">Clock for generating timestamps. Can be `null` if recorder does not require timestamps.</param>
        /// <param name="source">Audio source to record.</param>
        public AudioInput (MediaRecorder recorder, IClock clock, AudioSource source) : this(recorder, clock, source.gameObject) {}

        /// <summary>
        /// Create an audio recording input from an AudioSource.
        /// </summary>
        /// <param name="recorder">Media recorder to receive audio frames.</param>
        /// <param name="source">Audio source to record.</param>
        public AudioInput (MediaRecorder recorder, AudioSource source) : this(recorder, default, source) {}

        /// <summary>
        /// Stop recorder input and release resources.
        /// </summary>
        public void Dispose () => AudioInputAttachment.DestroyImmediate(attachment);
        #endregion


        #region --Operations--
        private readonly MediaRecorder recorder;
        private readonly IClock clock;
        private readonly AudioInputAttachment attachment;

        private AudioInput (MediaRecorder recorder, IClock clock, GameObject gameObject) {
            this.recorder = recorder;
            this.clock = clock;
            this.attachment = gameObject.AddComponent<AudioInputAttachment>();
            this.attachment.sampleBufferDelegate = OnSampleBuffer;
        }

        private void OnSampleBuffer (float[] data) => recorder.CommitSamples(data, clock?.timestamp ?? 0L);

        private class AudioInputAttachment : MonoBehaviour {
            public Action<float[]> sampleBufferDelegate;
            private void OnAudioFilterRead (float[] data, int channels) => sampleBufferDelegate?.Invoke(data);
        }
        #endregion
    }
}