/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Editor {

    using System.Collections.Generic;
    using UnityEditor;

    internal static class VideoKitSettingsProvider {
        
        [SettingsProvider]
        public static SettingsProvider CreateProvider() => new SettingsProvider(@"Project/VideoKit", SettingsScope.Project) {
            label = @"VideoKit",
            guiHandler = searchContext => {
                var settings = VideoKitProjectSettings.instance;
                EditorGUI.BeginChangeCheck();
                // Account
                EditorGUILayout.LabelField(@"VideoKit Account", EditorStyles.boldLabel);
                settings.accessKey = EditorGUILayout.TextField(@"Access Key", settings.accessKey);
                EditorGUILayout.Space(10);
                // Android settings
                EditorGUILayout.LabelField(@"Android Settings", EditorStyles.boldLabel);
                settings.embedAndroidX= EditorGUILayout.Toggle(@"Embed AndroidX Library", settings.embedAndroidX);
                EditorGUILayout.Space(10);
                // iOS settings
                EditorGUILayout.LabelField(@"iOS Settings", EditorStyles.boldLabel);
                settings.photoLibraryUsageDescription = EditorGUILayout.TextField(@"Photo Library Usage Description", settings.photoLibraryUsageDescription);
                // Save
                if (EditorGUI.EndChangeCheck())
                    settings.Save();
            },
            keywords = new HashSet<string>(new[] { @"VideoKit", @"NatCorder", @"NatDevice", @"NatShare" }),
        };
    }
}