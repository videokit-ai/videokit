/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All rights reserved.
*/

namespace VideoKit.Tests {

    using UnityEngine;

    internal sealed class VideoRecordingHandler : MonoBehaviour {

        public void OnRecordingCompleted (MediaAsset asset) {
            Debug.Log(asset.path);
        } 
    }
}