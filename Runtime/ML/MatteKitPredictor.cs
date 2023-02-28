/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.ML {

    using System;
    using System.Threading.Tasks;
    using UnityEngine;
    using Unity.Collections.LowLevel.Unsafe;
    using NatML.Features;
    using NatML.Internal;
    using NatML.Types;

    /// <summary>
    /// MatteKit human texture predictor.
    /// We're breaking the NatML rules a bit, because this predictor owns and returns the textures directly.
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
        /// </summary>
        /// <param name="variant">MatteKit graph variant.</param>
        /// <param name="configuration">MatteKit model configuration.</param>
        /// <param name="accessKey">NatML access key.</param>
        /// <returns>MatteKit predictor.</returns>
        public static async Task<MatteKitPredictor> Create (
            Variant variant = Variant.Default,
            MLEdgeModel.Configuration configuration = null,
            string accessKey = null
        ) {
            try {
                variant = variant == Variant.Default ? GetDefaultVariant() : variant;
                var tag = GetTag(variant);
                var model = await MLEdgeModel.Create(tag, configuration, accessKey);
                var predictor = new MatteKitPredictor(model);
                return predictor;
            } catch (Exception ex) {
                // We need to throw a more informative error than the generic "predictor tag is invalid"
                throw new InvalidOperationException(@"Failed to create MatteKitPredictor. Check that your access key is correct and that you are on the VideoKit ML plan.");
            }
        }
        #endregion


        #region --Operations--
        private readonly MLEdgeModel model;

        /// <summary>
        /// Create the MatteKit predictor.
        /// </summary>
        /// <param name="model">MatteKit model.</param>
        private MatteKitPredictor (MLEdgeModel model) {
            this.model = model;
            this.humanTexture = new Texture2D(16, 16, TextureFormat.RGBAFloat, false);
        }

        /// <summary>
        /// MatteKit uses static graph variants because CoreML has such poor support
        /// for models with dynamic IO tensors. Furthermore, static graphs enjoy better runtime
        /// performance because of fixed execution plans and allocations. VideoKit uses the image
        /// resolution to define variants for MatteKit graphs.
        /// </summary>
        private static string GetTag (Variant variant) => variant switch {
            Variant._1280x720   => "@videokit/mattekit@1280x720",
            Variant._720x1280   => "@videokit/mattekit@720x1280",
            _                   => null,
        };

        /// <summary>
        /// When running on a mobile device in portrait, we use the portrait HD variant.
        /// </summary>
        private static Variant GetDefaultVariant () => 
            Array.IndexOf(new [] { RuntimePlatform.Android, RuntimePlatform.IPhonePlayer }, Application.platform) >= 0 &&
            Array.IndexOf(new [] { ScreenOrientation.Portrait, ScreenOrientation.PortraitUpsideDown }, Screen.orientation) >= 0 ?
            Variant._720x1280 : Variant._1280x720;
        #endregion
    }
}