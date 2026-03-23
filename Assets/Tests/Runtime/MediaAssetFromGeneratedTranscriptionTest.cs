/* 
*   VideoKit
*   Copyright © 2026 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using UnityEngine;

    internal sealed class MediaAssetFromGeneratedTranscriptionTest : MonoBehaviour {

        [SerializeField]
        private AudioClip clip;

        private async void Start() {
            var asset = await MediaAsset.FromGeneratedTranscription(clip);
            var text = asset.ToText();
            Debug.Log(text);
        }
    }
}