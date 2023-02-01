/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit {

    using System;

    public sealed partial class VideoKitRecorder {

        #region --Enumerations--
        /// <summary>
        /// Recording format.
        /// </summary>
        public enum Format : int {
            /// <summary>
            /// MP4 video with H.264 AVC video codec.
            /// This format supports recording both video and audio.
            /// </summary>
            MP4 = 0,
            /// <summary>
            /// MP4 video with H.265 HEVC video codec.
            /// This format has better compression than `MP4`.
            /// This format supports recording both video and audio.
            /// </summary>
            HEVC = 1,
            /// <summary>
            /// WEBM video.
            /// This format support recording both video and audio.
            /// This is only supported on Android and WebGL.
            /// </summary>
            WEBM = 2,
            /// <summary>
            /// Animated GIF image.
            /// This format only supports recording video.
            /// </summary>
            GIF = 3,
            /// <summary>
            /// JPEG image sequence.
            /// This format only supports recording video.
            /// </summary>
            JPEG = 4,
            /// <summary>
            /// Waveform audio.
            /// This format only supports recording audio.
            /// </summary>
            WAV = 5,
        }

        /// <summary>
        /// Recorded media destination.
        /// </summary>
        [Flags]
        public enum Destination : int {
            /// <summary>
            /// Recorded media is discarded immediately.
            /// </summary>
            None        = 0,
            /// <summary>
            /// Record media to the app's private documents directory.
            /// </summary>
            Documents   = 1 << 0,
            /// <summary>
            /// Record images and videos to the camera roll.
            /// </summary>
            CameraRoll  = 1 << 1,
            /// <summary>
            /// Prompt the user to share the recorded media with the native sharing UI.
            /// </summary>
            PromptUser  = 1 << 2,
        }

        /// <summary>
        /// Video recording mode.
        /// </summary>
        public enum VideoMode : int {
            /// <summary>
            /// Don't record video.
            /// </summary>
            None            = 0,
            /// <summary>
            /// Record video frames from one or more game cameras.
            /// </summary>
            Camera          = 1,
            /// <summary>
            /// Record video frames from the screen.
            /// </summary>
            Screen          = 2,
            /// <summary>
            /// Record video frames from a texture.
            /// </summary>
            Texture         = 3,
            /// <summary>
            /// Record video frames from a camera device.
            /// </summary>
            CameraDevice    = 4,
        }

        /// <summary>
        /// Audio recording mode.
        /// </summary>
        [Flags]
        public enum AudioMode : int {
            /// <summary>
            /// Don't record audio.
            /// </summary>
            None            = 0,
            /// <summary>
            /// Record audio frames from the scene's audio listener.
            /// </summary>
            AudioListener   = 1 << 0,
            /// <summary>
            /// Record audio frames from an audio device.
            /// </summary>
            AudioDevice     = 1 << 1,
        }

        /// <summary>
        /// Video recording resolution presets.
        /// </summary>
        public enum Resolution : int {
            /// <summary>
            /// QVGA resolution.
            /// </summary>
            _320x240    = 5,
            /// <summary>
            /// HVGA resolution.
            /// </summary>
            _480x320    = 6,
            /// <summary>
            /// SD resolution.
            /// </summary>
            _640x480    = 0,
            /// <summary>
            /// HD resolution.
            /// </summary>
            _1280x720   = 1,
            /// <summary>
            /// Full HD resolution.
            /// </summary>
            _1920x1080  = 2,
            /// <summary>
            /// 2K WQHD resolution.
            /// </summary>
            _2K         = 3,
            /// <summary>
            /// 4K UHD resolution.
            /// </summary>
            _4K         = 4,
            /// <summary>
            /// Screen resolution.
            /// </summary>
            Screen      = 9,
            /// <summary>
            /// Half of the screen resolution.
            /// </summary>
            HalfScreen  = 10,
            /// <summary>
            /// Custom resolution.
            /// </summary>
            Custom      = 8,
        }

        /// <summary>
        /// Video orientation mode.
        /// </summary>
        public enum OrientationMode : int{
            /// <summary>
            /// Match the screen orientation.
            /// </summary>
            MatchScreen = 0,
            /// <summary>
            /// Record in landscape.
            /// </summary>
            Landscape   = 1,
            /// <summary>
            /// Record in portrait.
            /// </summary>
            Portrait    = 2,
        }

        /// <summary>
        /// Video aspect mode.
        /// </summary>
        public enum AspectMode : int {
            /// <summary>
            /// Use the video resolution aspect ratio.
            /// </summary>
            Resolution = 0,
            /// <summary>
            /// Match the screen aspect ratio by adjusting the video width.
            /// </summary>
            MatchScreenAdjustWidth = 1,
            /// <summary>
            /// Match the screen aspect ratio by adjusting the video height.
            /// </summary>
            MatchScreenAdjustHeight = 2,
        }

        /// <summary>
        /// Recorder status.
        /// </summary>
        public enum Status : int {
            /// <summary>
            /// No recording session is in progress.
            /// </summary>
            Idle = 0,
            /// <summary>
            /// Recording session is in progress.
            /// </summary>
            Recording = 1,
            /// <summary>
            /// Recording session is in progress but is paused.
            /// </summary>
            Paused = 2,
        }

        /// <summary>
        /// Video watermark mode.
        /// </summary>
        public enum WatermarkMode : int {
            /// <summary>
            /// No watermark.
            /// </summary>
            None = 0,
            /// <summary>
            /// Place watermark in the bottom-left of the frame.
            /// </summary>
            BottomLeft = 1,
            /// <summary>
            /// Place watermark in the bottom-right of the frame.
            /// </summary>
            BottomRight = 2,
            /// <summary>
            /// Place watermark in the upper-left of the frame.
            /// </summary>
            UpperLeft = 3,
            /// <summary>
            /// Place watermark in the upper-right of the frame.
            /// </summary>
            UpperRight = 4,
            /// <summary>
            /// Place watermark in a user-defined rectangle.
            /// Set the rect with the `watermarkRect` property.
            /// </summary>
            Custom = 5,
        }
        #endregion


        #region --Types--
        /// <summary>
        /// Recording session.
        /// </summary>
        public readonly struct RecordingSession {
            
            #region --Client API--
            /// <summary>
            /// Recording path.
            /// This is only populated when:
            /// 1. The session completed successfully.
            /// 2. The recorder `destination` includes `Destination.Documents`.
            /// </summary>
            public readonly string path;

            /// <summary>
            /// Recording exception.
            /// This is `null` if the session completed successfully.
            /// </summary>
            public readonly Exception exception;

            /// <summary>
            /// Bundle ID of app that received recording from user sharing action.
            /// This is only populated when:
            /// 1. The session completed successfully.
            /// 2. The recorder `destination` includes `Destination.PromptUser`.
            /// 3. The user completed the native sharing action (i.e. did not cancel).
            /// </summary>
            public readonly string receiverApp;

            /// <summary>
            /// Whether the video successfully saved to the camera roll.
            /// </summary>
            public readonly bool savedToCameraRoll;
            #endregion


            #region --Operations--

            internal RecordingSession (string path, Exception exception, string receiver, bool cameraRoll) {
                this.path = path;
                this.exception = exception;
                this.receiverApp = receiver;
                this.savedToCameraRoll = cameraRoll;
            }
            #endregion
        }
        #endregion
    }
}