/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.UI;

    internal sealed class VideoKitRecorderCaptureScreenshotTest : MonoBehaviour {

        [SerializeField] private VideoKitRecorder recorder;

        [Header(@"UI")]
        [SerializeField] private RawImage rawImage;

        private async void Start () {
            await Task.Delay(3_000);
            var imageAsset = await recorder.CaptureScreenshot();
            rawImage.texture = await imageAsset.ToTexture();
        }
    }
}