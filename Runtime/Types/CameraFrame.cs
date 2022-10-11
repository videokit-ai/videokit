/* 
*   VideoKit
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
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
        /// Frame width.
        /// </summary>
        public readonly int width => texture.width; // `image.width` isn't guaranteed to match others.
        
        /// <summary>
        /// Frame height.
        /// </summary>
        public readonly int height => texture.height; // `image.width` isn't guaranteed to match others.

        /// <summary>
        /// Camera image for this frame.
        /// This is always non-null.
        /// </summary>
        public readonly CameraImage image;

        /// <summary>
        /// Texture for this frame.
        /// This is always non-null.
        /// </summary>
        public readonly Texture texture;

        /// <summary>
        /// Pixel buffer for this frame.
        /// This is only non-null when the `PixelData` capability is enabled on the camera manager.
        /// </summary>
        public readonly NativeArray<Color32> pixelBuffer;

        /// <summary>
        /// Image feature for this frame.
        /// This is only non-null when the `MachineLearning` capability is enabled on the camera manager.
        /// </summary>
        public readonly MLImageFeature feature;

        /// <summary>
        /// Human texture for this frame.
        /// This texture segments the human in the image and renders non-human pixels as transparent.
        /// This is only non-null when the `HumanTexture` capability is enabled on the camera manager.
        /// </summary>
        public readonly Texture humanTexture; // INCOMPLETE // MATTEKIT

        /// <summary>
        /// Frame timestamp in nanoseconds.
        /// </summary>
        public readonly long timestamp => image.timestamp;
        #endregion


        #region --Operations--

        internal CameraFrame (
            CameraImage image,
            Texture texture,
            NativeArray<Color32> pixelBuffer,
            MLImageFeature feature,
            Texture humanTexture
        ) {
            this.image = image;
            this.texture = texture;
            this.pixelBuffer = pixelBuffer;
            this.feature = feature;
            this.humanTexture = humanTexture;
        }

        public static implicit operator Texture (CameraFrame frame) => frame.texture;

        public static implicit operator MLImageFeature (CameraFrame frame) => frame.feature;
        #endregion
    }
}