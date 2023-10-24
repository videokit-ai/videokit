/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Assets {

    using AOT;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using Function.Types;
    using Newtonsoft.Json;
    using NJsonSchema;
    using NJsonSchema.Generation;
    using Internal;

    /// <summary>
    /// Text asset.
    /// </summary>
    public sealed class TextAsset : MediaAsset {

        #region --Client API--
        /// <summary>
        /// Text contents.
        /// </summary>
        public readonly string text;

        /// <summary>
        /// Parse a structured data model from the text asset's contents.
        /// NOTE: This requires an active VideoKit AI plan.
        /// </summary>
        public async Task<T?> To<T> () where T : struct {
            // Generate schema
            var settings = new JsonSchemaGeneratorSettings {
                GenerateAbstractSchemas = false,
                GenerateExamples = false,
                UseXmlDocumentation = false,
                ResolveExternalXmlDocumentation = false,
                FlattenInheritanceHierarchy = false,
            };
            var schema = JsonSchema.FromType<T>(settings);
            // Convert to structured
            var prediction = await VideoKitSettings.Instance.fxn.Predictions.Create(
                "@videokit/un2structured-v0-1",
                inputs: new () { [@"text"] = text, [@"schema"] = schema.ToJson() }
            ) as CloudPrediction;
            // Check
            if (!string.IsNullOrEmpty(prediction.error))
                throw new InvalidOperationException(prediction.error);
            // Parse
            var resultStr = prediction.results[0] as string;
            var result = JsonConvert.DeserializeObject<T>(resultStr);
            // Return
            return result;
        }

        /// <inheritdoc/>
        public override async Task<Value> ToValue (int minUploadSize = 4096) { // DEPLOY
            var buffer = Encoding.UTF8.GetBytes(text);
            using var stream = new MemoryStream(buffer);
            var value = await VideoKitSettings.Instance.fxn.Predictions.ToValue(
                stream,
                @"asset.txt",
                Dtype.String,
                minUploadSize: minUploadSize
            );
            return value;
        }

        /// <summary>
        /// This is unsupported and will raise an exception.
        /// </summary>
        public override Task<string?> Share (string? message = null) {
            throw new InvalidOperationException(@"Text asset cannot be shared");
        }

        /// <summary>
        /// This is unsupported and will raise an exception.
        /// </summary>
        public override Task<bool> SaveToCameraRoll (string? album = null) {
            throw new InvalidOperationException(@"Text asset cannot be saved to the camera roll");
        }
        #endregion


        #region --Operations--

        internal TextAsset (string text) => this.text = text;

        public static implicit operator string (TextAsset asset) => asset.text;
        #endregion
    }
}