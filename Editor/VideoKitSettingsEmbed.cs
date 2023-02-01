/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.Editor {

    using System;
    using System.IO;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEditor.Build.Reporting;
    using UnityEngine;
    using Hub;
    using Hub.Editor;
    using Hub.Internal;
    using Hub.Requests;
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
        private const string CachePath = @"Assets/NMLBuildCache";

        protected override VideoKitSettings[] CreateEmbeds (BuildReport report) {
            var platform = ToPlatform(report.summary.platform);
            var bundle = BundleOverride?.identifier ?? NatMLHub.GetAppBundle(platform);
            var accessKey = HubSettings.Instance.AccessKey;
            var settings = VideoKitProjectSettings.CurrentSettings;
            try {
                settings = VideoKitProjectSettings.CreateSettings(platform, bundle, accessKey);
            } catch(Exception ex) {
                Debug.LogWarning($"VideoKit Error: {ex.InnerException.Message}");
            }
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