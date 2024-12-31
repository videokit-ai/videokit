/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using UnityEngine;

    internal sealed class MediaAssetFromSpeechPromptTest : MonoBehaviour {

        [SerializeField, TextArea]
        private string prompt;

        private async void Start () {
            var audioAsset = await MediaAsset.FromGeneratedSpeech(prompt);
            var audioClip = await audioAsset.ToAudioClip();
            AudioSource.PlayClipAtPoint(audioClip, Vector3.zero);
        }
    }
}