/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using UnityEngine;
    using UnityEngine.UI;

    internal sealed class MediaAssetFromAudioClipTest : MonoBehaviour {

        [SerializeField] private AudioClip clip;

        private async void Start () {
            var asset = await MediaAsset.FromAudioClip(clip);
            Debug.Log($"Created media asset at path: {asset.path}");
        }
    }
}