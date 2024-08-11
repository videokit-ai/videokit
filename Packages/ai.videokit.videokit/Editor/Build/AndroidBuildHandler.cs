/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Editor.Build {

    using UnityEditor;
    using UnityEditor.Build.Reporting;
    using Internal;

    internal sealed class AndroidBuildHandler : BuildHandler {

        protected override BuildTarget target => BuildTarget.Android;

        protected override VideoKitClient CreateClient(BuildReport report) {
            // Set androidx import settings
            SetAndroidXImportSettings();
            // Create client
            return base.CreateClient(report);
        }

        private static void SetAndroidXImportSettings () {
            // Find GUID
            var guids = AssetDatabase.FindAssets(@"videokit-androidx-core");
            if (guids.Length == 0)
                return;
            // Update importer
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var importer = PluginImporter.GetAtPath(path) as PluginImporter;
            importer.SetCompatibleWithPlatform(BuildTarget.Android, VideoKitProjectSettings.instance.EmbedAndroidX);
        }
    }
}