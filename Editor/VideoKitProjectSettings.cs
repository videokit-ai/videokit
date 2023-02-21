/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.Editor {

    using System;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEditor;
    using Hub;
    using Hub.Internal;
    using Hub.Requests;
    using Internal;

    [FilePath(@"ProjectSettings/VideoKit.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class VideoKitProjectSettings : ScriptableSingleton<VideoKitProjectSettings> {

        #region --Client API--
        /// <summary>
        /// VideoKit settings from the current project settings.
        /// </summary>
        internal static VideoKitSettings CurrentSettings {
            get {
                var settings = ScriptableObject.CreateInstance<VideoKitSettings>();
                settings.token = SessionState.GetString(tokenKey, string.Empty);
                return settings;
            }
        }

        /// <summary>
        /// Create VideoKit settings.
        /// </summary>
        /// <param name="platform">NatML platform identifier.</param>
        /// <param name="bundle">NatML app bundle.</param>
        /// <param name="accessKey">NatML access key.</param>
        internal static VideoKitSettings CreateSettings (string platform, string bundle, string accessKey) {
            var input = new CreateMediaSessionRequest.Input {
                api = VideoKitSettings.API,
                version = VideoKitSettings.Version,
                platform = platform,
                bundle = bundle
            };
            var session = Task.Run(() => NatMLHub.CreateMediaSession(input, accessKey)).Result;
            var settings = ScriptableObject.CreateInstance<VideoKitSettings>();
            settings.token = session.token;
            return settings;
        }
        #endregion


        #region --Operations--
        private static string tokenKey => $"{VideoKitSettings.API}.token";

        [InitializeOnLoadMethod]
        private static void OnLoad () {
            VideoKitSettings.Instance = CurrentSettings;
            HubSettings.OnUpdateSettings += OnUpdateHubSettings;
        }

        private static void OnUpdateHubSettings (HubSettings hubSettings) {
            try {
                var settings = CreateSettings(NatMLHub.CurrentPlatform, NatMLHub.GetEditorBundle(), hubSettings.AccessKey);
                SessionState.SetString(tokenKey, settings.token);
            } catch (Exception ex) {
                SessionState.EraseString(tokenKey);
                Debug.LogWarning($"VideoKit Error: {ex.InnerException.Message}");
            }
            VideoKitSettings.Instance = CurrentSettings;
        }
        #endregion
    }
}