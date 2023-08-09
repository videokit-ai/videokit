/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Editor {

    using System.IO;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using Internal;

    #if UNITY_IOS
    using UnityEditor.iOS.Xcode;
    #endif

    internal sealed class VideoKitiOSBuild : IPostprocessBuildWithReport {

        int IOrderedCallback.callbackOrder => 0;

        void IPostprocessBuildWithReport.OnPostprocessBuild (BuildReport report) {
            // Check
            if (report.summary.platform != BuildTarget.iOS)
                return;
            // Add photo library usage description
            #if UNITY_IOS
            var description = VideoKitProjectSettings.instance.PhotoLibraryUsageDescription;
            if (!string.IsNullOrEmpty(description)) {
                // Read plist
                var plistPath = Path.Combine(report.summary.outputPath, @"Info.plist");
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