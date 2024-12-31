/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using UnityEngine;
    using UnityEngine.UI;

    internal sealed class MediaAssetFromCameraRollTest : MonoBehaviour {

        [SerializeField] private RawImage rawImage;

        private async void Start () {
            var asset = await MediaAsset.FromCameraRoll(MediaAsset.MediaType.Image);
            if (asset != null)
                rawImage.texture = await asset.ToTexture();
            else
                Debug.Log("User did not pick image from camera roll");
        }
    }
}