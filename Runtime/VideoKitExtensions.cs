/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit {

    using System.Linq;

    /// <summary>
    /// VideoKit extension methods.
    /// </summary>
    public static class VideoKitExtensions {

        #region --Client API--
        /// <summary>
        /// Whether this format supports recording video frames.
        /// </summary>
        public static bool SupportsVideo (this MediaFormat format) => new [] {
            MediaFormat.MP4, MediaFormat.HEVC, MediaFormat.WEBM, MediaFormat.GIF, MediaFormat.JPEG
        }.Contains(format);

        /// <summary>
        /// Whether this format supports recording audio frames.
        /// </summary>
        public static bool SupportsAudio (this MediaFormat format) => new [] {
            MediaFormat.MP4, MediaFormat.HEVC, MediaFormat.WEBM, MediaFormat.WAV
        }.Contains(format);
        #endregion
    }
}