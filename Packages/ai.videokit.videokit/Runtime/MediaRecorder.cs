/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit {

    using AOT;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using UnityEngine;
    using Internal;
    using Status = Internal.VideoKit.Status;

    /// <summary>
    /// Media recorder capable of recording video and/or audio frames to a media file.
    /// All recorder methods are thread safe, and as such can be called from any thread.
    /// </summary>
    public class MediaRecorder {

        #region --Enumerations--
        /// <summary>
        /// Recording format.
        /// </summary>
        public enum Format : int {
            /// <summary>
            /// MP4 video with H.264 AVC video codec and AAC audio codec.
            /// This format supports recording both video and audio frames.
            /// This format is not supported on WebGL.
            /// </summary>
            MP4 = 0,
            /// <summary>
            /// MP4 video with H.265 HEVC video codec and AAC audio codec.
            /// This format has better compression than `MP4`.
            /// This format supports recording both video and audio frames.
            /// This format is not supported on WebGL.
            /// </summary>
            HEVC = 1,
            /// <summary>
            /// WEBM video with VP8 or VP9 video codec.
            /// This format support recording both video and audio frames.
            /// This is only supported on Android and WebGL.
            /// </summary>
            WEBM = 2,
            /// <summary>
            /// Animated GIF image.
            /// This format only supports recording video frames.
            /// </summary>
            GIF = 3,
            /// <summary>
            /// JPEG image sequence.
            /// This format only supports recording video frames.
            /// This format is not supported on WebGL.
            /// </summary>
            JPEG = 4,
            /// <summary>
            /// Waveform audio.
            /// This format only supports recording audio frames.
            /// </summary>
            WAV = 5,
            /// <summary>
            /// EXPERIMENTAL.
            /// MP4 video with AV1 video codec and AAC audio codec.
            /// This format supports recording both video and audio frames.
            /// This is currently supported on Android 14+.
            /// </summary>
            AV1 = 6,
            /// <summary>
            /// EXPERIMENTAL.
            /// Apple ProRes video.
            /// This format supports recording both video and audio frames.
            /// This is currently supported on iOS and macOS.
            /// </summary>
            ProRes4444 = 7,
        }
        #endregion


        #region --Client API--
        /// <summary>
        /// Recorder format.
        /// </summary>
        public virtual Format format => recorder.GetMediaRecorderFormat(out var format).Throw() == Status.Ok ? format : default;

        /// <summary>
        /// Recorder video width.
        /// </summary>
        public virtual int width => recorder.GetMediaRecorderWidth(out var width).Throw() == Status.Ok ? width : default;

        /// <summary>
        /// Recorder video height.
        /// </summary>
        public virtual int height => recorder.GetMediaRecorderHeight(out var height).Throw() == Status.Ok ? height : default;

        /// <summary>
        /// Recorder audio sample rate.
        /// </summary>
        public virtual int sampleRate => recorder.GetMediaRecorderSampleRate(out var sampleRate).Throw() == Status.Ok ? sampleRate : default;

        /// <summary>
        /// Recorder audio channel count.
        /// </summary>
        public virtual int channelCount => recorder.GetMediaRecorderChannelCount(out var channelCount).Throw() == Status.Ok ? channelCount : default;

        /// <summary>
        /// Check whether the media recorder supports appending sample buffers of the given type.
        /// </summary>
        /// <typeparam name="T">Sample buffer type.</typeparam>
        /// <returns>Whether the media recorder supports appending sample buffers of the given type.</returns>
        public virtual bool CanAppend<T> () where T : struct => CanAppend<T>(format);

        /// <summary>
        /// Append a video frame to the recorder.
        /// </summary>
        /// <param name="image">Input image to append. The image MUST have a valid timestamp for formats that require one.</param>
        public virtual void Append (PixelBuffer pixelBuffer) => recorder.AppendPixelBuffer(pixelBuffer).Throw();

        /// <summary>
        /// Append an audio frame to the recorder.
        /// </summary>
        /// <param name="audioBuffer">Input audio buffer to append. This audio buffer MUST have a valid timestamp for formats that require one.</param>
        public virtual void Append (AudioBuffer audioBuffer) => recorder.AppendSampleBuffer(audioBuffer).Throw();

        /// <summary>
        /// Finish writing.
        /// </summary>
        /// <returns>Recorded media asset.</returns>
        public virtual Task<MediaAsset> FinishWriting () {
            var tcs = new TaskCompletionSource<MediaAsset>();
            var handle = GCHandle.Alloc(tcs, GCHandleType.Normal);
            try {
                recorder.FinishWriting(OnFinishWriting, (IntPtr)handle).Throw();
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
            Format format,
            int width = 0,
            int height = 0,
            float frameRate = 0f,
            int sampleRate = 0,
            int channelCount = 0,
            int videoBitRate = 20_000_000,
            int keyframeInterval = 2,
            float compressionQuality = 0.8f,
            int audioBitRate = 64_000,
            string? prefix = null
        ) {
            // Check session
            await VideoKitClient.Instance!.CheckSession();
            // Create recorder
            IntPtr recorder = IntPtr.Zero;
            switch (format) {
                case Format.MP4: return new MediaRecorder(VideoKit.CreateMP4Recorder(
                        CreatePath(extension:@".mp4", prefix:prefix),
                        width,
                        height,
                        frameRate,
                        sampleRate,
                        channelCount,
                        videoBitRate,
                        keyframeInterval,
                        audioBitRate,
                        out recorder
                    ).Throw() == Status.Ok ? recorder : default);
                case Format.HEVC: return new MediaRecorder(VideoKit.CreateHEVCRecorder(
                        CreatePath(extension: @".mp4", prefix: prefix),
                        width,
                        height,
                        frameRate,
                        sampleRate,
                        channelCount,
                        videoBitRate,
                        keyframeInterval,
                        audioBitRate,
                        out recorder
                    ).Throw() == Status.Ok ? recorder : default);
                case Format.GIF: return new MediaRecorder(VideoKit.CreateGIFRecorder(
                        CreatePath(extension: @".gif", prefix: prefix),
                        width,
                        height,
                        1f / frameRate,
                        out recorder
                    ).Throw() == Status.Ok ? recorder : default);
                case Format.WAV: return new MediaRecorder(VideoKit.CreateWAVRecorder(
                        CreatePath(extension: @".wav", prefix: prefix),
                        sampleRate,
                        channelCount,
                        out recorder
                    ).Throw() == Status.Ok ? recorder : default);
                case Format.WEBM: return new MediaRecorder(VideoKit.CreateWEBMRecorder(
                        CreatePath(extension: @".webm", prefix: prefix),
                        width,
                        height,
                        frameRate,
                        sampleRate,
                        channelCount,
                        videoBitRate,
                        keyframeInterval,
                        audioBitRate,
                        out recorder
                    ).Throw() == Status.Ok ? recorder : default);
                case Format.JPEG: return new MediaRecorder(VideoKit.CreateJPEGRecorder(
                        CreatePath(prefix: prefix),
                        width,
                        height,
                        compressionQuality,
                        out recorder
                    ).Throw() == Status.Ok ? recorder : default);
                case Format.AV1: return new MediaRecorder(VideoKit.CreateAV1Recorder(
                        CreatePath(extension: @".mp4", prefix: prefix),
                        width,
                        height,
                        frameRate,
                        sampleRate,
                        channelCount,
                        videoBitRate,
                        keyframeInterval,
                        audioBitRate,
                        out recorder
                    ).Throw() == Status.Ok ? recorder : default);
                case Format.ProRes4444: return new MediaRecorder(VideoKit.CreateProRes4444Recorder(
                        CreatePath(extension: @".mov", prefix: prefix),
                        width,
                        height,
                        sampleRate,
                        channelCount,
                        audioBitRate,
                        out recorder
                    ).Throw() == Status.Ok ? recorder : default);
                default: throw new InvalidOperationException($"Cannot create media recorder because format is not supported: {format}");
            }
        }

        /// <summary>
        /// Check whether the media recorder supports appending sample buffers of the given type.
        /// </summary>
        /// <typeparam name="T">Sample buffer type.</typeparam>
        /// <param name="format">Media recorder format.</param>
        /// <returns>Whether the media recorder supports appending sample buffers of the given type.</returns>
        public static bool CanAppend<T> (Format format) where T : struct => FormatSampleBufferSupportMatrix.TryGetValue(typeof(T), out var formats) && formats.Contains(format);

        /// <summary>
        /// Check whether the current device supports recording to this format.
        /// </summary>
        /// <param name="format">Recording format.</param>
        /// <returns>Whether the current device supports recording to this format.</returns>
        public static bool CanCreate (Format format) => VideoKit.IsMediaRecorderFormatSupported(format) == Status.Ok;
        #endregion


        #region --Operations--
        private readonly IntPtr recorder;
        private static string directory = string.Empty;
        private static readonly Dictionary<Type, Format[]> FormatSampleBufferSupportMatrix = new () {
            [typeof(PixelBuffer)] = new [] {
                Format.MP4, Format.HEVC, Format.WEBM,
                Format.GIF, Format.JPEG, Format.AV1,
                Format.ProRes4444
            },
            [typeof(AudioBuffer)] = new [] {
                Format.MP4, Format.HEVC, Format.WEBM,
                Format.WAV, Format.AV1, Format.ProRes4444
            },
        };

        protected MediaRecorder (IntPtr recorder = default) => this.recorder = recorder;

        public static implicit operator IntPtr (MediaRecorder recorder) => recorder.recorder;

        public static implicit operator Action<PixelBuffer> (MediaRecorder recorder) => recorder.Append;

        public static implicit operator Action<AudioBuffer> (MediaRecorder recorder) => recorder.Append;

        private static string CreatePath (string? extension = null, string? prefix = null) {
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
        #endregion


        #region --Callbacks--

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void OnInitialize () => directory = Application.isEditor ?
            Directory.GetCurrentDirectory() :
            Application.persistentDataPath;
        
        [MonoPInvokeCallback(typeof(VideoKit.MediaAssetHandler))]
        private static unsafe void OnFinishWriting (IntPtr context, IntPtr asset) {
            // Check
            if (!VideoKit.IsAppDomainLoaded)
                return;
            // Get tcs
            TaskCompletionSource<MediaAsset>? tcs;
            try {
                var handle = (GCHandle)context;
                tcs = handle.Target as TaskCompletionSource<MediaAsset>;
                handle.Free();
            } catch (Exception ex) {
                Debug.LogException(ex);
                return;
            }
            // Invoke
            if (asset != IntPtr.Zero)
                tcs?.SetResult(new MediaAsset(asset));
            else
                tcs?.SetException(new Exception(@"Recorder failed to finish writing"));
        }
        #endregion
    }
}