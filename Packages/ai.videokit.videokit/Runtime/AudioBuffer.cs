/*
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit {

    using System;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Internal;
    using Status = Internal.VideoKit.Status;

    /// <summary>
    /// Audio buffer.
    /// The audio buffer always contains a linear PCM audio interleaved by channel.
    /// </summary>
    public unsafe readonly struct AudioBuffer : IDisposable {

        #region --Client API--
        /// <summary>
        /// Audio data.
        /// This is always linear PCM audio interleaved by channel.
        /// </summary>
        public unsafe readonly NativeArray<float> data {
            get {
                audioBuffer.GetAudioBufferData(out var data).Throw();
                audioBuffer.GetAudioBufferSampleCount(out var sampleCount).Throw();
                var nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float>(
                    data,
                    sampleCount,
                    Allocator.None
                );
                #if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, AtomicSafetyHandle.Create());
                #endif
                return nativeArray;
            }
        }

        /// <summary>
        /// Sample rate.
        /// </summary>
        public readonly int sampleRate => audioBuffer.GetAudioBufferSampleRate(out var sampleRate).Throw() == Status.Ok ? sampleRate : 0;

        /// <summary>
        /// Channel count.
        /// </summary>
        public readonly int channelCount => audioBuffer.GetAudioBufferChannelCount(out var channelCount).Throw() == Status.Ok ? channelCount : 0;

        /// <summary>
        /// Timestamp in nanoseconds.
        /// </summary>
        public readonly long timestamp => audioBuffer.GetSampleBufferTimestamp(out var timestamp).Throw() == Status.Ok ? timestamp : 0L;

        /// <summary>
        /// Create an audio buffer from a linear PCM sample buffer.
        /// NOTE: This overload makes a copy of the input buffer, so prefer using the other overloads instead.
        /// </summary>
        /// <param name="sampleRate">Sample rate.</param>
        /// <param name="channelCount">Channel count.</param>
        /// <param name="data">Audio data.</param>
        /// <param name="timestamp">Timestamp in nanoseconds.</param>
        public AudioBuffer (
            int sampleRate,
            int channelCount,
            float[] data,
            long timestamp = 0L
        ) {
            // Copy
            var byteSize = data.Length * sizeof(float);
            audioData = (float*)UnsafeUtility.Malloc(byteSize, 16, Allocator.Persistent);
            fixed (float* src = data)
                UnsafeUtility.MemCpy(audioData, src, byteSize);
            // Create
            VideoKit.CreateAudioBuffer(
                sampleRate,
                channelCount,
                audioData,
                data.Length,
                timestamp,
                out audioBuffer
            ).Throw();
        }

        /// <summary>
        /// Create an audio buffer from a linear PCM sample buffer.
        /// </summary>
        /// <param name="sampleRate">Sample rate.</param>
        /// <param name="channelCount">Channel count.</param>
        /// <param name="data">Audio data.</param>
        /// <param name="timestamp">Timestamp in nanoseconds.</param>
        public unsafe AudioBuffer (
            int sampleRate,
            int channelCount,
            NativeArray<float> data,
            long timestamp = 0L
        ) : this(sampleRate, channelCount, (float*)data.GetUnsafePtr(), data.Length, timestamp) { }

        /// <summary>
        /// Create an audio buffer from a linear PCM sample buffer.
        /// </summary>
        /// <param name="sampleRate">Sample rate.</param>
        /// <param name="channelCount">Channel count.</param>
        /// <param name="data">Audio data.</param>
        /// <param name="sampleCount">Total number of samples in sample buffer.</param>
        /// <param name="timestamp">Timestamp in nanoseconds.</param>
        public unsafe AudioBuffer (
            int sampleRate,
            int channelCount,
            float* data,
            int sampleCount,
            long timestamp = 0L
        ) : this(VideoKit.CreateAudioBuffer(
            sampleRate,
            channelCount,
            data,
            sampleCount,
            timestamp,
            out var buffer
        ).Throw() == Status.Ok ? buffer : default) { }

        /// <summary>
        /// Dispose the audio buffer and release resources.
        /// </summary>
        public void Dispose () {
            audioBuffer.ReleaseSampleBuffer();
            UnsafeUtility.Free(audioData, Allocator.Persistent);
        }
        #endregion


        #region --Operations--
        private readonly IntPtr audioBuffer;
        private readonly float* audioData;

        internal AudioBuffer (IntPtr buffer) {
            this.audioBuffer = buffer;
            this.audioData = null;
        }

        public static implicit operator IntPtr (AudioBuffer audioBuffer) => audioBuffer.audioBuffer;
        #endregion
    }
}