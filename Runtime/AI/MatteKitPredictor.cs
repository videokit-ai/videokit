/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.AI {

    using System;
    using System.Threading.Tasks;
    using UnityEngine;
    using Unity.Collections.LowLevel.Unsafe;
    using NatML;
    using NatML.Features;
    using NatML.Internal;
    using NatML.Types;
    using Internal;

    /// <summary>
    /// MatteKit human texture predictor.
    /// </summary>
    public sealed class MatteKitPredictor : IMLPredictor<object> {

        #region --Client API--
        /// <summary>
        /// MatteKit graph variant.
        /// </summary>
        public enum Variant : int {
            /// <summary>
            /// Default variant.
            /// This will choose a landscape or portrait graph depending on the screen orientation.
            /// </summary>
            Default = 0,
            /// <summary>
            /// Landscape HD graph.
            /// </summary>
            _1280x720 = 1,
            /// <summary>
            /// Portrait HD graph.
            /// </summary>
            _720x1280 = 2
        }

        /// <summary>
        /// Human texture.
        /// Access this after calling `Predict` with an image feature.
        //// </summary>
        public readonly Texture2D humanTexture;

        /// <summary>
        /// Predict the human texture from a given image.
        /// </summary>
        /// <param name="features">Image feature.</param>
        /// <returns>Null. Access the `humanTexture` or `backgroundTexture` instead.</returns>
        public unsafe object Predict (params MLFeature[] features) {
            // Predict
            var inputType = model.inputs[0] as MLImageType;
            using var inputFeature = (features[0] as IMLEdgeFeature).Create(inputType);
            using var outputFeatures = model.Predict(inputFeature);            
            // Resize
            var humanFeature = new MLArrayFeature<float>(outputFeatures[0]);
            var width = humanFeature.shape[2];
            var height = humanFeature.shape[1];
            if (humanTexture.width != width || humanTexture.height != height)
                humanTexture.Reinitialize(width, height);
            // Copy
            var elementsPerRow = width * 4;
            var bytesPerRow = elementsPerRow * sizeof(float);
            var dst = (float*)humanTexture.GetRawTextureData<float>().GetUnsafePtr();
            fixed (float* src = humanFeature)
                UnsafeUtility.MemCpyStride(
                    dst,
                    bytesPerRow,
                    src + (height - 1) * elementsPerRow,
                    -bytesPerRow,
                    bytesPerRow,
                    height
                );
            humanTexture.Apply();
            // Return
            return default;
        }

        /// <summary>
        /// Dispose the predictor and release resources.
        /// </summary>
        public void Dispose () {
            model.Dispose();
            Texture2D.Destroy(humanTexture);
        }

        /// <summary>
        /// Create the MatteKit predictor.
        /// NOTE: This requires an active VideoKit AI plan.
        /// </summary>
        /// <param name="variant">MatteKit graph variant.</param>
        /// <param name="configuration">MatteKit model configuration.</param>
        /// <returns>MatteKit predictor.</returns>
        public static async Task<MatteKitPredictor> Create (
            Variant variant = Variant.Default,
            MLEdgeModel.Configuration configuration = null
        ) {
            variant = variant == Variant.Default ? GetDefaultVariant() : variant;
            var tag = GetTag(variant);
            var model = await MLEdgeModel.Create(tag, configuration, VideoKitSettings.Instance.natml);
            var predictor = new MatteKitPredictor(model);
            return predictor;
        }
        #endregion


        #region --Operations--
        private readonly MLEdgeModel model;

        private MatteKitPredictor (MLEdgeModel model) {
            this.model = model;
            this.humanTexture = new Texture2D(16, 16, TextureFormat.RGBAFloat, false);
        }

        private static string GetTag (Variant variant) => variant switch {
            Variant._1280x720   => "@videokit/mattekit@1280x720-ne",
            Variant._720x1280   => "@videokit/mattekit@720x1280-ne",
            _                   => null,
        };

        private static Variant GetDefaultVariant () => 
            Array.IndexOf(new [] { RuntimePlatform.Android, RuntimePlatform.IPhonePlayer }, Application.platform) >= 0 &&
            Array.IndexOf(new [] { ScreenOrientation.Portrait, ScreenOrientation.PortraitUpsideDown }, Screen.orientation) >= 0 ?
            Variant._720x1280 : Variant._1280x720;
        #endregion
    }
}