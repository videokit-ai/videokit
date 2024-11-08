/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using UnityEngine;

    internal sealed class MediaAssetShareTest : MonoBehaviour {

        [SerializeField] private Texture2D image;

        private async void Start () {
            var asset = await MediaAsset.FromTexture(image);
            var receiver = await asset.Share();
            Debug.Log($"Asset shared to receiver: {receiver}");
        }
    }
}