/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.ML {

    using System;
    using System.Threading.Tasks;
    using UnityEngine;
    using NatML.Features;
    using NatML.Internal;
    using NatML.Types;

    /// <summary>
    /// SpeechKit text-to-speech predictor.
    /// </summary>
    internal sealed class SpeechKitPredictor : IMLPredictor<Task<AudioClip>> { // INCOMPLETE

        #region --Client API--
        /// <summary>
        /// Predict speech from a given text.
        /// </summary>
        /// <param name="inputs">Input string feature.</param>
        /// <returns>Predicted speech audio.</returns>
        public async Task<AudioClip> Predict (params MLFeature[] inputs) {
            return default;
        }

        /// <summary>
        /// Dispose the predictor and release resources.
        /// </summary>
        public void Dispose () => model.Dispose();

        /// <summary>
        /// Create the SpeechKit predictor.
        /// </summary>
        /// <param name="accessKey">NatML access key.</param>
        public static async Task<SpeechKitPredictor> Create (string accessKey = null) {
            return default;
        }
        #endregion


        #region --Operations--
        private readonly MLCloudModel model;

        private SpeechKitPredictor (MLCloudModel model) => this.model = model;
        #endregion
    }
}