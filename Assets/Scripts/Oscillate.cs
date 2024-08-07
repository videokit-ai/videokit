/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests.Behaviours {

    using UnityEngine;

    public class Oscillate : MonoBehaviour {

        private void Update () => transform.position = Vector3.up * Mathf.Sin(2f * Time.time);
    }
}