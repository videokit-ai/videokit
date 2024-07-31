/* 
*   VideoKit
*   Copyright © 2024 NatML Inc. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Internal {

    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Event manager for hooking into Unity events.
    /// </summary>
    [DefaultExecutionOrder(-9_000)]
    public sealed class VideoKitEvents : MonoBehaviour {

        #region --Client API--
        /// <summary>
        /// Event invoked at the end of each application frame.
        /// </summary>
        public event Action? onFrame;

        /// <summary>
        /// Event invoked on each Unity update.
        /// </summary>
        public event Action? onUpdate;

        /// <summary>
        /// Event invoked on each late Unity update.
        /// </summary>
        public event Action? onLateUpdate;

        /// <summary>
        /// Event invoked when application is quit.
        /// </summary>
        public event Action? onQuit;

        /// <summary>
        /// VideoKit events instance.
        /// </summary>
        public static VideoKitEvents Instance => OptionalInstance = !ReferenceEquals(OptionalInstance, null) ?
            OptionalInstance :
            new GameObject(@"VideoKitEvents").AddComponent<VideoKitEvents>();

        /// <summary>
        /// VideoKit events instance.
        /// </summary>
        public static VideoKitEvents? OptionalInstance { get; private set; }
        #endregion


        #region --Operations--

        private void Awake () => DontDestroyOnLoad(gameObject);

        private IEnumerator Start () {
            var yielder = new WaitForEndOfFrame();
            for (;;) {
                yield return yielder;
                onFrame?.Invoke();
            }
        }

        private void Update () => onUpdate?.Invoke();

        private void LateUpdate () => onLateUpdate?.Invoke();

        private void OnApplicationQuit () {
            onQuit?.Invoke();
            Destroy(gameObject);
            OptionalInstance = null;
        }
        #endregion
    }
}