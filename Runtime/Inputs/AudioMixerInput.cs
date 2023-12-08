/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Recorders.Inputs {

    using System;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using Clocks;
    using Devices;
    using Utilities;

    /// <summary>
    /// </summary>
    internal sealed class AudioMixerInput : IDisposable { // INCOMPLETE // Sync

        #region --Client API--
        /// <summary>
        /// Gain multiplier to apply to audio from audio device .
        /// </summary>
        public float audioDeviceGain = 1f;

        /// <summary>
        /// Create an audio mixer input.
        /// </summary>
        /// <param name="recorder">Media recorder to receive audio frames.</param>
        /// <param name="clock">Clock for generating timestamps. Can be `null` if recorder does not require timestamps.</param>
        /// <param name="audioDevice">Audio device to record microphone audio from.</param>
        /// <param name="audioListener">Audio listener to record game audio from.</param>
        public AudioMixerInput (MediaRecorder recorder, IClock clock, VideoKitAudioManager audioManager, AudioListener audioListener) {
            this.recorder = recorder;
            this.clock = clock;
            this.audioManager = audioManager;
            this.attachment = audioListener.gameObject.AddComponent<AudioMixerInputAttachment>();
            this.deviceRingBuffer = new RingBuffer<float>(RingBufferSize);
            this.unityRingBuffer = new RingBuffer<float>(RingBufferSize);
            this.deviceSampleBuffer = new float[MixBufferSize];
            this.unitySampleBuffer = new float[MixBufferSize];
            this.mixedBuffer = new float[MixBufferSize];
            this.signal = new SharedSignal(2);
            this.deviceFence = new object();
            this.unityFence = new object();
            signal.OnSignal += ClearBuffers;
            audioManager.OnAudioBuffer += OnDeviceSampleBuffer;
            attachment.OnSampleBuffer += OnUnitySampleBuffer;
        }

        /// <summary>
        /// Create an audio mixer input.
        /// </summary>
        /// <param name="recorder">Media recorder to receive audio frames.</param>
        /// <param name="audioDevice">Audio device to record microphone audio from.</param>
        /// <param name="audioListener">Audio listener to record game audio from.</param>
        public AudioMixerInput (MediaRecorder recorder, VideoKitAudioManager audioManager, AudioListener audioListener) : this(recorder, default, audioManager, audioListener) { }

        /// <summary>
        /// Stop the recorder input and release resources.
        /// </summary>
        public void Dispose () {
            AudioMixerInputAttachment.DestroyImmediate(attachment);
            audioManager.OnAudioBuffer -= OnDeviceSampleBuffer;
        }
        #endregion


        #region --Operations--
        private readonly MediaRecorder recorder;
        private readonly IClock clock;
        private readonly VideoKitAudioManager audioManager;
        private readonly AudioMixerInputAttachment attachment;
        private readonly RingBuffer<float> deviceRingBuffer;
        private readonly RingBuffer<float> unityRingBuffer;
        private readonly float[] deviceSampleBuffer;
        private readonly float[] unitySampleBuffer;
        private readonly float[] mixedBuffer;
        private readonly SharedSignal signal;
        private readonly object deviceFence;
        private readonly object unityFence;
        private const int RingBufferSize = 16384;
        private const int MixBufferSize = 1024;

        private void ClearBuffers () {
            lock (deviceFence)
                lock (unityFence) {
                    deviceRingBuffer.Clear();
                    unityRingBuffer.Clear();
                }
        }

        private void MixBuffers () {
            while (true) {
                // Read
                lock (deviceFence)
                    lock (unityFence) {
                        if (deviceRingBuffer.Length < MixBufferSize || unityRingBuffer.Length < MixBufferSize)
                            return;
                        deviceRingBuffer.Read(deviceSampleBuffer);
                        unityRingBuffer.Read(unitySampleBuffer);
                    }
                // Mix
                Mix(deviceSampleBuffer, unitySampleBuffer, mixedBuffer);
                // Commit
                recorder.CommitSamples(mixedBuffer, clock?.timestamp ?? 0L);
            }
        }

        private void OnDeviceSampleBuffer (AudioBuffer audioBuffer) {
            signal.Signal(deviceFence);
            if (!signal.signaled)
                return;
            lock (deviceFence)
                deviceRingBuffer.Write(audioBuffer.sampleBuffer);
            MixBuffers();
        }

        private void OnUnitySampleBuffer (float[] sampleBuffer) {
            signal.Signal(unityFence);
            if (!signal.signaled)
                return;
            lock (unityFence)
                unityRingBuffer.Write(sampleBuffer);
            MixBuffers();
        }

        private void Mix (float[] srcA, float[] srcB, float[] dst) {
            for (var i = 0; i < dst.Length; ++i)
                dst[i] = FTH(audioDeviceGain * srcA[i] + srcB[i]);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float FTH (float x) => 1f - (2f * (1f / (1f + Mathf.Exp(2 * x))));
        }

        private class AudioMixerInputAttachment : MonoBehaviour {
            public event Action<float[]> OnSampleBuffer;
            private void OnAudioFilterRead (float[] data, int channels) => OnSampleBuffer?.Invoke(data);
        }
        #endregion
    }
}