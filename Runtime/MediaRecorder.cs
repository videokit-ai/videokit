/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

#nullable enable

namespace VideoKit {

    using AOT;
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using UnityEngine;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Internal;

    /// <summary>
    /// Media recorder capable of recording video and/or audio frames to a media file.
    /// All recorder methods are thread safe, and as such can be called from any thread.
    /// </summary>
    public sealed class MediaRecorder {

        #region --Client API--
        /// <summary>
        /// Recorder format.
        /// </summary>
        public readonly MediaFormat format;

        /// <summary>
        /// Recording video frame size.
        /// </summary>
        public (int width, int height) frameSize {
            get {
                recorder.FrameSize(out var width, out var height).CheckStatus();
                return (width, height);
            }
        }

        /// <summary>
        /// Commit a video pixel buffer for encoding.
        /// The pixel buffer MUST have an RGBA8888 pixel layout.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer to commit.</param>
        /// <param name="timestamp">Pixel buffer timestamp in nanoseconds.</param>
        public unsafe void CommitFrame<T> (T[] pixelBuffer, long timestamp) where T : unmanaged {
            fixed (T* baseAddress = pixelBuffer)
                CommitFrame(baseAddress, timestamp);
        }

        /// <summary>
        /// Commit a video pixel buffer for encoding.
        /// The pixel buffer MUST have an RGBA8888 pixel layout.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer to commit.</param>
        /// <param name="timestamp">Pixel buffer timestamp in nanoseconds.</param>
        public unsafe void CommitFrame<T> (NativeArray<T> pixelBuffer, long timestamp) where T : unmanaged => CommitFrame(
            pixelBuffer.GetUnsafeReadOnlyPtr(),
            timestamp
        );

        /// <summary>
        /// Commit a video pixel buffer for encoding.
        /// The pixel buffer MUST have an RGBA8888 pixel layout.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer to commit.</param>
        /// <param name="timestamp">Pixel buffer timestamp in nanoseconds.</param>
        public unsafe void CommitFrame (void* pixelBuffer, long timestamp) => recorder.CommitFrame(
            pixelBuffer,
            timestamp
        ).CheckStatus();

        /// <summary>
        /// Commit an audio sample buffer for encoding.
        /// The sample buffer MUST be a linear PCM buffer interleaved by channel.
        /// </summary>
        /// <param name="sampleBuffer">Sample buffer to commit.</param>
        /// <param name="timestamp">Sample buffer timestamp in nanoseconds.</param>
        public unsafe void CommitSamples (float[] sampleBuffer, long timestamp) {
            fixed (float* baseAddress = sampleBuffer)
                CommitSamples(baseAddress, sampleBuffer.Length, timestamp);
        }

        /// <summary>
        /// Commit an audio sample buffer for encoding.
        /// The sample buffer MUST be a linear PCM buffer interleaved by channel.
        /// </summary>
        /// <param name="sampleBuffer">Sample buffer to commit.</param>
        /// <param name="timestamp">Sample buffer timestamp in nanoseconds.</param>
        public unsafe void CommitSamples (NativeArray<float> sampleBuffer, long timestamp) => CommitSamples(
            (float*)sampleBuffer.GetUnsafeReadOnlyPtr(),
            sampleBuffer.Length,
            timestamp
        );

        /// <summary>
        /// Commit an audio sample buffer for encoding.
        /// The sample buffer MUST be a linear PCM buffer interleaved by channel.
        /// </summary>
        /// <param name="sampleBuffer">Sample buffer to commit.</param>
        /// <param name="sampleCount">Total number of samples in the buffer.</param>
        /// <param name="timestamp">Sample buffer timestamp in nanoseconds.</param>
        public unsafe void CommitSamples (float* sampleBuffer, int sampleCount, long timestamp) => recorder.CommitSamples(
            sampleBuffer,
            sampleCount,
            timestamp
        ).CheckStatus();

        /// <summary>
        /// Finish writing.
        /// </summary>
        /// <returns>Path to recorded media file.</returns>
        public Task<string> FinishWriting () {
            var tcs = new TaskCompletionSource<string>();
            var handle = GCHandle.Alloc(tcs, GCHandleType.Normal);
            try {
                recorder.FinishWriting(OnFinishWriting, (IntPtr)handle).CheckStatus();
            } catch (Exception ex) {
                handle.Free();
                tcs.SetException(ex);
            }
            return tcs.Task;
        }

