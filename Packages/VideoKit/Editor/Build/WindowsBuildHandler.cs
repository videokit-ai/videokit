/* 
*   VideoKit
*   Copyright © 2024 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Editor.Build {

    using UnityEditor;

    internal sealed class WindowsBuildHandler : BuildHandler {

        protected override BuildTarget target => BuildTarget.StandaloneWindows64;
    }
}