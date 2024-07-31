/* 
*   VideoKit
*   Copyright © 2024 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Editor.Build {

    using System.IO;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;

    #if UNITY_IOS
    using UnityEditor.iOS.Xcode;
    #endif

    internal sealed class iOSBuildHandler : BuildHandler, IPostprocessBuildWithReport {

        protected override BuildTarget target => BuildTarget.iOS;

        void IPostprocessBuildWithReport.OnPostprocessBuild (BuildReport report) {
            // Check platform
            if (report.summary.platform != target)
                return;
            // Add photo library usage description
            AddPhotoLibraryUsageDescription(report);
        }

        private static void AddPhotoLibraryUsageDescription (BuildReport report) {
            #if UNITY_IOS
            var description = VideoKitProjectSettings.instance.PhotoLibraryUsageDescription;
            var outputPath = report.summary.outputPath;
            if (!string.IsNullOrEmpty(description)) {
                // Read plist
                var plistPath = Path.Combine(outputPath, @"Info.plist");
                var plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(plistPath));
                // Add photo library descriptions
                plist.root.SetString(@"NSPhotoLibraryUsageDescription", description);
                plist.root.SetString(@"NSPhotoLibraryAddUsageDescription", description);
                // Write to file
                File.WriteAllText(plistPath, plist.WriteToString());
            }
            #endif
        }
    }
}