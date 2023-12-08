/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Assets {

    using System.IO;
    using System.Threading.Tasks;
    using UnityEngine;
    using Function.Types;
    using Internal;

    /// <summary>
    /// Video asset.
    /// </summary>
    public sealed class VideoAsset : MediaAsset {

        #region --Client API--
        /// <summary>
        /// Video width.
        /// </summary>
        public readonly int width;

        /// <summary>
        /// Video height.
        /// </summary>
        public readonly int height;

        /// <summary>
        /// Video frame rate.
        /// </summary>
        public readonly float frameRate;

        /// <summary>
        /// Audio sample rate.
        /// </summary>
        public readonly int sampleRate;

        /// <summary>
        /// Audio channel count.
        /// </summary>
        public readonly int channelCount;

        /// <summary>
        /// Video duration in seconds.
        /// </summary>
        public readonly float duration;

        /// <summary>
        /// Playback the video asset in the native media player if supported.
        /// </summary>
        public void Playback () {
            #if UNITY_EDITOR
            //UnityEditor.EditorUtility.OpenWithDefaultApp(path); // errors on macOS for whatever reason
            #elif UNITY_IOS || UNITY_ANDROID
            Handheld.PlayFullScreenMovie($"file://{path}");
            #endif
        }

        /// <summary>
        /// Create a thumbnail image from the video.
        /// </summary>
        /// <param name="time">Approximate time to create the thumbnail from.</param>
        /// <returns>Thumbnail image.</returns>
        internal Task<Texture2D> CreateThumbnail (float time = 0f) { // INCOMPLETE
            return default;
        }

        /// <summary>
        /// Trim the video based on the start and end time.
        /// NOTE: This requires an active VideoKit plan.
        /// </summary>
        /// <param name="start">Start time in seconds.</param>
        /// <param name="duration">Duration in seconds. If negative, trim to the end of the video.</param>
        /// <returns>Resulting trimmed video.</returns>
        internal Task<VideoAsset> Trim (float start = 0f, float duration = -1f) { // INCOMPLETE
            return default;
        }

        /// <summary>
        /// Reverse the video.
        /// NOTE: This requires an active VideoKit plan.
        /// </summary>
        internal Task<VideoAsset> Reverse () { // INCOMPLETE
            return default;
        }

        /// <summary>
        /// Extract the audio track from a video.
        /// NOTE: This requires an active VideoKit plan.
        /// </summary>
        internal Task<AudioAsset> ExtractAudio () { // INCOMPLETE
            return default;
        }

        /// <inheritdoc/>
        public override async Task<Value> ToValue (int minUploadSize = 4096) { // DEPLOY
            using var stream = OpenReadStream();
            var fxn = VideoKitSettings.Instance.fxn;
            var name = Path.GetFileName(path);
            var value = await fxn.Predictions.ToValue(stream, name, Dtype.Video, minUploadSize: minUploadSize);
            return value;
        }
        #endregion


        #region --Operations--

        internal VideoAsset (string path, int width, int height, float frameRate, int sampleRate, int channelCount, float duration) {
            this.path = path;
            this.width = width;
            this.height = height;
            this.frameRate = frameRate;
            this.sampleRate = sampleRate;
            this.channelCount = channelCount;
            this.duration = duration;
        }
        #endregion
    }
}