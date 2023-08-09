/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Internal {

    using AOT;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using UnityEngine;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Recorders;

    /// <summary>
    /// Native media recorder.
    /// </summary>
    internal sealed class NativeRecorder : MediaRecorder {
        
        #region --Client API--
        /// <inheritdoc />
        public override (int width, int height) frameSize {
            get {
                recorder.FrameSize(out var width, out var height).CheckStatus();
                return (width, height);
            }
        }

        /// <inheritdoc />
        public override unsafe void CommitFrame<T> (T[] pixelBuffer, long timestamp) {
            fixed (T* baseAddress = pixelBuffer)
                CommitFrame(baseAddress, timestamp);
        }

        /// <inheritdoc />
        public override unsafe void CommitFrame<T> (NativeArray<T> pixelBuffer, long timestamp) {
            CommitFrame(pixelBuffer.GetUnsafeReadOnlyPtr(), timestamp);
        }

        /// <inheritdoc />
        public override unsafe void CommitFrame (void* pixelBuffer, long timestamp) => recorder.CommitFrame(
            pixelBuffer,
            timestamp
        ).CheckStatus();

        /// <inheritdoc />
        public override unsafe void CommitSamples (float[] sampleBuffer, long timestamp) {
            fixed (float* baseAddress = sampleBuffer)
                CommitSamples(baseAddress, sampleBuffer.Length, timestamp);
        }

        /// <inheritdoc />
        public override unsafe void CommitSamples (NativeArray<float> sampleBuffer, long timestamp) => CommitSamples(
            (float*)sampleBuffer.GetUnsafeReadOnlyPtr(),
            sampleBuffer.Length,
            timestamp
        );

        /// <inheritdoc />
        public override unsafe void CommitSamples (float* sampleBuffer, int sampleCount, long timestamp) => recorder.CommitSamples(
            sampleBuffer,
            sampleCount,
            timestamp
        ).CheckStatus();

        /// <inheritdoc />
        public override unsafe Task<string> FinishWriting () {
            var tcs = new TaskCompletionSource<string>();
            var handle = GCHandle.Alloc(tcs, GCHandleType.Normal);
            try {
                recorder.FinishWriting(OnRecorderCompleted, (IntPtr)handle).CheckStatus();
            } catch (Exception ex) {
                tcs.SetException(ex);
                handle.Free();
            }
            return tcs.Task;
        }
        #endregion


        #region --Operations--
        private readonly IntPtr recorder;

        internal NativeRecorder (MediaFormat format, IntPtr recorder) : base(format) {
            // Check
            if (recorder == IntPtr.Zero)
                throw new InvalidOperationException(@"Failed to create media recorder. Check the logs for more info.");
            // Set
            this.recorder = recorder;
        }

        [MonoPInvokeCallback(typeof(VideoKit.RecordingHandler))]
        private static unsafe void OnRecorderCompleted (IntPtr context, IntPtr path) {
            var handle = (GCHandle)context;
            var tcs = handle.Target as TaskCompletionSource<string>;
            handle.Free();
            if (path != IntPtr.Zero)
                tcs?.SetResult(Marshal.PtrToStringAnsi(path));
            else
                tcs?.SetException(new Exception(@"Recorder failed to finish writing"));
        }

        public static implicit operator IntPtr (NativeRecorder recorder) => recorder.recorder;
        #endregion
    }
}