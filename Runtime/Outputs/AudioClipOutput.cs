/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Devices.Outputs {

    using System;
    using System.IO;
    using UnityEngine;
    using Unity.Collections;
    using Devices;

    /// <summary>
    /// Audio device output that accumulates audio buffers into an `AudioClip`.
    /// </summary>
    public sealed class AudioClipOutput : AudioOutput {

        #region --Client API--
        /// <summary>
        /// Create an audio clip output.
        /// </summary>
        public AudioClipOutput () {
            this.sampleBuffer = new MemoryStream();
            this.fence = new object();
        }

        /// <summary>
        /// Update the output with a new audio buffer.
        /// </summary>
        /// <param name="audioBuffer">Audio buffer.</param>
        public override void Update (AudioBuffer audioBuffer) {
            lock (fence) {
                sampleRate = sampleRate == 0 ? audioBuffer.sampleRate : sampleRate;
                channelCount = channelCount == 0 ? audioBuffer.channelCount : channelCount;
                var audioData = new NativeSlice<float>(audioBuffer.sampleBuffer).SliceConvert<byte>().ToArray();
                sampleBuffer.Write(audioData, 0, audioData.Length);
                buffer = audioBuffer.Clone();
            }
        }

        /// <summary>
        /// Dispose the output and release resources.
        /// </summary>
        public override void Dispose () {
            lock (fence)
                sampleBuffer.Dispose();
        }

        /// <summary>
        /// Get the current clip containing all audio recorded up till this point.
        /// Note that this clip DOES NOT stream new audio that is provided to the output.
        /// </summary>
        public AudioClip ToClip () {
            lock (fence) {
                // Check
                if (sampleRate == 0 || channelCount == 0)
                    return null;
                // Get the full sample buffer
                var byteSamples = sampleBuffer.ToArray();
                var totalSampleCount = byteSamples.Length / sizeof(float); 
                var floatSamples = new float[totalSampleCount];  
                var recordingName = string.Format("recording_{0}", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff"));
                Buffer.BlockCopy(byteSamples, 0, floatSamples, 0, byteSamples.Length);
                // Create audio clip
                var audioClip = AudioClip.Create(recordingName, totalSampleCount / channelCount, channelCount, sampleRate, false);
                audioClip.SetData(floatSamples, 0);
                return audioClip;
            }
        }
        #endregion


        #region --Operations--
        private readonly MemoryStream sampleBuffer;
        private readonly object fence;
        private int sampleRate;
        private int channelCount;
        #endregion
    }
}