/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Internal {

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;
    using Function;
    using NatML.Internal;
    using Newtonsoft.Json;
    using Status = VideoKit.Status;

    /// <summary>
    /// VideoKit settings.
    /// </summary>
    [DefaultExecutionOrder(-10_000)]
    public sealed class VideoKitSettings : ScriptableObject {

        #region --Client API--
        /// <summary>
        /// VideoKit settings for this project.
        /// </summary>
        public static VideoKitSettings? Instance { get; internal set; }

        /// <summary>
        /// VideoKit Function client.
        /// </summary>
        public Function? Function => !string.IsNullOrEmpty(authToken) ? FunctionUnity.Create(authToken) : null;

        /// <summary>
        /// Check the application VideoKit session status.
        /// </summary>
        public async Task<Status> CheckSession () {
            // Check
            var current = VideoKit.SessionStatus();
            if (current != Status.InvalidSession)
                return current;
            // Set session token
            var token = await CreateSessionToken();
            var status = VideoKit.SetSessionToken(token);
            // Return
            return status;
        }
        #endregion


        #region --Operations--
        [SerializeField, HideInInspector]
        internal string licenseKey = string.Empty;
        [SerializeField, HideInInspector]
        internal string buildToken = string.Empty;
        private string authToken => !string.IsNullOrEmpty(licenseKey) ? licenseKey : NatMLSettings.Instance?.accessKey;
        public const string Version = @"0.0.14";
        private const string API = @"https://www.videokit.ai/api";

        private async void Awake () {
            // Set singleton in player
            if (!Application.isEditor)
                Instance = this;
            // Check session
            await CheckSession();
        }

        internal void CreateBuildToken () {
            using var request = new UnityWebRequest($"{API}/build", UnityWebRequest.kHttpVerbPOST);
            request.SetRequestHeader(@"Content-Type",  @"application/json");
            request.SetRequestHeader(@"Authorization", $"Bearer {authToken}");
            request.SendWebRequest();
            while (!request.isDone) ;
            // Check error
            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
            if (result.TryGetValue("error", out var error))
                throw new InvalidOperationException(error);
            // Get token
            buildToken = result?["token"];
        }

        private async Task<string?> CreateSessionToken () {
            await Task.Yield();
            // Check access key
            if (string.IsNullOrEmpty(authToken))
                return null;
            // Get bundle ID
            var bundleId = new StringBuilder(2048);
            VideoKit.BundleIdentifier(bundleId);
            // Generate session token
            var body = new Dictionary<string, object?> {
                ["version"] = Version,
                ["platform"] = GetPlatform(),
                ["bundle"] = bundleId.ToString(),
                ["build"] = buildToken,
            };
            var jsonBody = JsonConvert.SerializeObject(body);
            using var request = new UnityWebRequest($"{API}/session", UnityWebRequest.kHttpVerbPOST) {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody)),
                downloadHandler = new DownloadHandlerBuffer(),
                disposeDownloadHandlerOnDispose = true,
                disposeUploadHandlerOnDispose = true,
            };
            request.SetRequestHeader(@"Content-Type",  @"application/json");
            request.SetRequestHeader(@"Authorization", $"Bearer {authToken}");
            request.SendWebRequest();
            while (!request.isDone)
                await Task.Yield();
            // Check error
            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
            if (result.TryGetValue("error", out var error))
                throw new InvalidOperationException(error);
            // Return
            return result?["token"];
        }

        private static string? GetPlatform () => Application.platform switch {
            RuntimePlatform.Android         => "ANDROID",
            RuntimePlatform.IPhonePlayer    => "IOS",
            RuntimePlatform.OSXEditor       => "MACOS",
            RuntimePlatform.OSXPlayer       => "MACOS",
            RuntimePlatform.WebGLPlayer     => "WEB",
            RuntimePlatform.WindowsEditor   => "WINDOWS",
            RuntimePlatform.WindowsPlayer   => "WINDOWS",
            _                               => null
        };
        #endregion
    }
}