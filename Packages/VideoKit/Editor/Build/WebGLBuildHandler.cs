/* 
*   VideoKit
*   Copyright © 2024 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Editor.Build {

    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEditor.Build.Reporting;
    using Internal;

    internal sealed class WebGLBuildHandler : BuildHandler {

        private static string[] EM_ARGS => new string[] {
            
        };

        protected override BuildTarget target => BuildTarget.WebGL;

        protected override VideoKitClient CreateClient (BuildReport report) {
            // Set Emscripten args
            PlayerSettings.WebGL.emscriptenArgs = GetEmscriptenArgs();
            // Create client
            return base.CreateClient(report);
        }

        private static string GetEmscriptenArgs () {
            var cleanedArgs = Regex.Replace(
                PlayerSettings.WebGL.emscriptenArgs,
                @"-Wl,-uVKT_WEBGL_PUSH.*?-Wl,-uVKT_WEBGL_POP",
                string.Empty,
                RegexOptions.Singleline
            ).Split(' ');
            var args = new List<string>();
            args.AddRange(cleanedArgs);
            args.Add(@"-Wl,-uVKT_WEBGL_PUSH");
            args.AddRange(EM_ARGS);
            args.Add(@"-Wl,-uVKT_WEBGL_POP");
            return string.Join(@" ", args);
        }
    }
}