/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Editor {

    using UnityEditor;
    using Internal;

    internal static class VideoKitMenu {

        private const int BasePriority = 600;
        
        [MenuItem(@"Tools/VideoKit/VideoKit " + VideoKitClient.Version, false, BasePriority)]
        private static void Version () { }

        [MenuItem(@"Tools/VideoKit/VideoKit " + VideoKitClient.Version, true, BasePriority)]
        private static bool DisableVersion () => false;

        [MenuItem(@"Tools/VideoKit/Get Access Key", false, BasePriority + 1)]
        private static void GetAccessKey () => Help.BrowseURL(@"https://videokit.ai?account");

        [MenuItem(@"Tools/VideoKit/View the Docs", false, BasePriority + 2)]
        private static void OpenDocs () => Help.BrowseURL(@"https://videokit.ai");

        [MenuItem(@"Tools/VideoKit/Report an Issue", false, BasePriority + 3)]
        private static void OpenIssue () => Help.BrowseURL(@"https://github.com/vkt3d/videokit");
    }
}