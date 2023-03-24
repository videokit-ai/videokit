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
    /// CaptionKit speech-to-text predictor.
    /// </summary>
    internal sealed class CaptionKitPredictor : IMLPredictor<Task<CaptionKitPredictor.Caption[]>> { // INCOMPLETE

        #region --Transcription--
        /// <summary>
        /// Audio caption.
        /// </summary>
        public readonly struct Caption {

            /// <summary>
            /// Caption text.
            /// </summary>
            public readonly string text;

            /// <summary>
            /// Caption timestamp.
            /// </summary>
            public readonly long timestamp;

            public Caption (string text, long timestamp) {
                this.text = text;
                this.timestamp = timestamp;
            }
        }
        #endregion


        #region --Client API--
        /// <summary>
        /// Predict speech captions in a given video or audio feature.
        /// </summary>
        /// <param name="inputs">Input video or audio feature.</param>
        /// <returns>Predicted caption.</returns>
        public async Task<Caption[]> Predict (params MLFeature[] inputs) {
            return default;
        }

        /// <summary>
        /// Dispose the predictor and release resources.
        /// </summary>
        public void Dispose () => model.Dispose();

        /// <summary>
        /// Create the CaptionKit predictor.
        /// </summary>
        /// <param name="accessKey">NatML access key.</param>
        public static async Task<CaptionKitPredictor> Create (string accessKey = null) {
            return default;
        }
        #endregion


        #region --Operations--
        private readonly MLCloudModel model;

        private CaptionKitPredictor (MLCloudModel model) => this.model = model;
        #endregion
    }
}