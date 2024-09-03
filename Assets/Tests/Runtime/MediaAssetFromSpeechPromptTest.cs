/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using UnityEngine;
    using UnityEngine.Serialization;

    internal sealed class MediaAssetFromSpeechPromptTest : MonoBehaviour {

        [SerializeField, TextArea, FormerlySerializedAs(@"text")]
        private string prompt;

        private async void Start () {
            var audioAsset = await MediaAsset.FromGeneratedSpeech(prompt);
            var audioClip = await audioAsset.ToAudioClip();
            AudioSource.PlayClipAtPoint(audioClip, Vector3.zero);
        }
    }
}