        /// <summary>
        /// Create a media recorder.
        /// NOTE: This requires an active VideoKit plan.
        /// </summary>
        /// <param name="format">Recorder format.</param>
        /// <param name="width">Video width.</param>
        /// <param name="height">Video height.</param>
        /// <param name="frameRate">Video frame rate.</param>
        /// <param name="sampleRate">Audio sample rate.</param>
        /// <param name="channelCount">Audio channel count.</param>
        /// <param name="videoBitRate">Video bit rate in bits per second.</param>
        /// <param name="keyframeInterval">Keyframe interval in seconds.</param>
        /// <param name="compressionQuality">Image compression quality in range [0, 1].</param>
        /// <param name="audioBitRate">Audio bit rate in bits per second.</param>
        /// <param name="prefix">Subdirectory name to save recordings. This will be created if it does not exist.</param>
        /// <returns>Created recorder.</returns>
        public static async Task<MediaRecorder> Create (
            MediaFormat format,
            int width = 0,
            int height = 0,
            float frameRate = 0f,
            int sampleRate = 0,
            int channelCount = 0,
            int videoBitRate = 10_000_000,
            int keyframeInterval = 2,
            float compressionQuality = 0.8f,
            int audioBitRate = 64_000,
            string prefix = null
        ) {
            // Check session
            await VideoKitSettings.Instance.CheckSession();
            // Check video format
            if (new [] { MediaFormat.MP4, MediaFormat.HEVC, MediaFormat.GIF, MediaFormat.WEBM, MediaFormat.JPEG }.Contains(format)) {
                if (width <= 0)
                    throw new ArgumentException(@"Recorder width must be positive", nameof(width));
                if (height <= 0)
                    throw new ArgumentException(@"Recorder height must be positive", nameof(height));
            }
            // Check audio format
            if (new [] { MediaFormat.MP4, MediaFormat.HEVC, MediaFormat.WAV, MediaFormat.WEBM }.Contains(format)) {
                if (!ValidSampleRates.Contains(sampleRate))
                    throw new ArgumentException(@"Recorder sample rate must one of " + string.Join(", ", ValidSampleRates), nameof(width));
                if (channelCount < 0)
                    throw new ArgumentException(@"Recorder channel count must be non negative", nameof(height));
            }
            // Check divisible by two
            if (new [] { MediaFormat.MP4, MediaFormat.HEVC, MediaFormat.WEBM }.Contains(format)) {
                if (width % 2 != 0)
                    throw new ArgumentException(@"Recorder width must be divisible by 2", nameof(width));
                if (height % 2 != 0)
                    throw new ArgumentException(@"Recorder height must be divisible by 2", nameof(height));
            }
            // Create recorder
            switch (format) {
                case MediaFormat.MP4:   return CreateMP4(width, height, frameRate, sampleRate, channelCount, videoBitRate, keyframeInterval, audioBitRate, prefix);
                case MediaFormat.HEVC:  return CreateHEVC(width, height, frameRate, sampleRate, channelCount, videoBitRate, keyframeInterval, audioBitRate, prefix);
                case MediaFormat.GIF:   return CreateGIF(width, height, frameRate, prefix);
                case MediaFormat.WAV:   return CreateWAV(sampleRate, channelCount, prefix);
                case MediaFormat.JPEG:  return CreateJPEG(width, height, compressionQuality, prefix);
                case MediaFormat.WEBM:  return CreateWEBM(width, height, frameRate, sampleRate, channelCount, videoBitRate, keyframeInterval, audioBitRate, prefix);
                default:                throw new InvalidOperationException($"Cannot create media recorder because format is not supported: {format}");
            }
        }
        #endregion

        
        #region --Operations--
        private readonly IntPtr recorder;
        private static string directory = string.Empty;
        private static readonly int[] ValidSampleRates = new [] { 0, 8_000, 16_000, 22_050, 24_000, 44_100, 48_000 };

        private MediaRecorder (IntPtr recorder, MediaFormat format) {
            // Check
            if (recorder == IntPtr.Zero)
                throw new InvalidOperationException(@"Failed to create media recorder. Check the logs for more info.");
            // Set
            this.recorder = recorder;
            this.format = format;
        }

        public static implicit operator IntPtr (MediaRecorder recorder) => recorder.recorder;

