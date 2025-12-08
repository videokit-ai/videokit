/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Editor {

    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEditor;
    using Internal;

    /// <summary>
    /// VideoKit settings for the current Unity project.
    /// </summary>
    [FilePath(@"ProjectSettings/VideoKit.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class VideoKitProjectSettings : ScriptableSingleton<VideoKitProjectSettings> {

        #region --Client API--
        public string accessKey = @"";
        public bool embedAndroidX = true;
        public string photoLibraryUsageDescription = @"Allow this app access the camera roll.";

        public void Save() => Save(false);
        #endregion


        #region --Operations--

        private static readonly Dictionary<RuntimePlatform, string> PlatformToName = new() {
            [RuntimePlatform.LinuxEditor]   = @"linux",
            [RuntimePlatform.OSXEditor]     = @"macos",
            [RuntimePlatform.WindowsEditor] = @"windows",
        };

        [InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(EnterPlayModeOptions options) {            
            try {
                var platform = PlatformToName.GetValueOrDefault(Application.platform);
                var accessKey = instance.accessKey;
                var token = Task.Run(() => VideoKitClient.CreateAuthToken(platform, accessKey)).Result;
                VideoKitClient.Instance = VideoKitClient.Create(token);
            } catch (Exception ex) {
                VideoKitClient.Instance = VideoKitClient.Create(null);
                Debug.LogWarning($"VideoKit: {ex.Message}");
            }
        }
        #endregion
    }
}