/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Editor {

    using UnityEditor;
    using Internal;

    internal static class VideoKitMenu {

        private const int BasePriority = 600;
        
        [MenuItem(@"VideoKit/VideoKit " + VideoKitClient.Version, false, BasePriority)]
        private static void Version () { }

        [MenuItem(@"VideoKit/VideoKit " + VideoKitClient.Version, true, BasePriority)]
        private static bool DisableVersion () => false;

        [MenuItem(@"VideoKit/Get Access Key", false, BasePriority + 1)]
        private static void GetAccessKey () => Help.BrowseURL(@"https://videokit.ai?account");

        [MenuItem(@"VideoKit/View the Docs", false, BasePriority + 2)]
        private static void OpenDocs () => Help.BrowseURL(@"https://videokit.ai");

        [MenuItem(@"VideoKit/Report an Issue", false, BasePriority + 3)]
        private static void OpenIssue () => Help.BrowseURL(@"https://github.com/vkt3d/videokit");
    }
}