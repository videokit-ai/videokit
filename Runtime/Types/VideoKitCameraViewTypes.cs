/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.UI {

    public sealed partial class VideoKitCameraView {
        
        #region --Enumerations--
        /// <summary>
        /// View mode.
        /// </summary>
        public enum ViewMode : int {
            /// <summary>
            /// Display the camera texture.
            /// </summary>
            CameraTexture       = 0,
            /// <summary>
            /// Display the human texture.
            /// </summary>
            HumanTexture        = 1,
        }

        /// <summary>
        /// Gesture mode.
        /// </summary>
        public enum GestureMode : int {
            /// <summary>
            /// Do not respond to gestures.
            /// </summary>
            None = 0,
            /// <summary>
            /// Detect tap gestures.
            /// </summary>
            Tap = 1,
            /// <summary>
            /// Detect two-finger pinch gestures.
            /// </summary>
            Pinch = 2,
            /// <summary>
            /// Detect single-finger drag gestures.
            /// This gesture mode is recommended when the user is holding a button to record a video.
            /// </summary>
            Drag = 3,
        }
        #endregion
    }
}