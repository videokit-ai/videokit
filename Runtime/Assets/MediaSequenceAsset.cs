/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Assets {

    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;
    using Function.Types;
    using Internal;

    /// <summary>
    /// Media sequence which contains a collection of media assets.
    /// </summary>
    public sealed class MediaSequenceAsset : MediaAsset {

        #region --Client API--
        /// <summary>
        /// Media assets within the sequence.
        /// </summary>
        public readonly MediaAsset[] assets;

        /// <inheritdoc/>
        public override async Task<Value> ToValue (int minUploadSize = 4096) {
            throw new InvalidOperationException(@"Image sequences cannot be converted to Function values");
        }

        /// <summary>
        /// This is not supported and will raise an exception.
        /// </summary>
        public override async Task<string?> Share (string? message = null) {
            throw new InvalidOperationException(@"Image sequences cannot be shared");
        }

        /// <summary>
        /// This is not supported and will raise an exception.
        /// </summary>
        public override async Task<bool> SaveToCameraRoll (string? album = null) {
            throw new InvalidOperationException(@"Image sequences cannot be saved to the camera roll");
        }

        /// <inheritdoc/>
        public override void Delete () {
            foreach (var asset in assets)
                asset.Delete();
        }
        #endregion


        #region --Operations--

        internal MediaSequenceAsset (string path, MediaAsset[] assets) {
            this.path = path;
            this.assets = assets;
        }
        #endregion
    }
}