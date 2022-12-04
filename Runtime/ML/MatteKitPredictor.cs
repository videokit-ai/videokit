/* 
*   VideoKit
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.ML {

    using UnityEngine;
    using NatML.Features;
    using NatML.Internal;
    using NatML.Types;

    internal sealed class MatteKitPredictor : IMLPredictor<Texture> {
        
        #region --Client API--

        public MatteKitPredictor (MLModelData modelData) {
            this.model = new MLEdgeModel(modelData);
            this.texture = new Texture2D(16, 16, TextureFormat.RGBAFloat, false);
        }

        public Texture Predict (params MLFeature[] features) {
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

        public void Dispose () {
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