/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit {

    using System;

    public sealed partial class VideoKitCameraManager {

        #region --Enumerations--
        /// <summary>
        /// Camera resolution presets.
        /// </summary>
        public enum Resolution : int {
            /// <summary>
            /// Use the default camera resolution.
            /// With this preset, the camera resolution will not be set.
            /// </summary>
            Default     = 0,
            /// <summary>
            /// Lowest resolution supported by the camera device.
            /// </summary>
            Lowest      = 1,
            /// <summary>
            /// SD resolution.
            /// </summary>
            _640x480    = 2,
            /// <summary>
            /// HD resolution.
            /// </summary>
            _1280x720   = 3,
            /// <summary>
            /// Full HD resolution.
            /// </summary>
            _1920x1080  = 4,
            /// <summary>
            /// 4K UHD resolution.
            /// </summary>
            _4K         = 5,
            /// <summary>
            /// Highest resolution supported by the camera device.
            /// Using this resolution is strongly not recommended.
            /// </summary>
            Highest     = 10
        }

        /// <summary>
        /// Camera facing.
        /// </summary>
        public enum Facing : int { // bit flags: [prefer/require user/world]
            /// <summary>
            /// Prefer a user-facing camera but enable fallback to any available camera.
            /// </summary>
            PreferUser = 0b00,
            /// <summary>
            /// Prefer a world-facing camera but enable fallback to any available camera.
            /// </summary>
            PreferWorld = 0b01,
            /// <summary>
            /// Require a user-facing camera.
            /// </summary>
            RequireUser = 0b10,
            /// <summary>
            /// Require a world-facing camera.
            /// </summary>
            RequireWorld = 0b11,
        }

        /// <summary>
        /// Camera manager capabilities.
        /// </summary>
        [Flags]
        public enum Capabilities : int {
            /// <summary>
            /// Stream depth data along with the camera preview data.
            /// This flag adds a minimal performance cost, so enable it only when necessary.
            /// This flag is only supported on iOS and Android.
            /// </summary>
            Depth               = 0b0001,
            /// <summary>
            /// Ensure that the camera preview data can be used for machine learning predictions.
            /// Enabling this adds a minimal performance impact so it should be disabled when not needed.
            /// This flag is always enabled on WebGL.
            /// </summary>
            MachineLearning     = 0b00010,
            /// <summary>
            /// Generate a human texture from the camera preview stream.
            /// This flag adds a variable performance cost, so enable it only when necessary.
            /// </summary>
            HumanTexture        = MachineLearning | 0b00100,
            /// <summary>
            /// Detect human poses in the camera preview stream.
            /// This flag adds a variable performance cost, so enable it only when necessary.
            /// </summary>
            PoseDetection       = MachineLearning | 0b01000,
            /// <summary>
            /// Detect faces in the camera preview stream.
            /// This flag adds a variable performance cost, so enable it only when necessary.
            /// </summary>
            FaceDetection       = MachineLearning | 0b10000,
        }
        #endregion
    }
}