/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

#nullable enable

namespace VideoKit {

    /// <summary>
    /// Recording format.
    /// </summary>
    public enum MediaFormat : int {
        /// <summary>
        /// MP4 video with H.264 AVC video codec.
        /// This format supports recording both video and audio frames.
        /// This format is not supported on WebGL.
        /// </summary>
        MP4 = 0,
        /// <summary>
        /// MP4 video with H.265 HEVC video codec.
        /// This format has better compression than `MP4`.
        /// This format supports recording both video and audio frames.
        /// This format is not supported on WebGL.
        /// </summary>
        HEVC = 1,
        /// <summary>
        /// WEBM video.
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
        /// This format only supports recording audio.
        /// </summary>
        WAV = 5,
    }
}