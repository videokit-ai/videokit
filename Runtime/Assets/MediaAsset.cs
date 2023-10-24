/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Assets {

    using AOT;
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Android;
    using Function.Types;
    using Internal;
    using Recorders;
    using static Internal.VideoKit;

    /// <summary>
    /// Media asset.
    /// </summary>
    public abstract class MediaAsset {

        #region --Client API--
        /// <summary>
        /// Path to media asset.
        /// </summary>
        public string path { get; protected set; }

        /// <summary>
        /// Convert the media asset to a Function value.
        /// </summary>
        /// <param name="minUploadSize">Media assets larger than this size in bytes will be uploaded.</param>
        /// <returns>Function value for making predictions.</returns>
        public abstract Task<Value> ToValue (int minUploadSize = 4096);

        /// <summary>
        /// Share the media asset using the native sharing UI.
        /// </summary>
        /// <param name="message">Optional message to share with the media asset.</param>
        /// <returns>Receiving app bundle ID or `null` if the user did not complete the share action.</returns>
        public virtual Task<string?> Share (string? message = null) {
            var tcs = new TaskCompletionSource<string?>();
            var handle = GCHandle.Alloc(tcs, GCHandleType.Normal);
            try {
                VideoKit.ShareAsset(path, message, OnShare, (IntPtr)handle).CheckStatus();
            } catch (Exception ex) {
                handle.Free();
                tcs.SetException(ex);
            }
            return tcs.Task;
        }

        /// <summary>
        /// Save the media asset to the camera roll.
        /// </summary>
        /// <param name="album">Optional album to save media asset to.</param>
        /// <returns>Whether the asset was successfully saved to the camera roll.</returns>
        public virtual Task<bool> SaveToCameraRoll (string? album = null) {
            var tcs = new TaskCompletionSource<bool>();
            var handle = GCHandle.Alloc(tcs, GCHandleType.Normal);
            try {
                VideoKit.SaveAssetToCameraRoll(path, album, OnSaveToCameraRoll, (IntPtr)handle).CheckStatus();
            } catch (Exception ex) {
                handle.Free();
                tcs.SetException(ex);
            }
            return tcs.Task;
        }

        /// <summary>
        /// Delete the media asset on disk.
        /// </summary>
        public virtual void Delete () { // INCOMPLETE // Handle WebGL
            if (Application.platform != RuntimePlatform.WebGLPlayer)
                File.Delete(path);
        }
        #endregion


        #region --Creators--
        /// <summary>
        /// Create a media aseet from a file.
        /// </summary>
        /// <param name="path">Path to media file.</param>
        /// <returns>Media asset.</returns>
        public static Task<MediaAsset> FromFile (string path) {
            // Check sequence
            var paths = Path.PathSeparator == ':' ?
                Regex.Split(path, @"(?<!blob|http|https):(?!\d)") :
                path.Split(Path.PathSeparator);
            if (paths.Length > 1)
                return FromSequence(paths);
            // Load
            var tcs = new TaskCompletionSource<MediaAsset>();
            var handle = GCHandle.Alloc(tcs, GCHandleType.Normal);
            try {
                VideoKit.LoadAsset(path, OnLoad, (IntPtr)handle).CheckStatus();
            } catch (Exception ex) {
                handle.Free();
                tcs.SetException(ex);
            }
            // Return
            return tcs.Task;
        }

        /// <summary>
        /// Create a media aseet from a texture.
        /// </summary>
        /// <param name="texture">Texture.</param>
        /// <returns>Image asset.</returns>
        public static Task<ImageAsset> FromTexture (Texture2D texture) {
            // Check
            if (!texture.isReadable)
                return Task.FromException<ImageAsset>(new ArgumentException(@"Cannot create media asset from texture that is not readable"));
            // Write to file
            string path = null;
            var encoded = texture.EncodeToPNG();                
            if (Application.platform == RuntimePlatform.WebGLPlayer) {
                var sb = new StringBuilder(2048);
                VideoKitExt.WriteImage(encoded, encoded.Length, sb, sb.Capacity).CheckStatus();
                path = sb.ToString();
            } else {
                var name = Guid.NewGuid().ToString("N");
                path = Path.Combine(Application.temporaryCachePath, $"{name}.png");
                File.WriteAllBytes(path, encoded);
            }
            // Create asset
            var asset = new ImageAsset(path, texture.width, texture.height);
            // Return
            return Task.FromResult(asset);
        }

        /// <summary>
        /// Create a media aseet from plain text.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <returns>Text asset.</returns>
        public static Task<TextAsset> FromText (string text) => Task.FromResult(new TextAsset(text));

        /// <summary>
        /// Create a media aseet from an audio clip.
        /// </summary>
        /// <param name="clip">Audio clip.</param>
        /// <returns>Audio asset.</returns>
        public static async Task<AudioAsset> FromAudioClip (AudioClip clip) {
            var sampleBuffer = new float[clip.samples * clip.channels];
            var recorder = await MediaRecorder.Create(MediaFormat.WAV, sampleRate: clip.frequency, channelCount: clip.channels);
            clip.GetData(sampleBuffer, 0);
            recorder.CommitSamples(sampleBuffer, 0L);
            var path = await recorder.FinishWriting();
            var asset = new AudioAsset(path, clip.frequency, clip.channels, clip.length);
            return asset;
        }

        /// <summary>
        /// Create a media asset by prompting the user to select an image or video from the camera roll.
        /// NOTE: This requires iOS 14+.
        /// </summary>
        /// <returns>Media asset.</returns>
        public static Task<MediaAsset?> FromCameraRoll<T> () where T : MediaAsset {
            var type = GetAssetType<T>();
            var tcs = new TaskCompletionSource<MediaAsset>();
            var handle = GCHandle.Alloc(tcs, GCHandleType.Normal);
            try {
                VideoKit.LoadAssetFromCameraRoll(type, OnLoad, (IntPtr)handle).CheckStatus();
            } catch (Exception ex) {
                handle.Free();
                tcs.SetException(ex);
            }
            return tcs.Task;
        }
        #endregion


        #region --Operations--

        protected Stream OpenReadStream () { // INCOMPLETE // WebGL support
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                return null;
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        private static async Task<MediaAsset> FromSequence (string[] paths) {
            var path = string.Join(Path.PathSeparator, paths);
            var assets = await Task.WhenAll(paths.Select(FromFile));
            var sequence = new MediaSequenceAsset(path, assets);
            return sequence;
        }

        [MonoPInvokeCallback(typeof(VideoKit.AssetLoadHandler))]
        private static void OnLoad (
            IntPtr context,
            IntPtr rawPath,
            AssetType type,
            int width,
            int height,
            float frameRate,
            int sampleRate,
            int channelCount,
            float duration
        ) {
            // Check
            if (!VideoKit.IsAppDomainLoaded)
                return;
            // Get tcs
            TaskCompletionSource<MediaAsset> tcs;
            try {
                var handle = (GCHandle)context;
                tcs = handle.Target as TaskCompletionSource<MediaAsset>;
                handle.Free();
            } catch (Exception ex) {
                Debug.LogException(ex);
                return;
            }
            // Invoke
            var path = Marshal.PtrToStringUTF8(rawPath);
            if (type == AssetType.Image)
                tcs?.SetResult(new ImageAsset(path, width, height));
            else if (type == AssetType.Audio)
                tcs?.SetResult(new AudioAsset(path, sampleRate, channelCount, duration));
            else if (type == AssetType.Video)
                tcs?.SetResult(new VideoAsset(path, width, height, frameRate, sampleRate, channelCount, duration));
            else
                tcs?.SetResult(null); // any errors get logged natively
        }

        [MonoPInvokeCallback(typeof(AssetShareHandler))]
        private static void OnShare (IntPtr context, IntPtr receiver) {
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
            var result = Marshal.PtrToStringUTF8(receiver);
            tcs?.SetResult(result);
        }

        [MonoPInvokeCallback(typeof(AssetShareHandler))]
        private static void OnSaveToCameraRoll (IntPtr context, IntPtr receiver) {
            // Check
            if (!VideoKit.IsAppDomainLoaded)
                return;
            // Get tcs
            TaskCompletionSource<bool> tcs;
            try {
                var handle = (GCHandle)context;
                tcs = handle.Target as TaskCompletionSource<bool>;
                handle.Free();
            } catch (Exception ex) {
                Debug.LogException(ex);
                return;
            }
            // Invoke
            var result = receiver != IntPtr.Zero;
            tcs?.SetResult(result);
        }

        private static AssetType GetAssetType<T>() where T : MediaAsset => typeof(T) switch {
            var t when t == typeof(ImageAsset)  => VideoKit.AssetType.Image,
            var t when t == typeof(AudioAsset)  => VideoKit.AssetType.Audio,
            var t when t == typeof(VideoAsset)  => VideoKit.AssetType.Video,
            _                                   => VideoKit.AssetType.Unknown,
        };
        #endregion
    }
}