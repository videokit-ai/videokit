/* 
*   VideoKit
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.Inputs {

    using System;
    using UnityEngine;
    using NatML.Devices;
    using NatML.Recorders;
    using NatML.Recorders.Clocks;

    /// <summary>
    /// </summary>
    internal sealed class AudioMixerInput : IDisposable { // INCOMPLETE

        #region --Client API--
        /// <summary>
        /// Create an audio mixer input.
        /// </summary>
        /// <param name="recorder">Media recorder to receive audio frames.</param>
        /// <param name="clock">Clock for generating timestamps. Can be `null` if recorder does not require timestamps.</param>
        /// <param name="audioDevice">Audio device to record microphone audio from.</param>
        /// <param name="audioListener">Audio listener to record game audio from.</param>
        public AudioMixerInput (IMediaRecorder recorder, IClock clock, AudioDevice audioDevice, AudioListener audioListener) {

        }

        /// <summary>
        /// Create an audio mixer input.
        /// </summary>
        /// <param name="recorder">Media recorder to receive audio frames.</param>
        /// <param name="audioDevice">Audio device to record microphone audio from.</param>
        /// <param name="audioListener">Audio listener to record game audio from.</param>
        public AudioMixerInput (IMediaRecorder recorder, AudioDevice audioDevice, AudioListener audioListener) : this(recorder, default, audioDevice, audioListener) { }

        /// <summary>
        /// Stop recorder input and release resources.
        /// </summary>
        public void Dispose () {

        }
        #endregion
    }
}