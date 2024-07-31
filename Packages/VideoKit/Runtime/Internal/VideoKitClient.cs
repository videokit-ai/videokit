/* 
*   VideoKit
*   Copyright © 2024 NatML Inc. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Internal {

    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;
    using Function;
    using Newtonsoft.Json;
    using Status = VideoKit.Status;

    /// <summary>
    /// VideoKit API client.
    /// </summary>
    [DefaultExecutionOrder(-10_000)]
    public sealed class VideoKitClient : ScriptableObject {

        #region --Client API--
        /// <summary>
        /// VideoKit API URL.
        /// </summary>
        [SerializeField, HideInInspector]
        public string url = @"https://www.videokit.ai/api";

        /// <summary>
        /// VideoKit Function client.
        /// </summary>
        public Function fxn => _fxn ??= FunctionUnity.Create(buildToken, url: url);

        /// <summary>
        /// VideoKit client for this project.
        /// </summary>
        public static VideoKitClient? Instance { get; internal set; }

        /// <summary>
        /// VideoKit client version.
        /// </summary>
        public const string Version = @"0.0.20";

        /// <summary>
        /// Check the application VideoKit session status.
        /// </summary>
        public async Task<Status> CheckSession () {
            // Linux editor
            if (Application.platform == RuntimePlatform.LinuxEditor)
                return Status.NotImplemented;
            // Set
            buildToken ??= await CreateBuildToken(); // can only run if client is not serialized
            var token = sessionToken ?? await CreateSessionToken();
            var result = VideoKit.SetSessionToken(token);
            // Cache
            if (result.IsOk()) {
                PlayerPrefs.SetString(BuildTokenKey, buildToken);
                PlayerPrefs.SetString(SessionTokenKey, token);
            }
            // Return
            return result;
        }

        /// <summary>
        /// Create a VideoKit client.
        /// </summary>
        /// <param name="accessKey">VideoKit access key.</param>
        /// <param name="url">VideoKit API URL.</param>
        public static VideoKitClient Create (string accessKey, string? url = null) {
            var client = CreateInstance<VideoKitClient>();
            client.accessKey = accessKey;
            client.url = !string.IsNullOrEmpty(url) ? url : client.url;
            return client;
        }
        #endregion


        #region --Operations--
        [SerializeField, HideInInspector]
        internal string accessKey = string.Empty;
        [SerializeField, HideInInspector]
        internal string buildToken = string.Empty;
        private Function? _fxn;
        private static readonly Dictionary<RuntimePlatform, string> PlatformToName = new () {
            [RuntimePlatform.Android]       = @"android",
            [RuntimePlatform.IPhonePlayer]  = @"ios",
            [RuntimePlatform.LinuxEditor]   = @"linux",
            [RuntimePlatform.LinuxPlayer]   = @"linux",
            [RuntimePlatform.OSXEditor]     = @"macos",
            [RuntimePlatform.OSXPlayer]     = @"macos",
            [RuntimePlatform.WebGLPlayer]   = @"web",
            [RuntimePlatform.WindowsEditor] = @"windows",
            [RuntimePlatform.WindowsPlayer] = @"windows",
        };
        private const string BuildTokenKey = @"ai.videokit.build";
        private const string SessionTokenKey = @"ai.videokit.session";

        private string? sessionToken => (
            !Application.isEditor &&
            PlayerPrefs.HasKey(BuildTokenKey) &&
            PlayerPrefs.GetString(BuildTokenKey) == buildToken &&
            !string.IsNullOrEmpty(PlayerPrefs.GetString(SessionTokenKey))
        ) ? PlayerPrefs.GetString(SessionTokenKey) : null;

        private void Awake () {
            // Set singleton in player
            if (!Application.isEditor && Instance == null)
                Instance = this;
        }

        internal async Task<string> CreateBuildToken (string? platform = null) {
            // Create client
            using var request = new HttpClient();
            request.DefaultRequestHeaders.Authorization = !string.IsNullOrEmpty(accessKey) ?
                new AuthenticationHeaderValue(@"Bearer", accessKey) :
                null;
            // Request
            var payload = new Dictionary<string, object> {
                [@"platform"] = platform ?? PlatformToName.GetValueOrDefault(Application.platform),
                [@"version"] = Version
            };
            var payloadStr = JsonConvert.SerializeObject(payload);
            using var content = new StringContent(payloadStr, Encoding.UTF8, @"application/json");
            using var response = await request.PostAsync($"{url}/build", content);
            var responseStr = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseStr) ??
                throw new InvalidOperationException($"Failed to create build token with status: {response.StatusCode}");
            // Check error
            if (result.TryGetValue(@"error", out var error))
                throw new InvalidOperationException(error);
            // Set
            buildToken = result[@"token"];
            // Return
            return buildToken;
        }

        private async Task<string?> CreateSessionToken () {
            // Get session id
            var sessionId = new StringBuilder(2048);
            VideoKit.GetSessionIdentifier(sessionId, sessionId.Capacity);
            // Create payload
            var payload = new Dictionary<string, object> {
                [@"sessionId"] = sessionId.ToString()
            };
            var payloadStr = JsonConvert.SerializeObject(payload);
            // Generate session token
            using var request = new UnityWebRequest($"{url}/session/v3", UnityWebRequest.kHttpVerbPOST) {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payloadStr)),
                downloadHandler = new DownloadHandlerBuffer(),
                disposeDownloadHandlerOnDispose = true,
                disposeUploadHandlerOnDispose = true,
                timeout = 20,
            };
            request.SetRequestHeader(@"Content-Type",  @"application/json");
            request.SetRequestHeader(@"Authorization", $"Bearer {buildToken}");
            request.SendWebRequest();
            while (!request.isDone)
                await Task.Yield();
            // Check error
            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text) ??
                throw new InvalidOperationException($"Failed to create session token with status {request.responseCode} and error: {request.error}");
            if (result.TryGetValue(@"error", out var error))
                throw new InvalidOperationException(error);
            // Return
            return result["token"];
        }
        #endregion
    }
}