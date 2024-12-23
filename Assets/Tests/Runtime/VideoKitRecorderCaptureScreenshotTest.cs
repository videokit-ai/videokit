/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using UnityEngine;

    internal sealed class VideoKitRecorderCaptureScreenshotTest : MonoBehaviour {

        [SerializeField] private VideoKitRecorder recorder;

        private async void Start () {
            await Awaitable.WaitForSecondsAsync(2f);
            var asset = await recorder.CaptureScreenshot();
            Debug.Log($"Captured screenshot to path: {asset.path}");
        }
    }
}