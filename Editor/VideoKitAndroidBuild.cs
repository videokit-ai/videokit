/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Editor {

    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using Internal;

    internal sealed class VideoKitAndroidBuild : IPreprocessBuildWithReport {

        int IOrderedCallback.callbackOrder => 0;

        void IPreprocessBuildWithReport.OnPreprocessBuild (BuildReport report) {
            // Check
            if (report.summary.platform != BuildTarget.Android)
                return;
            // Retrieve
            var guids = AssetDatabase.FindAssets("videokit-androidx-core");
            if (guids.Length == 0)
                return;
            // Update importer
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var importer = PluginImporter.GetAtPath(path) as PluginImporter;
            importer.SetCompatibleWithPlatform(BuildTarget.Android, VideoKitProjectSettings.instance.EmbedAndroidX);
        }
    }
}