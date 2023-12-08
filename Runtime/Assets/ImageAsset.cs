/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Assets {

    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;
    using Function.Types;
    using Internal;

    /// <summary>
    /// Image asset.
    /// </summary>
    public sealed class ImageAsset : MediaAsset {

        #region --Client API--
        /// <summary>
        /// Image width.
        /// </summary>
        public readonly int width;

        /// <summary>
        /// Image height.
        /// </summary>
        public readonly int height;

        /// <summary>
        /// Load the image asset into a texture.
        /// </summary>
        public async Task<Texture2D?> ToTexture () {
            var uri = path[0] == '/' ? $"file://{path}" : path;
            using var request = UnityWebRequestTexture.GetTexture(uri);
            request.SendWebRequest();
            while (!request.isDone)
                await Task.Yield();
            if (request.result != UnityWebRequest.Result.Success) {
                Debug.LogError($"VideoKit: Image asset failed to create texture with error: {request.error}");
                return null;
            }
            var texture = DownloadHandlerTexture.GetContent(request);
            return texture;
        }

        /// <inheritdoc/>
        public override async Task<Value> ToValue (int minUploadSize = 4096) { // DEPLOY
            using var stream = OpenReadStream();
            var name = Path.GetFileName(path);
            var value = await VideoKitSettings.Instance.fxn.Predictions.ToValue(
                stream,
                name,
                Dtype.Image,
                minUploadSize: minUploadSize
            );
            return value;
        }
        #endregion


        #region --Operations--

        internal ImageAsset (string path, int width, int height) {
            this.path = path;
            this.width = width;
            this.height = height;
        }
        #endregion        
    }
}