/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Recorders {

    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;
    using Unity.Collections;
    using Internal;

    /// <summary>
    /// Media recorder capable of recording video and/or audio frames to a media file.
    /// All recorder methods are thread safe, and as such can be called from any thread.
    /// </summary>
    public abstract class MediaRecorder {

        #region --Client API--
        /// <summary>
        /// Recorder format.
        /// </summary>
        public readonly MediaFormat format;

        /// <summary>
        /// Recording video frame size.
        /// </summary>
        public abstract (int width, int height) frameSize { get; }

        /// <summary>
        /// Commit a video pixel buffer for encoding.
        /// The pixel buffer MUST have an RGBA8888 pixel layout.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer to commit.</param>
        /// <param name="timestamp">Pixel buffer timestamp in nanoseconds.</param>
        public abstract void CommitFrame<T> (T[] pixelBuffer, long timestamp) where T : unmanaged;

        /// <summary>
        /// Commit a video pixel buffer for encoding.
        /// The pixel buffer MUST have an RGBA8888 pixel layout.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer to commit.</param>
        /// <param name="timestamp">Pixel buffer timestamp in nanoseconds.</param>
        public abstract void CommitFrame<T> (NativeArray<T> pixelBuffer, long timestamp) where T : unmanaged;

        /// <summary>
        /// Commit a video pixel buffer for encoding.
        /// The pixel buffer MUST have an RGBA8888 pixel layout.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer to commit.</param>
        /// <param name="timestamp">Pixel buffer timestamp in nanoseconds.</param>
        public abstract unsafe void CommitFrame (void* pixelBuffer, long timestamp);

        /// <summary>
        /// Commit an audio sample buffer for encoding.
        /// The sample buffer MUST be a linear PCM buffer interleaved by channel.
        /// </summary>
        /// <param name="sampleBuffer">Sample buffer to commit.</param>
        /// <param name="timestamp">Sample buffer timestamp in nanoseconds.</param>
        public abstract void CommitSamples (float[] sampleBuffer, long timestamp);

        /// <summary>
        /// Commit an audio sample buffer for encoding.
        /// The sample buffer MUST be a linear PCM buffer interleaved by channel.
        /// </summary>
        /// <param name="sampleBuffer">Sample buffer to commit.</param>
        /// <param name="timestamp">Sample buffer timestamp in nanoseconds.</param>
        public abstract void CommitSamples (NativeArray<float> sampleBuffer, long timestamp);

        /// <summary>
        /// Commit an audio sample buffer for encoding.
        /// The sample buffer MUST be a linear PCM buffer interleaved by channel.
        /// </summary>
        /// <param name="sampleBuffer">Sample buffer to commit.</param>
        /// <param name="sampleCount">Total number of samples in the buffer.</param>
        /// <param name="timestamp">Sample buffer timestamp in nanoseconds.</param>
        public abstract unsafe void CommitSamples (float* sampleBuffer, int sampleCount, long timestamp);

        /// <summary>
        /// Finish writing.
        /// </summary>
        /// <returns>Path to recorded media file.</returns>
        public abstract Task<string> FinishWriting ();

        /// <summary>
        /// Create a media recorder.
        /// NOTE: This requires an active VideoKit Core plan.
        /// </summary>
        /// <param name="format">Recorder format.</param>
        /// <param name="width">Video width.</param>
        /// <param name="height">Video height.</param>
        /// <param name="frameRate">Video frame rate.</param>
        /// <param name="sampleRate">Audio sample rate.</param>
        /// <param name="channelCount">Audio channel count.</param>
        /// <param name="videoBitRate">Video bit rate in bits per second.</param>
        /// <param name="keyframeInterval">Keyframe interval in seconds.</param>
        /// <param name="videoQuality">Video encoding quality in range [1, 100]. This only applies to JPEG recorders.</param>
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
            int videoQuality = 80,
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
                case MediaFormat.JPEG:  return new JPEGRecorder(width, height, videoQuality, prefix);
                case MediaFormat.WEBM:  return CreateWEBM(width, height, frameRate, sampleRate, channelCount, videoBitRate, keyframeInterval, audioBitRate, prefix);
                default:                throw new InvalidOperationException($"Cannot create media recorder because format is not supported: {format}");
            }
        }
        #endregion

        
        #region --Operations--
        private static string directory = string.Empty;
        private static readonly int[] ValidSampleRates = new [] { 0, 8_000, 16_000, 22_050, 24_000, 44_100, 48_000 };

        protected MediaRecorder (MediaFormat format) => this.format = format;

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
            return new NativeRecorder(MediaFormat.MP4, recorder);
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
            return new NativeRecorder(MediaFormat.HEVC, recorder);
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
            return new NativeRecorder(MediaFormat.GIF, recorder);
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
            return new NativeRecorder(MediaFormat.WAV, recorder);
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
            return new NativeRecorder(MediaFormat.WEBM, recorder);          
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
        #endregion
    }
}