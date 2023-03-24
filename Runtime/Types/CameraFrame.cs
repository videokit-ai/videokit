/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit {

    using UnityEngine;
    using Unity.Collections;
    using NatML.Devices;
    using NatML.Features;

    /// <summary>
    /// VideoKit camera frame.
    /// This is a lightweight collection of different representations of a camera image.
    /// </summary>
    public readonly struct CameraFrame {

        #region --Client API--
        /// <summary>
        /// Camera image for this frame.
        /// </summary>
        public readonly CameraImage image;

        /// <summary>
        /// Texture for this frame.
        /// This is always populated.
        /// </summary>
        public readonly Texture texture;

        /// <summary>
        /// Image feature for this frame.
        /// This is only populated when the `MachineLearning` capability is enabled on the camera manager.
        /// </summary>
        public readonly MLImageFeature feature;

        /// <summary>
        /// Human texture for this frame.
        /// This texture segments the human in the image and leaves non-human pixels transparent.
        /// This is only populated when the `HumanTexture` capability is enabled on the camera manager.
        /// </summary>
        public readonly Texture humanTexture;
        #endregion


        #region --Operations--

        internal CameraFrame (CameraImage image, Texture texture, MLImageFeature feature, Texture humanTexture) {
            this.image = image;
            this.texture = texture;
            this.feature = feature;
            this.humanTexture = humanTexture;
        }

        public static implicit operator CameraImage (CameraFrame frame) => frame.image;

        public static implicit operator Texture (CameraFrame frame) => frame.texture;

        public static implicit operator MLImageFeature (CameraFrame frame) => frame.feature;
        #endregion
    }
}