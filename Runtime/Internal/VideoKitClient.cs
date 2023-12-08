/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
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
    using Newtonsoft.Json;

    /// <summary>
    /// VideoKit API client.
    /// </summary>
    public sealed class VideoKitClient {

        #region --Client API--
        /// <summary>
        /// VideoKit client version.
        /// </summary>
        public const string Version = @"0.0.16";

        /// <summary>
        /// VideoKit API URL.
        /// </summary>
        public const string URL = @"https://www.videokit.ai/api";

        /// <summary>
        /// Create the VideoKit client.
        /// </summary>
        /// <param name="accessKey">VideoKit access key.</param>
        /// <param name="url">VideoKit API URL.</param>
        public VideoKitClient (string accessKey, string url = null) {
            this.accessKey = accessKey;
            this.url = url ?? URL;
        }

        /// <summary>
        /// Create a build token.
        /// </summary>
        /// <returns>Build token.</returns>
        public async Task<string?> CreateBuildToken () {
            // Create client
            using var request = new HttpClient();
            request.DefaultRequestHeaders.Authorization = !string.IsNullOrEmpty(accessKey) ? new AuthenticationHeaderValue(@"Bearer", accessKey) : null;
            // Request
            using var response = await request.PostAsync(buildUrl, null);
            var responseStr = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseStr);
            // Check error
            if (result.TryGetValue(@"error", out var error))
                throw new InvalidOperationException(error);
            // Return
            return result?["token"];
        }

        /// <summary>
        /// Create a session token.
        /// </summary>
        /// <returns>Session token.</returns>
        public Task<string?> CreateSessionToken (
            string buildToken,
            string bundleId,
            string platform
        ) {
            var payload = new Dictionary<string, string?> {
                ["build"] = buildToken,
                ["bundle"] = bundleId,
                ["platform"] = platform,
                ["version"] = Version,
            };
            var payloadStr = JsonConvert.SerializeObject(payload);
            return Application.platform == RuntimePlatform.WebGLPlayer ? CreateSessionTokenUnity(payloadStr) : CreateSessionTokenDotNet(payloadStr);
        }
        #endregion


        #region --Operations--
        private readonly string accessKey;
        private readonly string url;
        private string buildUrl => $"{url}/build";
        private string sessionUrl => $"{url}/session";

        private async Task<string?> CreateSessionTokenDotNet (string payload) {
            // Create client
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = !string.IsNullOrEmpty(accessKey) ? new AuthenticationHeaderValue(@"Bearer", accessKey) : null;
            // Request
            using var content = new StringContent(payload, Encoding.UTF8, @"application/json");
            using var response = await client.PostAsync(sessionUrl, content);
            // Parse
            var responseStr = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseStr);
            // Check error
            if (result.TryGetValue(@"error", out var error))
                throw new InvalidOperationException(error);
            // Return
            return result?["token"];
        }

        private async Task<string?> CreateSessionTokenUnity (string payload) {
            // Generate session token
            using var request = new UnityWebRequest(sessionUrl, UnityWebRequest.kHttpVerbPOST) {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload)),
                downloadHandler = new DownloadHandlerBuffer(),
                disposeDownloadHandlerOnDispose = true,
                disposeUploadHandlerOnDispose = true,
            };
            request.SetRequestHeader(@"Content-Type",  @"application/json");
            request.SetRequestHeader(@"Authorization", $"Bearer {accessKey}");
            request.SendWebRequest();
            while (!request.isDone)
                await Task.Yield();
            // Check error
            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
            if (result.TryGetValue(@"error", out var error))
                throw new InvalidOperationException(error);
            // Return
            return result?["token"];
        }
        #endregion
    }
}