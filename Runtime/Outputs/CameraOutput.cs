/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Devices.Outputs {

    using System;

    /// <summary>
    /// Camera device output which consumes camera images.
    /// </summary>
    public abstract class CameraOutput {

        #region --Client API--
        /// <summary>
        /// Latest camera image processed by the camera output.
        /// </summary>
        public CameraImage image { get; protected set; }

        /// <summary>
        /// Latest camera image timestamp.
        /// This is the timestamp of the latest cimage processed by the camera output.
        /// </summary>
        public long timestamp => image.timestamp;

        /// <summary>
        /// Update the output with a new camera image.
        /// </summary>
        /// <param name="image">Camera image.</param>
        public abstract void Update (CameraImage image);

        /// <summary>
        /// Dispose the camera output and release resources.
        /// </summary>
        public virtual void Dispose () {}
        #endregion


        #region --Operations--
        public static implicit operator Action<CameraImage> (CameraOutput output) => output.Update;
        #endregion
    }
}