/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Editor.Build {

    using UnityEditor;

    internal sealed class macOSBuildHandler : BuildHandler {

        protected override BuildTarget target => BuildTarget.StandaloneOSX;
    }
}