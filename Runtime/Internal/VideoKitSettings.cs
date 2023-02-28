/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.Internal {

    using UnityEngine;

    internal sealed class VideoKitSettings : ScriptableObject {

        #region --Data--
        [SerializeField, HideInInspector]
        internal string token = string.Empty;
        #endregion


        #region --Client API--
        /// <summary>
        /// Get the VideoKit session token.
        /// </summary>
        internal string Token => !string.IsNullOrEmpty(token) ? token : null;

        /// <summary>
        /// VideoKit settings for this project.
        /// </summary>
        public static VideoKitSettings Instance { get; internal set; }
        #endregion


        #region --Operations--
        public const string API = @"ai.natml.videokit";
        public const string Version = @"0.0.11";

        void OnEnable () {
            if (!Application.isEditor)
                Instance = this;
        }
        #endregion
    }
}