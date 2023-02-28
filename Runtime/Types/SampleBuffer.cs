/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit {

    using UnityEngine;
    using Unity.Collections;
    using NatML.Devices;

    /// <summary>
    /// VideoKit audio sample buffer.
    /// This is a lightweight collection of different representations of an audio sample buffer.
    /// </summary>
    public readonly struct SampleBuffer {

        #region --Client API--
        /// <summary>
        /// Audio buffer.
        /// </summary>
        public readonly AudioBuffer audioBuffer;
        #endregion


        #region --Operations--

        internal SampleBuffer (AudioBuffer audioBuffer) {
            this.audioBuffer = audioBuffer;
        }

        public static implicit operator AudioBuffer (SampleBuffer sampleBuffer) => sampleBuffer.audioBuffer;
        #endregion
    }
}