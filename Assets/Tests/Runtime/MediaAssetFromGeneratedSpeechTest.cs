/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using UnityEngine;

    internal sealed class MediaAssetFromGeneratedSpeechTest : MonoBehaviour {

        [SerializeField, TextArea]
        private string prompt;

        private async void Start() {
            var asset = await MediaAsset.FromGeneratedSpeech(
                prompt,
                voice: MediaAsset.NarrationVoice.Kevin
            );
            var clip = await asset.ToAudioClip();
            AudioSource.PlayClipAtPoint(clip, Vector3.zero);
        }
    }
}