/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit {

    using System;
    using UnityEngine;

    /// <summary>
    /// VideoKit editor for editing videos.
    /// </summary>
    [Tooltip(@"VideoKit editor for editing videos.")]
    [HelpURL(@"https://docs.videokit.ai/videokit/api/videokiteditor")]
    [DisallowMultipleComponent]
    [AddComponentMenu(@"")] // Hide this for now
    internal sealed class VideoKitEditor : MonoBehaviour {

        #region --Enumerations--
        /// <summary>
        /// Video editor capabilities.
        /// </summary>
        [Flags]
        public enum Capabilities : int {
            /// <summary>
            /// Generate speech captions for the video.
            /// </summary>
            Captions = 0b001,
        }
        #endregion


        #region --Inspector--
        /// <summary>
        /// Auto-play the video.
        /// </summary>
        public bool autoPlay;

        /// <summary>
        /// Mute any audio from the video.
        /// </summary>
        public bool mute;
        #endregion


        #region --Client API--

        #endregion


        #region --Operations--

        #endregion
    }
}