/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Recorders.Inputs {

    using UnityEngine;

    internal interface ITextureInput {

        void CommitFrame (Texture source, RenderTexture destination);
    }
}