/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Editor {

    using System.Collections.Generic;
    using UnityEditor;

    internal static class VideoKitSettingsProvider {
        
        [SettingsProvider]
        public static SettingsProvider CreateProvider () => new SettingsProvider(@"Project/VideoKit", SettingsScope.Project) {
            label = @"VideoKit",
            guiHandler = searchContext => {
                // License
                EditorGUILayout.LabelField(@"VideoKit Account", EditorStyles.boldLabel);
                VideoKitProjectSettings.instance.AccessKey = EditorGUILayout.TextField(@"Access Key", VideoKitProjectSettings.instance.AccessKey);
                EditorGUILayout.Space(10);
                // Android settings
                EditorGUILayout.LabelField(@"Android Settings", EditorStyles.boldLabel);
                VideoKitProjectSettings.instance.EmbedAndroidX = EditorGUILayout.Toggle(@"Embed AndroidX Library", VideoKitProjectSettings.instance.EmbedAndroidX);
                EditorGUILayout.Space(10);
                // iOS settings
                EditorGUILayout.LabelField(@"iOS Settings", EditorStyles.boldLabel);
                VideoKitProjectSettings.instance.PhotoLibraryUsageDescription = EditorGUILayout.TextField(@"Photo Library Usage Description", VideoKitProjectSettings.instance.PhotoLibraryUsageDescription);
            },
            keywords = new HashSet<string>(new[] { @"VideoKit", @"NatML", @"NatCorder", @"NatDevice", @"NatShare", @"Hub" }),
        };
    }
}