/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Editor {

    using System;
    using System.IO;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEditor.Build.Reporting;
    using UnityEngine;
    using NatML.Editor;
    using Internal;

    internal sealed class VideoKitSettingsEmbed : BuildEmbedHelper<VideoKitSettings> {

        protected override BuildTarget[] SupportedTargets => new [] {
            BuildTarget.Android,
            BuildTarget.iOS,
            BuildTarget.StandaloneOSX,
            BuildTarget.StandaloneWindows,
            BuildTarget.StandaloneWindows64,
            BuildTarget.WebGL,
        };
        private const string CachePath = @"Assets/__VIDEOKIT_DELETE_THIS__";

        protected override VideoKitSettings[] CreateEmbeds (BuildReport report) {
            var platform = ToPlatform(report.summary.platform);
            var bundleId = Application.identifier;
            var settings = VideoKitProjectSettings.CreateSettings();
            var client = new VideoKitClient(settings.accessKey);
            // Create build token
            try {
                settings.buildToken = Task.Run(() => client.CreateBuildToken()).Result;
                settings.sessionToken = Task.Run(() => client.CreateSessionToken(settings.buildToken, bundleId, platform)).Result;
            } catch (Exception ex) {
                Debug.LogWarning($"VideoKit: {ex.Message}");
                Debug.LogException(ex);
            }
            // Embed
            Directory.CreateDirectory(CachePath);
            AssetDatabase.CreateAsset(settings, $"{CachePath}/VideoKit.asset");
            return new [] { settings };
        }

        protected override void ClearEmbeds (BuildReport report) {
            base.ClearEmbeds(report);
            AssetDatabase.DeleteAsset(CachePath);
        }

        private static string ToPlatform (BuildTarget target) => target switch {
            BuildTarget.Android             => "ANDROID",
            BuildTarget.iOS                 => "IOS",
            BuildTarget.StandaloneLinux64   => "LINUX",
            BuildTarget.StandaloneOSX       => "MACOS",
            BuildTarget.StandaloneWindows   => "WINDOWS",
            BuildTarget.StandaloneWindows64 => "WINDOWS",
            BuildTarget.WebGL               => "WEB",
            _                               => null,
        };
    }
}