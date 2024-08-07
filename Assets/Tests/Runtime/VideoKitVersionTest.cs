/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All rights reserved.
*/

namespace VideoKit.Tests {

    using System.Runtime.InteropServices;
    using UnityEngine;
    using Internal;

    internal sealed class VideoKitVersionTest : MonoBehaviour {

        private void Start () {
            var version = Marshal.PtrToStringUTF8(VideoKit.GetVersion());
            Debug.Log($"VideoKit {version}");
        }
    }
}