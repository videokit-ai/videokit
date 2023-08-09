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
            var settings = VideoKitProjectSettings.CreateSettings();
            // Create build token
            try {
                settings.CreateBuildToken();
            } catch (Exception ex) {
                Debug.LogWarning($"VideoKit: {ex.Message}");
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
    }
}