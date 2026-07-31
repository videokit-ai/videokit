/* 
*   VideoKit
*   Copyright © 2026 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using UnityEngine;
    using Internal;
    using Muna;
    using Newtonsoft.Json;
    using UI;
    using Models = Internal.VideoKitModels;
    using Stopwatch = System.Diagnostics.Stopwatch;

    internal sealed class HumanTextureTest : MonoBehaviour {

        [SerializeField] private VideoKitCameraView cameraView;
        [SerializeField] UnityEngine.UI.RawImage rawImage;
        private Texture2D humanTexture;
        private Muna muna;
        private bool ready;

        private async void Start() {
            // Preload model
            muna = VideoKitClient.Instance.muna;
            await muna.Predictions.Create(
                tag: Models.HumanTexture_v2,
                inputs: new() { [@""] = null }
            );
            ready = true;
            Debug.Log("Created predictor");
        }

        private void Update() {
            // Check
            if (!ready)
                return;
            // Check
            var previewTexture = cameraView.texture;
            if (previewTexture == null)
                return;
            // Predict
            var watch = Stopwatch.StartNew();
            var prediction = muna.Predictions.Create(
                tag: Models.HumanTexture_v2,
                inputs: new () {
                    [@"image"] = previewTexture.ToImage()
                }
            ).Result;
            watch.Stop();
            Debug.Log(JsonConvert.SerializeObject(prediction, Formatting.Indented));
            Debug.Log($"Prediction roundtrip latency: {watch.Elapsed.TotalMilliseconds}ms");
            // Update texture
            var result = (Image)prediction.results[0];
            result.CopyTo(humanTexture);
            rawImage.texture = humanTexture;
        }
    }
}