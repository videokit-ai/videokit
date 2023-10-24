/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Editor {

    using UnityEditor;
    using Internal;

    internal static class VideoKitMenu {

        private const int BasePriority = 600;
        
        [MenuItem(@"NatML/VideoKit " + VideoKitClient.Version, false, BasePriority)]
        private static void Version () { }

        [MenuItem(@"NatML/VideoKit " + VideoKitClient.Version, true, BasePriority)]
        private static bool DisableVersion () => false;

        [MenuItem(@"NatML/View VideoKit Docs", false, BasePriority + 1)]
        private static void OpenDocs () => Help.BrowseURL(@"https://docs.natml.ai/videokit");

        [MenuItem(@"NatML/Open a VideoKit Issue", false, BasePriority + 2)]
        private static void OpenIssue () => Help.BrowseURL(@"https://github.com/natmlx/videokit");
    }
}