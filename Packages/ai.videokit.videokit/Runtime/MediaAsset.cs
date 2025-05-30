/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit {

    using AOT;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;
    using Function.Types;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using NJsonSchema;
    using NJsonSchema.Generation;
    using Internal;
    using Status = Internal.VideoKit.Status;

    /// <summary>
    /// Media asset.
    /// </summary>
    public sealed class MediaAsset {

        #region --Enumerations--
        /// <summary>
        /// Media type.
        /// </summary>
        public enum MediaType : int { // CHECK // `VideoKit.h`
            /// <summary>
            /// Unknown or unsupported media type.
            /// </summary>
            [EnumMember(Value = @"unknown")]
            Unknown = 0,
            /// <summary>
            /// Image.
            /// </summary>
            [EnumMember(Value = @"image")]
            Image = 1,
            /// <summary>
            /// Audio.
            /// </summary>
            [EnumMember(Value = @"audio")]
            Audio = 2,
            /// <summary>
            /// Video.
            /// </summary>
            [EnumMember(Value = @"video")]
            Video = 3,
            /// <summary>
            /// Text.
            /// </summary>
            [EnumMember(Value = @"text")]
            Text = 4,
            /// <summary>
            /// Sequence.
            /// </summary>
            [EnumMember(Value = @"sequence")]
            Sequence = 5,
        }

        /// <summary>
        /// Audio narration voice.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum NarrationVoice : int {
            /// <summary>
            /// Default narration voice.
            /// </summary>
            [EnumMember(Value = @"default")]
            Default = 0,
            /// <summary>
            /// Male 1 narration voice.
            /// </summary>
            [EnumMember(Value = @"kevin")]
            Kevin = 1,
            /// <summary>
            /// Male 2 narration voice.
            /// </summary>
            [EnumMember(Value = @"arjun")]
            Arjun = 2,
            /// <summary>
            /// Male 3 narration voice.
            /// </summary>
            [EnumMember(Value = @"dami")]
            Dami = 3,
            /// <summary>
            /// Male 4 narration voice.
            /// </summary>
            [EnumMember(Value = @"juan")]
            Juan = 4,
            /// <summary>
            /// Female 1 narration voice.
            /// </summary>
            [EnumMember(Value = @"rhea")]
            Rhea = 5,
            /// <summary>
            /// Female 2 narration voice.
            /// </summary>
            [EnumMember(Value = @"aliyah")]
            Aliyah = 6,
            /// <summary>
            /// Female 3 narration voice.
            /// </summary>
            [EnumMember(Value = @"kristen")]
            Kristen = 7,
            /// <summary>
            /// Female 4 narration voice.
            /// </summary>
            [EnumMember(Value = @"salma")]
            Salma = 8,
        }
        #endregion


        #region --Properties--
        /// <summary>
        /// Path to media asset.
        /// This is `null` for sequence assets.
        /// </summary>
        public string? path {
            get {
                var sb = new StringBuilder(2048);
                var status = asset.GetMediaAssetPath(sb, sb.Capacity);
                return status == Status.Ok ? sb.ToString() : null;
            }
        }

        /// <summary>
        /// Asset media type.
        /// </summary>
        public MediaType type => asset.GetMediaAssetMediaType(out var type).Throw() == Status.Ok ? type : default;

        /// <summary>
        /// Image or video width.
        /// </summary>
        public int width => asset.GetMediaAssetWidth(out var width) == Status.Ok ? width : default;

        /// <summary>
        /// Image or video height.
        /// </summary>
        public int height => asset.GetMediaAssetHeight(out var height) == Status.Ok ? height : default;

        /// <summary>
        /// Video frame rate.
        /// </summary>
        public float frameRate => asset.GetMediaAssetFrameRate(out var frameRate) == Status.Ok ? frameRate : default;

        /// <summary>
        /// Audio sample rate.
        /// </summary>
        public int sampleRate => asset.GetMediaAssetSampleRate(out var sampleRate) == Status.Ok ? sampleRate : default;

        /// <summary>
        /// Audio channel count.
        /// </summary>
        public int channelCount => asset.GetMediaAssetChannelCount(out var channelCount) == Status.Ok ? channelCount : default;

        /// <summary>
        /// Video or audio duration in seconds.
        /// </summary>
        public float duration => asset.GetMediaAssetDuration(out var duration) == Status.Ok ? duration : default;

        /// <summary>
        /// Media assets contained within this asset.
        /// This is only populated for `Sequence` assets.
        /// </summary>
        public IReadOnlyList<MediaAsset> assets => new NativeMediaSequence(this);
        #endregion


        #region --Creators--
        /// <summary>
        /// Create a media aseet from a file.
        /// </summary>
        /// <param name="path">Path to media file.</param>
        /// <returns>Media asset.</returns>
        public static Task<MediaAsset> FromFile (string path) {
            var tcs = new TaskCompletionSource<MediaAsset>();
            var handle = GCHandle.Alloc(tcs, GCHandleType.Normal);
            try {
                VideoKit.CreateMediaAsset(path, OnCreateAsset, (IntPtr)handle).Throw();
            } catch (Exception ex) {
                handle.Free();
                tcs.SetException(ex);
            }
            return tcs.Task;
        }

        /// <summary>
        /// Create a media aseet from a texture.
        /// </summary>
        /// <param name="texture">Texture.</param>
        /// <returns>Image asset.</returns>
        public static Task<MediaAsset> FromTexture (Texture2D texture) {
            // Check
            if (texture == null)
                return Task.FromException<MediaAsset>(new ArgumentNullException(nameof(texture)));
            // Check
            if (!texture.isReadable)
                return Task.FromException<MediaAsset>(new ArgumentException(@"Cannot create media asset from texture that is not readable"));
            // Write to file
            var encoded = texture.EncodeToPNG();
            var name = Guid.NewGuid().ToString("N");
            var path = Path.Combine(Application.temporaryCachePath, $"{name}.png");
            File.WriteAllBytes(path, encoded);
            // Create asset
            return FromFile(path);
        }

        /// <summary>
        /// Create a media aseet from an audio clip.    
        /// NOTE: This requires an active VideoKit plan.
        /// </summary>
        /// <param name="clip">Audio clip.</param>
        /// <param name="format">Media format used to encode the audio.</param>
        /// <returns>Audio asset.</returns>
        public static async Task<MediaAsset> FromAudioClip (
            AudioClip clip,
            MediaRecorder.Format format = MediaRecorder.Format.WAV
        ) {
            // Create audio buffer
            var sampleBuffer = new float[clip.samples * clip.channels];
            clip.GetData(sampleBuffer, 0);
            using var audioBuffer = new AudioBuffer(clip.frequency, clip.channels, sampleBuffer);
            // Create asset
            var recorder = await MediaRecorder.Create(
                format,
                sampleRate: audioBuffer.sampleRate,
                channelCount: audioBuffer.channelCount
            );
            recorder.Append(audioBuffer);
            var asset = await recorder.FinishWriting();
            // Return
            return asset;
        }

        /// <summary>
        /// Create a media aseet from plain text.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <returns>Text asset.</returns>
        public static Task<MediaAsset> FromText (string text) {
            // Write to file
            var name = Guid.NewGuid().ToString("N");
            var path = Path.Combine(Application.temporaryCachePath, $"{name}.txt");
            File.WriteAllText(path, text);
            // Create asset
            return FromFile(path);
        }

        /// <summary>
        /// Create a media asset by prompting the user to select an image or video from the camera roll.
        /// NOTE: This requires iOS 14+.
        /// </summary>
        /// <param name="type">Desired asset type.</param>
        /// <returns>Media asset.</returns>
        public static Task<MediaAsset?> FromCameraRoll (MediaType type) {
            var tcs = new TaskCompletionSource<MediaAsset?>();
            var handle = GCHandle.Alloc(tcs, GCHandleType.Normal);
            try {
                VideoKit.CreateMediaAssetFromCameraRoll(type, OnCreateAsset, (IntPtr)handle).Throw();
            } catch (Exception ex) {
                handle.Free();
                tcs.SetException(ex);
            }
            return tcs.Task;
        }

        /// <summary>
        /// Create a media asset from a file in `StreamingAssets`.
        /// </summary>
        /// <param name="path">Relative file path in `StreamingAssets`.</param>
        /// <returns>Media asset.</returns>
        public static async Task<MediaAsset?> FromStreamingAssets (string path) {
            // Get absolute path
            var absolutePath = await StreamingAssetsToAbsolutePath(path);
            if (absolutePath == null)
                throw new InvalidOperationException($"Failed to create media asset because file '{path}' could not be found in `StreamingAssets`");
            // Create asset
            return await FromFile(absolutePath);
        }

        /// <summary>
        /// Create a media asset by performing text-to-speech on the provided text prompt.
        /// </summary>
        /// <param name="prompt">Text to synthesize speech from.</param>
        /// <param name="voice">Voice to use for generation. See https://videokit.ai/reference/mediaasset for more information.</param>
        /// <returns>Generated audio asset.</returns>
        internal static async Task<MediaAsset> FromGeneratedSpeech ( // INCOMPLETE
            string prompt,
            NarrationVoice voice = 0
        ) {
            return default;
        }

        /// <summary>
        /// Create a media asset by performing text-to-image on the provided text prompt.
        /// </summary>
        /// <param name="prompt">Text prompt to use to generate the image.</param>
        /// <param name="desiredWidth">Desired image width. NOTE: The generated image is not guaranteed to have this width.</param>
        /// <param name="desiredHeight">Desired image height. NOTE: The generated image is not guaranteed to have this height.</param>
        /// <returns>Generated image asset.</returns>
        internal static async Task<MediaAsset> FromGeneratedImage ( // INCOMPLETE
            string prompt,
            int desiredWidth = 1024,
            int desiredHeight = 1024
        ) {
            return default;
        }
        #endregion


        #region --Converters--
        /// <summary>
        /// Create a texture from the media asset.
        /// This can only be used on image and video assets.
        /// </summary>
        /// <param name="time">Time to extract the texture from. This is only supported for video assets.</param>
        /// <returns>Texture.</returns>
        public async Task<Texture2D> ToTexture (float time = 0f) {
            // Check video
            if (type == MediaType.Video)
                throw new NotImplementedException(@"`MediaAsset.ToTexture` is not yet supported for video assets");
            // Check type
            if (type != MediaType.Image)
                throw new ArgumentException(@"`MediaAsset.ToTexture` can only be used on image assets");
            // Load data
            var uri = path![0] == '/' ? $"file://{path}" : path;
            using var request = UnityWebRequestTexture.GetTexture(uri);
            request.SendWebRequest();
            while (!request.isDone)
                await Task.Yield();
            // Check
            if (request.result != UnityWebRequest.Result.Success)
                throw new InvalidOperationException($"Image asset could not be loaded with error: {request.error}");
            // Return
            return DownloadHandlerTexture.GetContent(request);
        }

        /// <summary>
        /// Create an audio clip from the media asset.
        /// </summary>
        /// <returns>Audio clip.</returns>
        public async Task<AudioClip> ToAudioClip () { // CHECK // WAV only for now
            // Check type
            if (type != MediaType.Audio)
                throw new ArgumentException($"Cannot create audio clip from asset because asset has invalid type: {type}");
            // Load data
            var uri = path![0] == '/' ? $"file://{path}" : path;
            using var request = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.WAV);
            request.SendWebRequest();
            while (!request.isDone)
                await Task.Yield();
            // Check
            if (request.result != UnityWebRequest.Result.Success)
                throw new InvalidOperationException($"Audio clip could not be loaded with error: {request.error}");
            // Return
            return DownloadHandlerAudioClip.GetContent(request);
        }
        #endregion


        #region --Reading--
        /// <summary>
        /// Read sample buffers in the media asset.
        /// </summary>
        /// <returns>Sample buffers in the media asset.</returns>
        public IEnumerable<T> Read<T> () where T : struct {
            var type = GetMediaType<T>();
            foreach (var sampleBuffer in Read(type)) {
                if (type == MediaType.Video)
                    yield return (T)(object)new PixelBuffer(sampleBuffer); // sexy code is worth boxing overhead :p
                else if (type == MediaType.Audio)
                    yield return (T)(object)new AudioBuffer(sampleBuffer);
                else
                    break;
            }
        }
        #endregion


        #region --Sharing--
        /// <summary>
        /// Share the media asset using the native sharing UI.
        /// </summary>
        /// <param name="message">Optional message to share with the media asset.</param>
        /// <returns>Receiving app bundle ID or `null` if the user did not complete the share action.</returns>
        public Task<string?> Share (string? message = null) {
            // Check
            if (type == MediaType.Sequence)
                throw new InvalidOperationException(@"Sequence assets cannot be shared");
            // Share
            var tcs = new TaskCompletionSource<string?>();
            var handle = GCHandle.Alloc(tcs, GCHandleType.Normal);
            try {
                asset.ShareMediaAsset(
                    message,
                    OnShare,
                    (IntPtr)handle
                ).Throw();
            } catch (NotImplementedException) {
                tcs.SetResult(null);
            } catch (Exception ex) {
                tcs.SetException(ex);
            } finally {
                handle.Free();
            }
            // Return
            return tcs.Task;
        }

        /// <summary>
        /// Save the media asset to the camera roll.
        /// </summary>
        /// <param name="album">Optional album to save media asset to.</param>
        /// <returns>Whether the asset was successfully saved to the camera roll.</returns>
        public Task<bool> SaveToCameraRoll (string? album = null) {
            // Check
            if (type == MediaType.Sequence)
                throw new InvalidOperationException(@"Sequence assets cannot be saved to the camera roll");
            // Save
            var tcs = new TaskCompletionSource<bool>();
            var handle = GCHandle.Alloc(tcs, GCHandleType.Normal);
            try {
                asset.SaveMediaAssetToCameraRoll(
                    album,
                    OnSaveToCameraRoll,
                    (IntPtr)handle
                ).Throw();
            } catch (NotImplementedException) {
                tcs.SetResult(false);
            } catch (Exception ex) {
                tcs.SetException(ex);
            } finally {
                handle.Free();
            }
            // Return
            return tcs.Task;
        }
        #endregion


        #region --AI--
        /// <summary>
        /// Parse the text asset into a structure.
        /// </summary>
        /// <typeparam name="T">Structure to parse into.</typeparam>
        /// <returns>Parsed structure.</returns>
        internal async Task<T> Parse<T> () {
            // Check
            if (type != MediaType.Text)
                throw new ArgumentException($"Cannot perform structured parsing on media asset because asset is not a text asset");
            // Generate schema
            var settings = new JsonSchemaGeneratorSettings {
                GenerateAbstractSchemas = false,
                GenerateExamples = false,
                UseXmlDocumentation = false,
                ResolveExternalXmlDocumentation = false,
                FlattenInheritanceHierarchy = false,
            };
            var schema = JsonSchema.FromType<T>(settings);
            // Parse
            
            return default;
        }

        /// <summary>
        /// Transcribe the audio asset by performing speech-to-text.
        /// </summary>
        internal async Task<string> Transcribe () {
            // Check type
            if (type != MediaType.Audio)
                throw new InvalidOperationException($"Cannot caption media asset because asset is not an audio asset");
            // Transcribe
            
            return default;
        }
        #endregion


        #region --Operations--
        private readonly IntPtr asset;

        internal MediaAsset (IntPtr asset) => this.asset = asset;

        ~MediaAsset () => asset.ReleaseMediaAsset();

        private IEnumerable<IntPtr> Read (MediaType type) {
            asset.CreateMediaReader(type, out var reader).Throw();
            try {
                for (;;) {
                    var status = reader.ReadNextSampleBuffer(out var sampleBuffer);
                    if (status == Status.InvalidOperation)
                        break;
                    if (sampleBuffer == default)
                        continue;
                    yield return sampleBuffer;
                    sampleBuffer.ReleaseSampleBuffer();
                }
            } finally {
                reader.ReleaseMediaReader();
            }
        }

        public static implicit operator IntPtr (MediaAsset asset) => asset.asset;
        #endregion


        #region --Callbacks--

        [MonoPInvokeCallback(typeof(VideoKit.MediaAssetHandler))]
        private static void OnCreateAsset (IntPtr context, IntPtr asset) {
            try {
                if (!VideoKit.IsAppDomainLoaded)
                    return;
                var handle = (GCHandle)context;
                var tcs = handle.Target as TaskCompletionSource<MediaAsset?>;
                handle.Free();
                tcs?.SetResult(asset != IntPtr.Zero ? new MediaAsset(asset) : null);
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        [MonoPInvokeCallback(typeof(VideoKit.MediaAssetShareHandler))]
        private static void OnShare (IntPtr context, IntPtr receiver) {
            try {
                // Check
                if (!VideoKit.IsAppDomainLoaded)
                    return;
                // Get tcs
                var handle = (GCHandle)context;
                var tcs = handle.Target as TaskCompletionSource<string>;
                handle.Free();
                // Complete task
                var result = Marshal.PtrToStringUTF8(receiver);
                tcs?.SetResult(result);
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        [MonoPInvokeCallback(typeof(VideoKit.MediaAssetShareHandler))]
        private static void OnSaveToCameraRoll (IntPtr context, IntPtr receiver) {
            try {
                // Check
                if (!VideoKit.IsAppDomainLoaded)
                    return;
                // Get tcs
                var handle = (GCHandle)context;
                var tcs = handle.Target as TaskCompletionSource<bool>;
                handle.Free();
                // Complete task
                var result = receiver != IntPtr.Zero;
                tcs?.SetResult(result);
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }
        #endregion


        #region --Utilities--

        private static async Task<string?> StreamingAssetsToAbsolutePath (string relativePath) {
            var fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
            if (Application.platform != RuntimePlatform.Android)
                return File.Exists(fullPath) ? fullPath : null;
            var persistentPath = Path.Combine(Application.persistentDataPath, relativePath);
            if (File.Exists(persistentPath))
                return persistentPath;
            var directory = Path.GetDirectoryName(persistentPath);
            Directory.CreateDirectory(directory);
            using var request = UnityWebRequest.Get(fullPath);
            request.SendWebRequest();
            while (!request.isDone)
                await Task.Yield();
            if (request.result != UnityWebRequest.Result.Success)
                return null;
            File.WriteAllBytes(persistentPath, request.downloadHandler.data);
            return persistentPath;
        }

        private static string GetValueExtension (Dtype type) => type switch {
            Dtype.Image => ".png",
            Dtype.Audio => ".wav",
            Dtype.Video => ".mp4",
            _           => string.Empty,
        };

        private static MediaType GetMediaType<T> () => typeof(T) switch {
            var x when x == typeof(AudioBuffer) => MediaType.Audio,
            var x when x == typeof(PixelBuffer) => MediaType.Video,
            _                                   => MediaType.Unknown,
        };

        private readonly struct NativeMediaSequence : IReadOnlyList<MediaAsset?> {

            private readonly IntPtr asset;

            public NativeMediaSequence (IntPtr asset) => this.asset = asset;

            public int Count => asset.GetMediaAssetSubAssetCount(out var count) == Status.Ok ? count : default;

            public MediaAsset? this [int index] => asset.GetMediaAssetSubAsset(index, out var subAsset).Throw() == Status.Ok ? new MediaAsset(subAsset) : default;
    
            IEnumerator<MediaAsset?> IEnumerable<MediaAsset?>.GetEnumerator () {
                for (var idx = 0; idx < Count; ++idx)
                    yield return this[idx];
            }

            IEnumerator IEnumerable.GetEnumerator () => (this as IEnumerable<MediaAsset>).GetEnumerator();
        }
        #endregion
    }
}