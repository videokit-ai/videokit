/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Assets {

    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Function.Types;
    using Internal;

    /// <summary>
    /// Audio asset.
    /// </summary>
    public sealed class AudioAsset : MediaAsset {

        #region --Client API--
        /// <summary>
        /// Audio sample rate.
        /// </summary>
        public readonly int sampleRate;

        /// <summary>
        /// Audio channel count.
        /// </summary>
        public readonly int channelCount;

        /// <summary>
        /// Audio duration in seconds.
        /// </summary>
        public readonly float duration;

        /// <summary>
        /// Generate captions for the audio asset.
        /// NOTE: This requires an active VideoKit AI plan.
        /// </summary>
        public async Task<string> Caption () {
            // Caption
            using var stream = OpenReadStream();      
            var prediction = await VideoKitSettings.Instance.fxn.Predictions.Create(
                "@videokit/caption-v0-1",
                inputs: new () { [@"audio"] = stream }
            ) as CloudPrediction;
            // Check
            if (!string.IsNullOrEmpty(prediction.error))
                throw new InvalidOperationException(prediction.error);
            // Return
            var result = prediction.results[0] as string;
            return result;
        }

        /// <inheritdoc/>
        public override async Task<Value> ToValue (int minUploadSize = 4096) { // DEPLOY
            using var stream = OpenReadStream();
            var name = Path.GetFileName(path);
            var value = await VideoKitSettings.Instance.fxn.Predictions.ToValue(
                stream,
                name,
                Dtype.Audio,
                minUploadSize: minUploadSize
            );
            return value;
        }
        #endregion


        #region --Operations--

        internal AudioAsset (string path, int sampleRate, int channelCount, float duration) {
            this.path = path;
            this.sampleRate = sampleRate;
            this.channelCount = channelCount;
            this.duration = duration;
        }
        #endregion
    }
}