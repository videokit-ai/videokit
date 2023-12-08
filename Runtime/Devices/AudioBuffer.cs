/*
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Devices {

    using System;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Internal;

    /// <summary>
    /// Audio buffer provided by an audio device.
    /// The audio buffer always contains a linear PCM sample buffer interleaved by channel.
    /// </summary>
    public readonly struct AudioBuffer {

        #region --Client API--
        /// <summary>
        /// Audio device that this buffer was generated from.
        /// </summary>
        public readonly AudioDevice device;

        /// <summary>
        /// Audio sample buffer.
        /// </summary>
        public unsafe readonly NativeArray<float> sampleBuffer;

        /// <summary>
        /// Audio buffer sample rate.
        /// </summary>
        public readonly int sampleRate;

        /// <summary>
        /// Audio buffer channel count.
        /// </summary>
        public readonly int channelCount;

        /// <summary>
        /// Audio buffer timestamp in nanoseconds.
        /// The timestamp is based on the system media clock.
        /// </summary>
        public readonly long timestamp;

        /// <summary>
        /// Safely clone the audio buffer.
        /// The clone will never contain a valid sample buffer
        /// </summary>
        public AudioBuffer Clone () => new AudioBuffer(
            device,
            default,
            sampleRate,
            channelCount,
            timestamp
        );
        #endregion


        #region --Operations--
        internal readonly IntPtr nativeBuffer;

        internal unsafe AudioBuffer (
            AudioDevice device,
            IntPtr audioBuffer
        ) {
            this.device = device;
            this.nativeBuffer = audioBuffer;
            this.sampleBuffer = Wrap(audioBuffer.AudioBufferData(), audioBuffer.AudioBufferSampleCount());
            this.sampleRate = audioBuffer.AudioBufferSampleRate();
            this.channelCount = audioBuffer.AudioBufferChannelCount();
            this.timestamp = audioBuffer.AudioBufferTimestamp();
        }

        private AudioBuffer (
            AudioDevice device,
            NativeArray<float> sampleBuffer,
            int sampleRate,
            int channelCount,
            long timestamp
        ) {
            this.device = device;
            this.sampleBuffer = sampleBuffer;
            this.sampleRate = sampleRate;
            this.channelCount = channelCount;
            this.timestamp = timestamp;
            this.nativeBuffer = default;
        }

        private static unsafe NativeArray<float> Wrap (float* buffer, int size) {
            var nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float>(buffer, size, Allocator.None);
            #if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, AtomicSafetyHandle.Create());
            #endif
            return nativeArray;
        }

        private static NativeArray<float> Wrap (float[] buffer) => new NativeArray<float>(buffer, Allocator.Temp);
        #endregion
    }
}