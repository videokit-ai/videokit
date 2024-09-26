/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Editor.Build {
    
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using Internal;

    internal abstract class BuildHandler : IPreprocessBuildWithReport {

        #region --Client API--
        protected abstract BuildTarget target { get; }
        public virtual int callbackOrder => -1;

        protected virtual VideoKitClient CreateClient (BuildReport report) {
            var platform = TargetToPlatform.GetValueOrDefault(report.summary.platform);
            var client = VideoKitClient.Create(
                accessKey: VideoKitProjectSettings.instance.accessKey
            );
            try {
                client.buildToken = Task.Run(() => client.CreateBuildToken(platform: platform)).Result;
            } catch (Exception ex) {
                Debug.LogWarning($"VideoKit: {ex.Message}");
                Debug.LogException(ex);
            }
            return client;
        }
        #endregion


        #region --Operations--
        protected const string CachePath = @"Assets/__VIDEOKIT_DELETE_THIS__";

        [Function.Function.Embed(VideoKitCameraManager.HumanTextureTag)]
        private static Function.Function fxn => new (
            accessKey: VideoKitProjectSettings.instance.accessKey,
            url: "https://www.videokit.ai/api"
        );

        void IPreprocessBuildWithReport.OnPreprocessBuild (BuildReport report) {
            // Check target
            if (report.summary.platform != target)
                return;
            // Create settings
            var client = CreateClient(report);
            // Register failure listener
            EditorApplication.update += FailureListener;
            // Clear settings
            ClearSettings();
            // Embed settings
            EmbedClient(client);
        }

        private void FailureListener () {
            // Check that we're done building
            if (BuildPipeline.isBuildingPlayer)
                return;
            // Clear
            ClearSettings();
            // Stop listening
            EditorApplication.update -= FailureListener;
        }
        #endregion


        #region --Utilities--
        private static readonly Dictionary<BuildTarget, string> TargetToPlatform = new () {
            [BuildTarget.Android]               = @"android",
            [BuildTarget.iOS]                   = @"ios",
            [BuildTarget.StandaloneLinux64]     = @"linux",
            [BuildTarget.StandaloneOSX]         = @"macos",
            [BuildTarget.WebGL]                 = @"web",
            [BuildTarget.StandaloneWindows]     = @"windows",
            [BuildTarget.StandaloneWindows64]   = @"windows",
        };

        private static void EmbedClient (VideoKitClient client) {
            // Create asset
            Directory.CreateDirectory(CachePath);
            AssetDatabase.CreateAsset(client, $"{CachePath}/VideoKit.asset");
            // Add to build
            var assets = PlayerSettings.GetPreloadedAssets()?.ToList() ?? new List<UnityEngine.Object>();
            assets.Add(client);
            PlayerSettings.SetPreloadedAssets(assets.ToArray());
        }

        private static void ClearSettings () {
            var assets = PlayerSettings.GetPreloadedAssets()?.ToList();
            if (assets != null) {
                assets.RemoveAll(asset => asset && asset.GetType() == typeof(VideoKitClient));
                PlayerSettings.SetPreloadedAssets(assets.ToArray());
            }
            AssetDatabase.DeleteAsset(CachePath);
        }
        #endregion
    }
}