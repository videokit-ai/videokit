/*
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Utilities {

    using System;
    using UnityEngine;

    [DefaultExecutionOrder(-1000)]
    internal sealed class LifecycleHelper : MonoBehaviour, IDisposable {

        #region --Client API--
        /// <summary>
        /// Event invoked when Unity's `Update` message is invoked.
        /// </summary>
        public event Action onUpdate;

        /// <summary>
        /// Event invoked when the app is paused or resumed.
        /// </summary>
        public event Action<bool> onPause;

        /// <summary>
        /// Event invoked when the app is exiting.
        /// </summary>
        public event Action onQuit;

        /// <summary>
        /// Create a lifecycle helper.
        /// </summary>
        public static LifecycleHelper Create () => new GameObject(@"VideoKit Helper").AddComponent<LifecycleHelper>();

        /// <summary>
        /// Dispose the helper.
        /// </summary>
        public void Dispose () {
            onPause = null;
            onQuit = null;
            Destroy(gameObject);
            DestroyImmediate(this);
        }
        #endregion


        #region --Operations--

        private void Awake () => DontDestroyOnLoad(gameObject);

        private void Update () => onUpdate?.Invoke();

        private void OnApplicationPause (bool pause) => onPause?.Invoke(pause);

        private void OnApplicationQuit () => onQuit?.Invoke();
        #endregion
    }
}