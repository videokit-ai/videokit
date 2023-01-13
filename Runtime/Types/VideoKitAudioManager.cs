/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit {

    using System;

    public sealed partial class VideoKitAudioManager {

        #region --Enumerations--
        /// <summary>
        /// Audio sample rate.
        /// </summary>
        public enum SampleRate : int {
            /// <summary>
            /// Match Unity's audio DSP sample rate.
            /// </summary>
            MatchUnity = 0,
            /// <summary>
            /// 8KHz
            /// </summary>
            _8000 = 8000,
            /// <summary>
            /// 16KHz
            /// </summary>
            _160000 = 16000,
            /// <summary>
            /// 22.05KHz
            /// </summary>
            _22050 = 22050,
            /// <summary>
            /// 24KHz
            /// </summary>
            _24000 = 24000,
            /// <summary>
            /// 44.1KHz
            /// </summary>
            _44100 = 44100,
            /// <summary>
            /// 48KHz
            /// </summary>
            _48000 = 48000,
        }

        /// <summary>
        /// Audio channel count.
        /// </summary>
        public enum ChannelCount : int {
            /// <summary>
            /// Match Unity's audio DSP channel count.
            /// </summary>
            MatchUnity = 0,
            /// <summary>
            /// Mono audio.
            /// </summary>
            Mono = 1,
            /// <summary>
            /// Stereo audio.
            /// </summary>
            Stereo = 2,
        }
        #endregion
    }
}