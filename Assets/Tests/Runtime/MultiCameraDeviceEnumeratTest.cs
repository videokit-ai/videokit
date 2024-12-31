/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using System.Collections.Generic;
    using UnityEngine;
    using Newtonsoft.Json;

    internal sealed class MultiCameraDeviceEnumerateTest : MonoBehaviour {

        private async void Start () {
            await MultiCameraDevice.CheckPermissions();
            var multiCameraDevices = await MultiCameraDevice.Discover();
            foreach (var device in multiCameraDevices)
                Debug.Log(JsonConvert.SerializeObject(new Dictionary<string, object> {
                    // Identifiers
                    [@"uniqueID"] = device.uniqueId,
                    [@"name"] = device.name,
                    [@"location"] = device.location,
                    [@"defaultForMediaType"] = device.defaultForMediaType,
                    // Streaming
                    [@"running"] = device.running,
                }, Formatting.Indented));
        }
    }
}