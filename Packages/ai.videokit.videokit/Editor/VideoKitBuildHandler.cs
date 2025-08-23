/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
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

    internal sealed class VideoKitBuildHandler : IPreprocessBuildWithReport, IPostprocessBuildWithReport {

        #region --Operations--
        private const string CachePath = @"Assets/__VIDEOKIT_DELETE_THIS__";

        [Muna.Muna.Embed(VideoKitCameraManager.HumanTextureTag)]
        private static Muna.Muna muna => new(
            accessKey: VideoKitProjectSettings.instance.accessKey,
            url: VideoKitClient.URL
        );

        private VideoKitClient CreateClient (BuildReport report) {
            try {
                var platform = TargetToPlatform.GetValueOrDefault(report.summary.platform);
                var accessKey = VideoKitProjectSettings.instance.accessKey;
                var token = Task.Run(() => VideoKitClient.CreateToken(platform, accessKey)).Result;
                return VideoKitClient.Create(token: token);
            } catch (Exception ex) {
                Debug.LogWarning($"VideoKit: {ex.Message}");
                return VideoKitClient.Create(token: null); // unauthed
            }
        }

        private void FailureListener () {
            if (BuildPipeline.isBuildingPlayer)
                return;
            ClearSettings();
            EditorApplication.update -= FailureListener;
        }

        int IOrderedCallback.callbackOrder => -1;

        void IPreprocessBuildWithReport.OnPreprocessBuild (BuildReport report) {
            if (report.summary.platform == BuildTarget.Android)
                SetAndroidXImportSettings();
            var client = CreateClient(report);
            EditorApplication.update += FailureListener;
            ClearSettings();
            EmbedClient(client);
        }

        void IPostprocessBuildWithReport.OnPostprocessBuild (BuildReport report) {
            if (
                report.summary.platform == BuildTarget.iOS ||
                report.summary.platform == BuildTarget.VisionOS
            )
                AddPhotoLibraryUsageDescription(report);
        }
        #endregion


        #region --Utilities--

        private static readonly Dictionary<BuildTarget, string> TargetToPlatform = new () {
            [BuildTarget.Android]                   = @"android",
            [BuildTarget.iOS]                       = @"ios",
            [BuildTarget.StandaloneLinux64]         = @"linux",
            [BuildTarget.LinuxHeadlessSimulation]   = @"linux",
            [BuildTarget.EmbeddedLinux]             = @"linux",
            [BuildTarget.StandaloneOSX]             = @"macos",
            [BuildTarget.VisionOS]                  = @"visionos",
            [BuildTarget.WebGL]                     = @"web",
            [BuildTarget.StandaloneWindows]         = @"windows",
            [BuildTarget.StandaloneWindows64]       = @"windows",
        };

        private static void EmbedClient (VideoKitClient client) {
            Directory.CreateDirectory(CachePath);
            AssetDatabase.CreateAsset(client, $"{CachePath}/VideoKit.asset");
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

        private static void SetAndroidXImportSettings () {
            var guids = AssetDatabase.FindAssets(@"videokit-androidx-core");
            if (guids.Length == 0)
                return;
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var importer = PluginImporter.GetAtPath(path) as PluginImporter;
            importer.SetCompatibleWithPlatform(BuildTarget.Android, VideoKitProjectSettings.instance.embedAndroidX);
        }

        private static void AddPhotoLibraryUsageDescription (BuildReport report) {
            var description = VideoKitProjectSettings.instance.photoLibraryUsageDescription;
            var outputPath = report.summary.outputPath;
            if (string.IsNullOrEmpty(description))
                return;
        #if UNITY_IOS || UNITY_VISIONOS
            var plistPath = Path.Combine(outputPath, @"Info.plist");
            var plist = new UnityEditor.iOS.Xcode.PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
            plist.root.SetString(@"NSPhotoLibraryUsageDescription", description);
            plist.root.SetString(@"NSPhotoLibraryAddUsageDescription", description);
            File.WriteAllText(plistPath, plist.WriteToString());
        #endif
        }
        #endregion
    }
}