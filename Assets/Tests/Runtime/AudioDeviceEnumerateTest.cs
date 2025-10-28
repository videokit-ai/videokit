/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using System.Collections.Generic;
    using UnityEngine;
    using Newtonsoft.Json;

    internal sealed class AudioDeviceEnumerateTest : MonoBehaviour {

        private async void Start() {
            await AudioDevice.CheckPermissions();
            var audioDevices = await AudioDevice.Discover();
            foreach (var device in audioDevices)
                Debug.Log(JsonConvert.SerializeObject(new Dictionary<string, object> {
                    ["uniqueID"] = device.uniqueId,
                    ["name"] = device.name,
                    ["location"] = device.location,
                    ["defaultForMediaType"] = device.defaultForMediaType,
                    ["echoCancellationSupported"] = device.echoCancellationSupported,
                    ["echoCancellation"] = device.echoCancellation,
                    ["sampleRate"] = device.sampleRate,
                    ["channelCount"] = device.channelCount,
                    ["running"] = device.running
                }, Formatting.Indented));
        }
    }
}