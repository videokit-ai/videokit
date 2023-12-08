/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Recorders.Inputs {

    using System;
    using UnityEngine;
    using UnityEngine.Rendering;
    using Unity.Collections.LowLevel.Unsafe;

    /// <summary>
    /// Recorder input for recording video frames from textures.
    /// Textures will be recorded by performing a synchronous pixel buffer readback.
    /// </summary>
    public class TextureInput : IDisposable {

        #region --Client API--
        /// <summary>
        /// Texture input frame size.
        /// Note that it is not required that committed textures have the frame size, though it is recommended.
        /// </summary>
        public virtual (int width, int height) frameSize => recorder.frameSize;

        /// <summary>
        /// When an Android app renders with OpenGL ES3, the default `TextureInput` implementation can be very expensive.
        /// Enabling this flag will use the custom `GLESTextureInput` to accelerate pixel buffer readbacks from the GPU.
        /// Apps that use ARFoundation will see recording performance greatly benefit from enabling this flag.
        /// This flag only has an effect on Android when rendering with OpenGL ES3.
        /// This flag defaults to `true`.
        /// </summary>
        public static bool UseGLESTextureInput = true;

        /// <summary>
        /// Create a texture input which performs synchronous readbacks.
        /// </summary>
        /// <param name="recorder">Media recorder to receive video frames.</param>
        public TextureInput (MediaRecorder recorder) => this.recorder = recorder;

        /// <summary>
        /// Commit a video frame from a texture.
        /// </summary>
        /// <param name="texture">Source texture.</param>
        /// <param name="timestamp">Frame timestamp in nanoseconds.</param>
        public virtual void CommitFrame (Texture texture, long timestamp) {
            // Blit
            var (width, height) = recorder.frameSize;
            var renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            Graphics.Blit(texture, renderTexture);
            // Readback // Completely kills performance
            var prevActive = RenderTexture.active;
            RenderTexture.active = renderTexture;
            readbackBuffer = readbackBuffer ? readbackBuffer : new Texture2D(width, height, TextureFormat.RGBA32, false);
            readbackBuffer.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
            RenderTexture.active = prevActive;
            RenderTexture.ReleaseTemporary(renderTexture);
            // Commit
            recorder.CommitFrame(readbackBuffer.GetRawTextureData<byte>(), timestamp);
        }

        /// <summary>
        /// Stop recorder input and release resources.
        /// </summary>
        public virtual void Dispose () => Texture2D.Destroy(readbackBuffer);

        /// <summary>
        /// Create the platform default texture input for the given recorder.
        /// </summary>
        /// <param name="recorder"></param>
        /// <returns>Create texture input.</returns>
        public static TextureInput CreateDefault (MediaRecorder recorder) => Application.platform switch {
            RuntimePlatform.Android when AllowGLESDefault   => new GLESTextureInput(recorder),
            RuntimePlatform.WebGLPlayer                     => new TextureInput(recorder),
            _ when SystemInfo.supportsAsyncGPUReadback      => new AsyncTextureInput(recorder),
            _                                               => new TextureInput(recorder),
        };
        #endregion


        #region --Operations--
        protected readonly MediaRecorder recorder;
        private Texture2D readbackBuffer;
        private static bool AllowGLESDefault => SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 && UseGLESTextureInput;
        #endregion
    }
}