/*
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Recorders {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Internal;
    
    /// <summary>
    /// JPEG image sequence recorder.
    /// </summary>
    internal sealed class JPEGRecorder : MediaRecorder { // DEPRECATE

        #region --Client API--
        /// <summary>
        /// Image size.
        /// </summary>
        public override (int width, int height) frameSize => size;

        /// <summary>
        /// Create a JPEG image sequence recorder.
        /// </summary>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="quality">Encoding quality in range [1, 100].</param>
        /// <param name="directory">Subdirectory name to save recordings. This will be created if it does not exist.</param>
        public JPEGRecorder (int width, int height, int quality = 80, string prefix = null) : base(MediaFormat.JPEG) {
            // Save state
            this.size = (width, height);
            this.quality = Mathf.Clamp(quality, 1, 100);
            this.writeTasks = new List<Task>();
            // Create directory
            this.recordingDir = MediaRecorder.CreatePath(prefix: prefix);
            Directory.CreateDirectory(recordingDir);
        }

        /// <inheritdoc />
        public override unsafe void CommitFrame<T> (T[] pixelBuffer, long timestamp = default) {
            fixed (T* baseAddress = pixelBuffer)
                CommitFrame(baseAddress, timestamp);
        }

        /// <inheritdoc />
        public override unsafe void CommitFrame<T> (NativeArray<T> pixelBuffer, long timestamp = default) => CommitFrame(
            pixelBuffer.GetUnsafeReadOnlyPtr(),
            timestamp
        );

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override unsafe void CommitFrame (void* pixelBuffer, long timestamp = default) {
            // Encode immediately
            var nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(
                pixelBuffer,
                frameSize.width * frameSize.height * 4,
                Allocator.None
            );
            using var buffer = ImageConversion.EncodeNativeArrayToJPG(
                nativeArray,
                GraphicsFormat.R8G8B8A8_UNorm,
                (uint)frameSize.width,
                (uint)frameSize.height,
                quality: quality
            );
            // Write on background thread
            var imageIndex = ++frameCount;
            var imagePath = Path.Combine(recordingDir, $"{imageIndex}.jpg");
            var imageData = buffer.ToArray();
            var task = Task.Run(() => File.WriteAllBytes(imagePath, imageData));
            writeTasks.Add(task);
        }

        /// <summary>
        /// This is not supported and is a nop.
        /// </summary>
        public override unsafe void CommitSamples (float[] sampleBuffer, long timestamp) {
            fixed (float* baseAddress = sampleBuffer)
                CommitSamples(baseAddress, sampleBuffer.Length, timestamp);
        }

        /// <summary>
        /// This is not supported and is a nop.
        /// </summary>
        public override unsafe void CommitSamples (NativeArray<float> sampleBuffer, long timestamp) => CommitSamples(
            (float*)sampleBuffer.GetUnsafeReadOnlyPtr(),
            sampleBuffer.Length,
            timestamp
        );

        /// <summary>
        /// This is not supported and is a nop.
        /// </summary>
        public override unsafe void CommitSamples (float* sampleBuffer, int sampleCount, long timestamp) {
            Debug.LogError("JPEGRecorder does not support committing audio samples");
        }

        /// <summary>
        /// Finish writing.
        /// </summary>
        /// <returns>Path to recorded images separated by `Path.PathSeparator` character.</returns>
        public override async Task<string> FinishWriting () {
            await Task.WhenAll(writeTasks);
            var recordingPath = string.Join(Path.PathSeparator, Enumerable.Range(0, frameCount).Select(idx => $"{idx}.jpg"));
            return recordingPath;
        }
        #endregion


        #region --Operations--
        private readonly (int width, int height) size;
        private readonly int quality;
        private readonly string recordingDir;
        private readonly List<Task> writeTasks;
        private int frameCount;
        #endregion
    }
}