/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit {

    using System;
    using System.Buffers;
    using System.Collections.Concurrent;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Clocks;

    /// <summary>
    /// Replay buffer for recording the last several seconds of video.
    /// NOTE: This only supports recording video.
    /// NOTE: This is not supported on WebGL due to the lack of C# multithreading..
    /// </summary>
    public sealed class ReplayBuffer {

        #region --Client API--
        /// <summary>
        /// Create a replay buffer recorder.
        /// </summary>
        /// <param name="format">Recording format.</param>
        /// <param name="width">Video width.</param>
        /// <param name="height">Video height.</param>
        /// <param name="frameRate">Video frame rate.</param>
        /// <param name="duration">Video duration in seconds.</param>
        /// <param name="prefix">Subdirectory name to save recordings. This will be created if it does not exist.</param>
        public ReplayBuffer(
            MediaRecorder.Format format,
            int width,
            int height,
            float frameRate,
            float duration,
            string? prefix = null
        ) {
            this.width = width;
            this.height = height;
            this.frameRate = frameRate;
            this.duration = duration;
            this.chunkDurationNs = (duration + 2f) * 1e+9f; // add 2 second padding to each chunk
            this.prefix = prefix;
            this.clock = new();
            this.queue = new();
            this.finishSource = new();
            this.worker = new Thread(async () => {
                foreach (var action in queue.GetConsumingEnumerable())
                    action();
                finishSource.SetResult(true);
            });
            this.chunkTask = Task.FromResult<MediaAsset>(null!);
            worker.Start();
        }

        /// <summary>
        /// Append a pixel buffer.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer.</param>
        /// <param name="rotation">Rotation to apply to the pixel buffer.</param>
        public void Append(
            PixelBuffer pixelBuffer,
            PixelBuffer.Rotation rotation = PixelBuffer.Rotation._0
        ) {
            // Check size
            var portrait = rotation == PixelBuffer.Rotation._90 || rotation == PixelBuffer.Rotation._270;
            var width = portrait ? pixelBuffer.height : pixelBuffer.width;
            var height = portrait ? pixelBuffer.width : pixelBuffer.height;
            if (width != this.width || height != this.height)
                throw new ArgumentException($"Cannot append pixel buffer with size {width}x{height} to replay buffer with size {this.width}x{this.height}");
            // Copy pixel data from the camera buffer to an `RGBA8888` buffer
            var packet = new Packet(width, height, timestamp: clock.timestamp);
            pixelBuffer.CopyTo(packet.buffer, rotation);
            // Post work
            queue.Add(() => FlushPacket(packet));
        }

        /// <summary>
        /// Finish writing and return the replay buffer video.
        /// </summary>
        public async Task<MediaAsset?> FinishWriting() {
            // Wait until writer thread is done
            queue.CompleteAdding();
            await finishSource.Task;
            // Check
            if (recorder == null)
                throw new InvalidOperationException(@"Recorder failed to finish writing because no pixel buffers were appended");
            // Finish writing
            var result = await recorder.FinishWriting();
            // Concatenate the previous chunk to the final one if the final one is not long enough
            var chunk = await chunkTask;
            if (result.duration < duration && chunk != null)
                result = await MediaAsset.Concatenate(chunk, result);
            // Trim the result if longer than duration
            if (result.duration > duration)
                result = await MediaAsset.TakeLast(result, duration);
            // Return
            return result;
        }
        #endregion


        #region --Operations--
        private readonly MediaRecorder.Format format;
        private readonly int width;
        private readonly int height;
        private readonly float frameRate;
        private readonly float duration;
        private readonly float chunkDurationNs;
        private readonly string? prefix;
        private readonly RealtimeClock clock;
        private readonly BlockingCollection<Action> queue;
        private readonly TaskCompletionSource<bool> finishSource;
        private readonly Thread worker;
        private MediaRecorder? recorder;
        private ulong recorderIdx;
        private Task<MediaAsset> chunkTask;

        private void FlushPacket(in Packet packet) {
            // Create a new chunk if we hit duration
            var chunkIdx = (ulong)(packet.buffer.timestamp / chunkDurationNs);
            if (recorderIdx < chunkIdx && recorder != null) {
                chunkTask = recorder.FinishWriting();
                recorder = null;
            }
            // Create recorder
            if (recorder == null) {
                recorder = MediaRecorder.Create( // CHECK
                    format: format,
                    width: width,
                    height: height,
                    frameRate: frameRate,
                    sampleRate: 0,
                    channelCount: 0,
                    keyframeInterval: 1,
                    prefix: prefix
                ).Result;
                recorderIdx = chunkIdx;
            }
            // Append frame
            recorder.Append(packet.buffer);
            packet.Dispose();
        }
        #endregion


        #region --Encoder Packets--
        /// <summary>
        /// This is basically a wrapper of a `PixelBuffer` along with bits required to save on allocations.
        /// </summary>
        private readonly struct Packet : IDisposable {

            public readonly byte[] data; // allocated via `ArrayPool<T>`
            public readonly GCHandle handle; // pins `data` so GC keeps it fixed
            public readonly PixelBuffer buffer; // wraps `data`

            /// <summary>
            /// Create a packet which contains a `PixelBuffer` backed by data allocated from a memory pool.
            /// </summary>
            /// <param name="width">Pixel buffer width.</param>
            /// <param name="height">Pixel buffer height.</param>
            /// <param name="timestamp">Pixel buffer timestamp.</param>
            public unsafe Packet(int width, int height, long timestamp) {
                data = ArrayPool<byte>.Shared.Rent(width * height * 4);
                handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                buffer = new PixelBuffer(
                    width: width,
                    height: height,
                    format: PixelBuffer.Format.RGBA8888,
                    data: (byte*)handle.AddrOfPinnedObject(),
                    timestamp: timestamp
                );
            }

            /// <summary>
            /// Dispose the packet.
            /// </summary>
            public void Dispose() {
                buffer.Dispose();
                handle.Free();
                ArrayPool<byte>.Shared.Return(data);
            }
        }
        #endregion
    }
}