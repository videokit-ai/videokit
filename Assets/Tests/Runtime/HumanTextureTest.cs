/* 
*   VideoKit
*   Copyright © 2024 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using UnityEngine;
    using Internal;
    using Function;
    using Function.Types;
    using Newtonsoft.Json;
    using Stopwatch = System.Diagnostics.Stopwatch;

    internal sealed class HumanTextureTest : MonoBehaviour {

        [SerializeField] private VideoKitCameraManager cameraManager;
        [SerializeField] UnityEngine.UI.RawImage rawImage;
        private Texture2D humanTexture;
        private Function fxn;

        private async void Start () {
            fxn = VideoKitClient.Instance.fxn;
            await fxn.Predictions.Create("@videokit/human-texture");
            Debug.Log("Created predictor");
        }

        private void Update () {
            // Check
            var previewTexture = cameraManager.texture;
            if (previewTexture == null)
                return;
            // Check
            //if (!Input.GetKey(KeyCode.Space))
            //    return;
            // Predict
            var watch = Stopwatch.StartNew();
            var prediction = fxn.Predictions.Create(
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