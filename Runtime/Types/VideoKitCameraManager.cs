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
        public enum Resolution {
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
        /// Camera manager capabilities.
        /// </summary>
        [Flags]
        public enum Capabilities : int {
            /// <summary>
            /// Stream depth data along with the camera preview data.
            /// This flag adds a minimal performance cost, so enable it only when necessary.
            /// This flag is only supported on iOS and Android.
            /// </summary>
            Depth           = 1,
            /// <summary>
            /// Ensure that the camera preview data can be used for machine learning predictions.
            /// Enabling this adds a minimal performance impact so it should be disabled when not needed.
            /// This flag is always enabled on WebGL.
            /// </summary>
            MachineLearning = 2,
            /// <summary>
            /// Generate a human segmentation texture from the camera preview stream.
            /// This flag adds a variable performance cost, so enable it only when necessary.
            /// </summary>
            HumanTexture    = 4 | Capabilities.MachineLearning,
            /// <summary>
            /// Detect human poses in the camera preview stream.
            /// This flag adds a variable performance cost, so enable it only when necessary.
            /// </summary>
            PoseDetection   = 8 | Capabilities.MachineLearning,
            /// <summary>
            /// Detect faces in the camera preview stream.
            /// This flag adds a variable performance cost, so enable it only when necessary.
            /// </summary>
            FaceDetection   = 16 | Capabilities.MachineLearning,
        }
        #endregion
    }
}