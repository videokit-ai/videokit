/* 
*   VideoKit
*   Copyright © 2026 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using System.Runtime.Serialization;
    using UnityEngine;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    internal sealed class MediaAssetParseTest : MonoBehaviour {

        enum Direction {
            [EnumMember(Value = @"North")]
            North,
            [EnumMember(Value = @"East")]
            East,
            [EnumMember(Value = @"West")]
            West,
            [EnumMember(Value = @"South")]
            South    
        }

        [StructuredOutput]
        struct Command {
            public string name;
            [JsonConverter(typeof(StringEnumConverter))]
            public Direction direction;
        }

        private async void Start() {
            var asset = await MediaAsset.FromText(@"My name is Yusuf and I'm heading South");
            var command = await asset.Parse<Command>();
            Debug.Log(JsonConvert.SerializeObject(command, Formatting.Indented));
        }
    }
}