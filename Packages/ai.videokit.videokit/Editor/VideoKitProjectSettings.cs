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
    public sealed class VideoKitProjectSettings : ScriptableSingleton<VideoKitProjectSettings> {

        #region --Data--
        [SerializeField]
        private string accessKey = @"";
        [SerializeField]
        private bool androidx = true;
        [SerializeField]
        private string photoLibraryUsageDescription = @"Allow this app access the camera roll.";
        #endregion


        #region --Client API--
        /// <summary>
        /// VideoKit access key.
        /// </summary>
        internal string AccessKey {
            get => accessKey;
            set {
                if (value != accessKey) {
                    accessKey = value;
                    Save(false);
                }
            }
        }

        /// <summary>
        /// Whether to embed the `androidx` support library in the build.
        /// </summary>
        public bool EmbedAndroidX {
            get => androidx;
            set {
                // Check
                if (value == androidx)
                    return;
                // Set
                androidx = value;
                Save(false);
            }
        }

        /// <summary>
        /// Photo library usage description presented to the user when sharing a media asset.
        /// This only applies on iOS.
        /// </summary>
        public string PhotoLibraryUsageDescription {
            get => photoLibraryUsageDescription;
            set {
                // Check
                if (value == photoLibraryUsageDescription)
                    return;
                // Set
                photoLibraryUsageDescription = value;
                Save(false);
            }
        }

        /// <summary>
        /// Create VideoKit settings from the current project settings.
        /// </summary>
        internal static VideoKitClient CreateClient () => VideoKitClient.Create(instance.AccessKey);
        #endregion


        #region --Operations--

        [InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor (EnterPlayModeOptions options) {
            var client = CreateClient();
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