/* 
*   VideoKit
*   Copyright © 2026 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Internal {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;
    using Muna;
    using Newtonsoft.Json;
    using NJsonSchema;
    using NJsonSchema.Generation;
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
        public Muna muna => _muna ??= MunaUnity.Create(authToken, url: $"{url}/muna");

        /// <summary>
        /// VideoKit client for this project.
        /// </summary>
        public static VideoKitClient? Instance { get; internal set; }

        /// <summary>
        /// VideoKit client version.
        /// </summary>
        public const string Version = @"1.0.14";

        /// <summary>
        /// Check the application VideoKit session status.
        /// </summary>
        public async Task<Status> CheckSession() {
            var task = sessionTask;
            if (task == null) {
                task = CheckSessionCore();
                sessionTask = task;
            }
            try {
                return await task;
            } finally {
                if (ReferenceEquals(sessionTask, task))
                    sessionTask = null;
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


        #region --Internal API--

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

        /// <summary>
        /// Get the JSON schema for a given structured output type.
        /// </summary>
        /// <typeparam name="T">Structured output type.</typeparam>
        /// <returns>Serialized JSON schema.</returns>
        internal StructuredOutputSchema? GetSchema<T>() => GetSchema(typeof(T));

        /// <summary>
        /// Get the JSON schema for a given structured output type.
        /// </summary>
        /// <param name="type">Structured output type.</typeparam>
        /// <returns>Serialized JSON schema.</returns>
        internal StructuredOutputSchema? GetSchema(Type type) {
            // Build cache
            schemaMap ??= schemas?.ToDictionary(e => e.key, e => e);
            // Check if cached
            var key = $"{type.FullName}, {type.Assembly.GetName().Name}";
            if (schemaMap?.TryGetValue(key, out var res) ?? false)
                return res;
            // Cache on-demand in the editor
            if (Application.isEditor) {
                var settings = new JsonSchemaGeneratorSettings {
                    GenerateAbstractSchemas         = false,
                    GenerateExamples                = false,
                    UseXmlDocumentation             = false,
                    ResolveExternalXmlDocumentation = false,
                    FlattenInheritanceHierarchy     = false,
                };
                var schema = JsonSchema.FromType(type, settings);
                var result = new StructuredOutputSchema {
                    key = key,
                    schema = schema.ToJson(Formatting.None)
                };
                schemaMap?.Add(key, result);
                return result;
            }
            // Unrecognized type
            return null;
        }
        #endregion


        #region --State--
        [SerializeField, HideInInspector]
        private string? authToken = string.Empty;
        [SerializeField, HideInInspector]
        internal StructuredOutputSchema[] schemas;
        #endregion


        #region --Operations--
        private Muna? _muna;
        private string? sessionToken;
        private Task<Status>? sessionTask;
        private Dictionary<string, StructuredOutputSchema>? schemaMap;
        public const string URL = @"https://www.videokit.ai/api";
        private const string SessionTokenKey = @"ai.videokit.session";
        private const string DeviceLimitErrorCode = @"device_limit_reached";

        private void Awake() {
            // Check editor
            if (Application.isEditor)
                return;
            // Set singleton in player
            Instance = Instance ? Instance : this;
            // Set session token
            sessionToken = (
                PlayerPrefs.HasKey(SessionTokenKey) ?
                PlayerPrefs.GetString(SessionTokenKey) :
                null
            );
        }

        private async Task<string?> CreateSessionToken() {
            // Get device identifier
            var deviceId = new StringBuilder(2048);
            var identifierStatus = VideoKit.GetSessionIdentifier(deviceId, deviceId.Capacity);
            if (identifierStatus != Status.Ok)
                throw new InvalidOperationException($"Failed to get VideoKit device identifier with status {identifierStatus}");
            var version = Marshal.PtrToStringUTF8(VideoKit.GetVersion());
            if (string.IsNullOrEmpty(version))
                throw new InvalidOperationException("Failed to get VideoKit native version");
            // Create payload
            var payload = new Dictionary<string, object?> {
                [@"buildToken"] = authToken,
                [@"deviceId"] = deviceId.ToString(),
                [@"bundleId"] = Application.identifier,
                [@"version"] = version,
            };
            var payloadStr = JsonConvert.SerializeObject(payload);
            // Generate session token
            using var request = new UnityWebRequest($"{url}/session/v4", UnityWebRequest.kHttpVerbPOST) {
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
            return ParseSessionTokenResponse(
                request.downloadHandler.text,
                request.responseCode
            );
        }

        private static string ParseSessionTokenResponse(
            string responseText,
            long responseCode
        ) {
            SessionResponse? response;
            try {
                response = JsonConvert.DeserializeObject<SessionResponse>(responseText);
            } catch {
                throw new InvalidOperationException($"Failed to create session token with status {responseCode} and error: {responseText}");
            }
            if (response == null)
                throw new InvalidOperationException($"Failed to create session token with status {responseCode} and error: {responseText}");
            if (string.Equals(response.code, DeviceLimitErrorCode, StringComparison.Ordinal)) {
                var message = !string.IsNullOrEmpty(response.error) ?
                    response.error :
                    @"VideoKit device limit reached";
                throw new SessionStatusException(Status.DeviceLimitReached, message);
            }
            if (!string.IsNullOrEmpty(response.error))
                throw new InvalidOperationException(response.error);
            if (string.IsNullOrEmpty(response.token))
                throw new InvalidOperationException($"Failed to create session token with status {responseCode} and error: {responseText}");
            return response.token;
        }

        private async Task<Status> CheckSessionCore() {
            var cachedStatus = VideoKit.SetSessionToken(sessionToken);
            if (cachedStatus == Status.Ok)
                return Status.Ok;
            if (cachedStatus == Status.SessionStale) {
                try {
                    var refreshStatus = await RefreshSession(sessionToken);
                    if (refreshStatus == Status.Ok || refreshStatus == Status.SessionStale)
                        return refreshStatus;
                    Debug.LogWarning($"VideoKit: Session refresh returned {refreshStatus}; continuing with the offline session");
                    return Status.SessionStale;
                } catch (Exception ex) {
                    Debug.LogWarning($"VideoKit: Session refresh deferred while offline: {ex.Message}");
                    return Status.SessionStale;
                }
            }
            if (
                !string.IsNullOrEmpty(sessionToken) &&
                (cachedStatus == Status.InvalidSession || cachedStatus == Status.InvalidPlan)
            ) {
                sessionToken = null;
                PlayerPrefs.DeleteKey(SessionTokenKey);
                PlayerPrefs.Save();
            }
            try {
                return await RefreshSession();
            } catch (SessionStatusException ex) {
                Debug.LogWarning($"VideoKit: Failed to check session with error: {ex.Message}");
                return ex.Status;
            } catch (Exception ex) {
                Debug.LogWarning($"VideoKit: Failed to check session with error: {ex.Message}");
                return Status.InvalidOperation;
            }
        }

        private async Task<Status> RefreshSession(string? fallbackToken = null) {
            var token = await CreateSessionToken();
            var status = VideoKit.SetSessionToken(token);
            if (status == Status.Ok || status == Status.SessionStale) {
                sessionToken = token;
                PlayerPrefs.SetString(SessionTokenKey, token);
                PlayerPrefs.Save();
            } else if (!string.IsNullOrEmpty(fallbackToken)) {
                VideoKit.SetSessionToken(fallbackToken);
            }
            return status;
        }
        #endregion


        #region --Types--
        private sealed class SessionResponse {
            public string? token { get; set; }
            public string? error { get; set; }
            public string? code { get; set; }
        }
    
        [Serializable]
        internal struct StructuredOutputSchema {
            /// <summary>
            /// Type key.
            /// </summary>
            public string key;
            /// <summary>
            /// Type schema.
            /// </summary>
            public string schema;
        }
        #endregion
    }

    internal sealed class SessionStatusException : InvalidOperationException {

        public Status Status { get; }

        public SessionStatusException(Status status, string message) : base(message) {
            Status = status;
        }
    }
}