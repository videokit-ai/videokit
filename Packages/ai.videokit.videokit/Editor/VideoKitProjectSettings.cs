/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Editor {

    using System;
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

        public void Save () => Save(false);
        #endregion


        #region --Operations--

        [InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor (EnterPlayModeOptions options) {
            var client = VideoKitClient.Create(instance.accessKey);
            try {
                client.buildToken = Task.Run(() => client.CreateBuildToken()).Result;
            } catch (Exception ex) {
                Debug.LogWarning($"VideoKit: {ex.Message}");
                Debug.LogException(ex);
            }
            VideoKitClient.Instance = client;
        }
        #endregion
    }
}