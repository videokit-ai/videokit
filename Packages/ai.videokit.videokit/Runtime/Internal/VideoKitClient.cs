/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Internal {

    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;
    using Muna;
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
        public string url = URL;

        /// <summary>
        /// VideoKit Muna client.
        /// </summary>
        public Muna muna => _muna ??= MunaUnity.Create(authToken, url: url);

        /// <summary>
        /// VideoKit client for this project.
        /// </summary>
        public static VideoKitClient? Instance { get; internal set; }

        /// <summary>
        /// VideoKit client version.
        /// </summary>
        public const string Version = @"1.0.8";

        /// <summary>
        /// Check the application VideoKit session status.
        /// </summary>
        public async Task<Status> CheckSession() {
            try {
                // Check if initialized
                if (VideoKit.SetSessionToken(sessionToken) == Status.Ok)
                    return Status.Ok;
                // Create token
                var token = await CreateSessionToken();
                var status = VideoKit.SetSessionToken(token);
                if (status == Status.Ok) {
                    sessionToken = token;
                    PlayerPrefs.SetString(SessionTokenKey, token);
                }
                // Return
                return status;
            } catch (Exception ex) {
                Debug.LogWarning($"VideoKit: Failed to check session with error: {ex.Message}");
                return Status.InvalidOperation;
            }
        }

        /// <summary>
        /// Create a VideoKit client.
        /// </summary>
        /// <param name="token">VideoKit auth token.</param>
        /// <param name="url">VideoKit API URL.</param>
        public static VideoKitClient Create(string? token, string? url = null) {
            var client = CreateInstance<VideoKitClient>();
            client.authToken = token;
            client.url = !string.IsNullOrEmpty(url) ? url : client.url;
            return client;
        }
        #endregion


        #region --Operations--
        [SerializeField, HideInInspector]
        private string? authToken = string.Empty;
        private Muna? _muna;
        private string? sessionToken;
        public const string URL = @"https://www.videokit.ai/api";
        private const string SessionTokenKey = @"ai.videokit.session";

        private void Awake() {
            // Check editor
            if (Application.isEditor)
                return;
            // Set singleton in player
            Instance = Instance ? Instance : this;
            // Set session token
            sessionToken = (
                PlayerPrefs.HasKey(sessionToken) ?
                PlayerPrefs.GetString(sessionToken) :
                null
            );
        }

        internal async static Task<string> CreateAuthToken(
            string platform,
            string apiKey,
            string? url = URL
        ) {
            // Create client
            using var request = new HttpClient();
            request.DefaultRequestHeaders.Authorization = !string.IsNullOrEmpty(apiKey) ?
                new AuthenticationHeaderValue(@"Bearer", apiKey) :
                null;
            // Request
            var version = Marshal.PtrToStringUTF8(VideoKit.GetVersion());
            var payload = new Dictionary<string, object> {
                [@"platform"] = platform,
                [@"version"] = version
            };
            var payloadStr = JsonConvert.SerializeObject(payload);
            using var content = new StringContent(payloadStr, Encoding.UTF8, @"application/json");
            using var response = await request.PostAsync($"{url}/build", content);
            // Parse response
            var responseStr = await response.Content.ReadAsStringAsync();
            Dictionary<string, string> responseBody;
            try {
                responseBody = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseStr)!;
            } catch {
                throw new InvalidOperationException($"Failed to create build token with status {response.StatusCode} and error: {responseStr}");
            }
            // Check error
            if (responseBody.TryGetValue(@"error", out var error))
                throw new InvalidOperationException(error);
            // Return
            return responseBody[@"token"];
        }

        private async Task<string?> CreateSessionToken() {
            // Get session id
            var sessionId = new StringBuilder(2048);
            VideoKit.GetSessionIdentifier(sessionId, sessionId.Capacity);
            // Create payload
            var payload = new Dictionary<string, object?> {
                [@"buildToken"] = authToken,
                [@"sessionId"] = sessionId.ToString(),
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
            request.SendWebRequest();
            while (!request.isDone)
                await Task.Yield();
            // Parse response
            Dictionary<string, string> response;
            try {
                response = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text)!;
            } catch {
                throw new InvalidOperationException($"Failed to create session token with status {request.responseCode} and error: {request.downloadHandler.text}");
            }
            // Check error
            if (response.TryGetValue(@"error", out var error))
                throw new InvalidOperationException(error);
            // Return
            return response["token"];
        }
        #endregion
    }
}