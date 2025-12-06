/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using UnityEngine;

    internal sealed class MediaAssetTrimEndTest : MonoBehaviour { // INCOMPLETE

        private async void Start() {
            // Load asset
            var asset = await MediaAsset.FromStreamingAssets(@"rain.mp4");
            Debug.Log($"{asset.width}x{asset.height} @{asset.frameRate}Hz {asset.duration}s");
        }
    }
}