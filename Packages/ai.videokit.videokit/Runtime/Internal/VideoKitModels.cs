/* 
*   VideoKit
*   Copyright © 2026 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Internal {

    /// <summary>
    /// VideoKit models
    /// </summary>
   public static class VideoKitModels {
        /// <summary>
        /// Human segmentation model.
        /// </summary>
        public const string HumanTexture_v2 = @"@videokit/human-texture-2";
        /// <summary>
        /// OpenAI-compatible text-to-speech model.
        /// </summary>
        public const string Narrate_v1 = @"@videokit/narrate-v1-260725";
        /// <summary>
        /// OpenAI-compatible structured output parsing model.
        /// </summary>
        public const string Parse_v1 = @"@videokit/parse-v1-260721";
        /// <summary>
        /// OpenAI-compatible speech-to-text model.
        /// </summary>
        public const string Transcribe_v1 = @"@videokit/transcribe-v1";
    }
}