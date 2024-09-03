/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using UnityEngine;
    using Newtonsoft.Json;

    internal sealed class MediaAssetCaptionToStructureTest : MonoBehaviour {

        enum Direction {
            North,
            East,
            West,
            South    
        }
        
        struct Command {
            [JsonProperty(Required = Required.Always)]
            public string name;
            [JsonProperty(Required = Required.Always)]
            public Direction direction;
        }

        private async void Start () {
            var asset = await MediaAsset.FromText(@"My name is Yusuf and I'm heading East");
            var command = await asset.Parse<Command>();
            Debug.Log(JsonConvert.SerializeObject(command, Formatting.Indented));
        }
    }
}