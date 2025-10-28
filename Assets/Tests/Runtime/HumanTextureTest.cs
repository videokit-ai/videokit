/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using UnityEngine;
    using Internal;
    using Muna;
    using Newtonsoft.Json;
    using UI;
    using Stopwatch = System.Diagnostics.Stopwatch;

    internal sealed class HumanTextureTest : MonoBehaviour {

        [SerializeField] private VideoKitCameraView cameraView;
        [SerializeField] UnityEngine.UI.RawImage rawImage;
        private Texture2D humanTexture;
        private Muna muna;

        private async void Start() {
            muna = VideoKitClient.Instance.muna;
            await muna.Predictions.Create("@videokit/human-texture");
            Debug.Log("Created predictor");
        }

        private void Update() {
            // Check
            var previewTexture = cameraView.texture;
            if (previewTexture == null)
                return;
            // Check
            //if (!Input.GetKey(KeyCode.Space))
            //    return;
            // Predict
            var watch = Stopwatch.StartNew();
            var prediction = muna.Predictions.Create(
                "@videokit/human-texture",
                inputs: new () {
                    [@"image"] = previewTexture.ToImage()
                }
            ).Result;
            watch.Stop();
            Debug.Log(JsonConvert.SerializeObject(prediction, Formatting.Indented));
            Debug.Log($"Prediction roundtrip latency: {watch.Elapsed.TotalMilliseconds}ms");
            // Update texture
            var result = (Image)prediction.results[0];
            humanTexture = result.ToTexture(humanTexture);
            rawImage.texture = humanTexture;
        }
    }
}