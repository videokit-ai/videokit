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

        /// <summary>
        /// This is not supported and will raise an exception.
        /// </summary>
        public override Task<string?> Share (string? message = null) {
            var ex = new InvalidOperationException(@"Image sequences cannot be shared");
            return Task.FromException<string>(ex);
        }

        /// <summary>
        /// This is not supported and will raise an exception.
        /// </summary>
        public override Task<bool> SaveToCameraRoll (string? album = null) {
            var ex = new InvalidOperationException(@"Image sequences cannot be saved to the camera roll");
            return Task.FromException<bool>(ex);
        }

        /// <inheritdoc/>
        public override void Delete () {
            foreach (var asset in assets)
                File.Delete(asset.path);
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