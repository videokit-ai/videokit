/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Devices.Outputs {

    using System;

    /// <summary>
    /// </summary>
    public abstract class AudioOutput {

        #region --Client API--
        /// <summary>
        /// Latest audio buffer processed by the audio output.
        /// </summary>
        public AudioBuffer buffer { get; protected set; }

        /// <summary>
        /// Latest audio buffer timestamp.
        /// This is the timestamp of the latest buffer processed by the audio output.
        /// </summary>
        public long timestamp => buffer.timestamp;

        /// <summary>
        /// Update the output with a new audio buffer.
        /// </summary>
        /// <param name="audioBuffer">Audio buffer.</param>
        public abstract void Update (AudioBuffer audioBuffer);

        /// <summary>
        /// Dispose the audio output and release resources.
        /// </summary>
        public virtual void Dispose () {}
        #endregion


        #region --Operations--
        /// <summary>
        /// Implicitly convert the output to an audio buffer delegate.
        /// </summary>
        public static implicit operator Action<AudioBuffer> (AudioOutput output) => output.Update;
        #endregion
    }
}