/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Internal {

    using System;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using Function;
    using NatML;
    using NatML.API;
    using NatML.Internal;
    using Status = VideoKit.Status;

    /// <summary>
    /// VideoKit settings.
    /// </summary>
    [DefaultExecutionOrder(-10_000)]
    public sealed class VideoKitSettings : ScriptableObject {

        #region --Client API--
        /// <summary>
        /// VideoKit API client.
        /// </summary>
        public VideoKitClient? client => _client = _client ?? new VideoKitClient(accessKey);

        /// <summary>
        /// VideoKit Function client.
        /// </summary>
        public Function? fxn => _fxn = _fxn ?? (!string.IsNullOrEmpty(accessKey) ? FunctionUnity.Create(accessKey) : null);

        /// <summary>
        /// VideoKit NatML client.
        /// </summary>
        public NatMLClient? natml => _natml ?? (!string.IsNullOrEmpty(accessKey) ? MLUnityExtensions.CreateClient(accessKey) : null);

        /// <summary>
        /// VideoKit settings for this project.
        /// </summary>
        public static VideoKitSettings? Instance { get; internal set; }

        /// <summary>
        /// VideoKit application bundle identifier.
        /// </summary>
        public static string BundleId {
            get {
                var result = new StringBuilder(2048);
                VideoKit.BundleIdentifier(result);
                return result.ToString();
            }
        }

        /// <summary>
        /// Check the application VideoKit session status.
        /// </summary>
        public async Task<Status> CheckSession () {
            // Linux editor
            if (Application.platform == RuntimePlatform.LinuxEditor)
                return Status.NotImplemented;
            // Check
            var current = VideoKit.SessionStatus();
            if (current != Status.InvalidSession)
                return current;
            // Set local token
            if (!string.IsNullOrEmpty(sessionToken)) {
                var status = VideoKit.SetSessionToken(sessionToken);
                if (status != Status.InvalidSession)
                    return status;
            }
            // Set session token
            var token = await client.CreateSessionToken(buildToken, BundleId, ToPlatform(Application.platform));
            var result = VideoKit.SetSessionToken(token);
            // Return
            return result;
        }
        #endregion


        #region --Operations--
        [SerializeField, HideInInspector]
        internal string accessKey = string.Empty;
        [SerializeField, HideInInspector]
        internal string buildToken = string.Empty;
        [SerializeField, HideInInspector]
        internal string sessionToken = string.Empty;
        private VideoKitClient? _client;
        private Function? _fxn;
        private NatMLClient? _natml;
        internal static string FallbackAccessKey => NatMLSettings.Instance?.accessKey;

        private async void Awake () {
            // Set singleton in player
            if (Application.isEditor)
                return;
            // Set singleton
            Instance = this;
            // Check session
            await CheckSession();
        }

        private static string? ToPlatform (RuntimePlatform platform) => platform switch {
            RuntimePlatform.Android         => @"ANDROID",
            RuntimePlatform.IPhonePlayer    => @"IOS",
            RuntimePlatform.LinuxEditor     => @"LINUX",
            RuntimePlatform.LinuxPlayer     => @"LINUX",
            RuntimePlatform.OSXEditor       => @"MACOS",
            RuntimePlatform.OSXPlayer       => @"MACOS",
            RuntimePlatform.WebGLPlayer     => @"WEB",
            RuntimePlatform.WindowsEditor   => @"WINDOWS",
            RuntimePlatform.WindowsPlayer   => @"WINDOWS",
            _                               => null
        };
        #endregion
    }
}