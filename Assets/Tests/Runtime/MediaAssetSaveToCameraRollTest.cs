/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using UnityEngine;

    internal sealed class MediaAssetSaveToCameraRollTest : MonoBehaviour {

        [SerializeField] private Texture2D image;

        private async void Start() {
            var asset = await MediaAsset.FromTexture(image);
            var saved = await asset.SaveToCameraRoll();
            Debug.Log($"Asset saved to camera roll: {saved}");
        }
    }
}