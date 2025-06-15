/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Samples {
    using System.Threading.Tasks;
    using UnityEngine;

    public class RecordMicAudio : MonoBehaviour {

        public AudioSource audioSource;

        public async void OnRecordingCompleted (MediaAsset asset) {
            AudioClip clip = await asset.ToAudioClip();
            audioSource.PlayOneShot(clip);
        }
    }
}