        private static MediaRecorder CreateMP4 (int width, int height, float frameRate, int sampleRate, int channelCount, int videoBitRate, int keyframeInterval, int audioBitRate, string prefix) {            
            // Create
            VideoKit.CreateMP4Recorder(
                CreatePath(extension:@".mp4", prefix:prefix),
                width,
                height,
                frameRate,
                sampleRate,
                channelCount,
                videoBitRate,
                keyframeInterval,
                audioBitRate,
                out var recorder
            ).CheckStatus();
            // Return
            return new MediaRecorder(recorder, MediaFormat.MP4);
        }

        private static MediaRecorder CreateHEVC (int width, int height, float frameRate, int sampleRate, int channelCount, int videoBitRate, int keyframeInterval, int audioBitRate, string prefix) {            
            // Create
            VideoKit.CreateHEVCRecorder(
                CreatePath(extension: @".mp4", prefix: prefix),
                width,
                height,
                frameRate,
                sampleRate,
                channelCount,
                videoBitRate,
                keyframeInterval,
                audioBitRate,
                out var recorder
            ).CheckStatus();
            // Return
            return new MediaRecorder(recorder, MediaFormat.HEVC);
        }

        private static MediaRecorder CreateGIF (int width, int height, float frameRate, string prefix) {
            // Create
            VideoKit.CreateGIFRecorder(
                CreatePath(extension: @".gif", prefix: prefix),
                width,
                height,
                1f / frameRate,
                out var recorder
            ).CheckStatus();
            // Return
            return new MediaRecorder(recorder, MediaFormat.GIF);
        }

        private static MediaRecorder CreateWAV (int sampleRate, int channelCount, string prefix) {
            // Create
            VideoKit.CreateWAVRecorder(
                CreatePath(extension: @".wav", prefix: prefix),
                sampleRate,
                channelCount,
                out var recorder
            ).CheckStatus();
            // Return
            return new MediaRecorder(recorder, MediaFormat.WAV);
        }

        private static MediaRecorder CreateWEBM (int width, int height, float frameRate, int sampleRate, int channelCount, int videoBitRate, int keyframeInterval, int audioBitRate, string prefix) {
            // Create
            VideoKit.CreateWEBMRecorder(
                CreatePath(extension: @".webm", prefix: prefix),
                width,
                height,
                frameRate,
                sampleRate,
                channelCount,
                videoBitRate,
                keyframeInterval,
                audioBitRate,
                out var recorder
            ).CheckStatus();
            // Return
            return new MediaRecorder(recorder, MediaFormat.WEBM);          
        }

        private static MediaRecorder CreateJPEG (int width, int height, float quality, string prefix) {
            // Create
            VideoKit.CreateJPEGRecorder(
                CreatePath(prefix: prefix),
                width,
                height,
                quality,
                out var recorder
            ).CheckStatus();
            // Return
            return new MediaRecorder(recorder, MediaFormat.JPEG);    
        }

        internal static string CreatePath (string? extension = null, string? prefix = null) {
            // Create parent directory
            var parentDirectory = !string.IsNullOrEmpty(prefix) ? Path.Combine(directory, prefix) : directory;
            Directory.CreateDirectory(parentDirectory);
            // Get recording path
            var timestamp = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
            var name = $"recording_{timestamp}{extension ?? string.Empty}";
            var path = Path.Combine(parentDirectory, name);
            // Return
            return path;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void OnInitialize () => directory = Application.isEditor ?
            Directory.GetCurrentDirectory() :
            Application.persistentDataPath;
        
        [MonoPInvokeCallback(typeof(VideoKit.RecordingHandler))]
        private static unsafe void OnFinishWriting (IntPtr context, IntPtr path) {
            // Check
            if (!VideoKit.IsAppDomainLoaded)
                return;
            // Get tcs
            TaskCompletionSource<string> tcs;
            try {
                var handle = (GCHandle)context;
                tcs = handle.Target as TaskCompletionSource<string>;
                handle.Free();
            } catch (Exception ex) {
                Debug.LogException(ex);
                return;
            }
            // Invoke
            if (path != IntPtr.Zero)
                tcs?.SetResult(Marshal.PtrToStringUTF8(path));
            else
                tcs?.SetException(new Exception(@"Recorder failed to finish writing"));
        }
        #endregion
    }
}