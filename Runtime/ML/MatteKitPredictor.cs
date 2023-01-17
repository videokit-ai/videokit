/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.ML {

    using UnityEngine;
    using NatML.Features;
    using NatML.Internal;
    using NatML.Types;

    internal sealed class MatteKitPredictor : IMLPredictor<Texture> { // we're breaking the rules a little bit

        #region --Client API--
        /// <summary>
        /// Create the MatteKit predictor.
        /// </summary>
        /// <param name="model">MatteKit model.</param>
        public MatteKitPredictor (MLModel model) {
            this.model = model as MLEdgeModel;
            this.texture = new Texture2D(16, 16, TextureFormat.RGBAFloat, false);
        }

        /// <summary>
        /// Predict the human texture from a given image.
        /// </summary>
        /// <param name="features">Image feature.</param>
        public Texture Predict (params MLFeature[] features) { // violation #1: using Unity APIs within `Predict`
            var inputType = model.inputs[0] as MLImageType;
            using var inputFeature = (features[0] as IMLEdgeFeature).Create(inputType);
            using var outputFeatures = model.Predict(inputFeature);
            var outputFeature = new MLArrayFeature<float>(outputFeatures[0]);
            var outputType = MLImageType.FromType(outputFeature.type);
            if (texture.width != outputType.width || texture.height != outputType.height)
                texture.Reinitialize(outputType.width, outputType.height);
            outputFeature.CopyTo(texture.GetRawTextureData<float>());
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Dispose the predictor and release resources.
        /// </summary>
        public void Dispose () { // violation #2: disposing model in predictor
            model.Dispose();
            Texture2D.Destroy(texture);
        }
        #endregion


        #region --Operations--
        private readonly MLEdgeModel model;
        private readonly Texture2D texture;
        #endregion
    }
}