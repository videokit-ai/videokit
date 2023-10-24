/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Recorders.Inputs {

    using AOT;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.Rendering;
    using Internal;

    /// <summary>
    /// Recorder input for recording video frames from textures with hardware acceleration on Android OpenGL ES3.
    /// </summary>
    public sealed class GLESTextureInput : TextureInput {

        #region --Client API--
        /// <summary>
        /// Create a GLES texture input.
        /// </summary>
        /// <param name="recorder">Media recorder to receive video frames.</param>
        public GLESTextureInput (MediaRecorder recorder) : base(recorder) {
            // Check platform
            if (Application.platform != RuntimePlatform.Android)
                throw new InvalidOperationException(@"GLESTextureInput can only be used on Android");
            // Check render API
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES3)
                throw new InvalidOperationException(@"GLESTextureInput can only be used with OpenGL ES3");
            // Initialize
            var (width, height) = recorder.frameSize;
            this.frameBuffer = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            this.frameBuffer.Create();
            this.frameBufferID = frameBuffer.GetNativeTexturePtr();
            this.fence = new object();
            // Create native input
            VideoKitExt.CreateTexutreInput(width, height, OnReadbackCompleted, out input);
        }

        /// <summary>
        /// Commit a video frame from a texture.
        /// </summary>
        /// <param name="texture">Source texture.</param>
        /// <param name="timestamp">Frame timestamp in nanoseconds.</param>
        public override unsafe void CommitFrame (Texture texture, long timestamp) {
            // Check
            lock (fence)
                if (input == default)
                    return;
            // Create command buffer
            Action<IntPtr> callback = pixelBuffer => recorder.CommitFrame((void*)pixelBuffer, timestamp);
            var handle = GCHandle.Alloc(callback, GCHandleType.Normal);
            var commandBuffer = new CommandBuffer();
            commandBuffer.name = @"GLESTextureInput";
            commandBuffer.Blit(texture, frameBuffer);
            RunOnRenderThread(commandBuffer, () => {
                lock (fence)
                    input.CommitFrame(frameBufferID, (IntPtr)handle);
            });
            // Execute
            Graphics.ExecuteCommandBuffer(commandBuffer);
        }

        /// <summary>
        /// Stop recorder input and release resources.
        /// </summary>
        public override void Dispose () {
            lock (fence) {
                frameBuffer.Release();
                input.ReleaseTextureInput();
                input = default;
                base.Dispose();
            }
        }
        #endregion


        #region --Operations--
        private readonly RenderTexture frameBuffer;
        private readonly IntPtr frameBufferID;
        private IntPtr input;
        private readonly object fence;
        private static readonly IntPtr RenderThreadCallback;

        static GLESTextureInput () => RenderThreadCallback = Marshal.GetFunctionPointerForDelegate<UnityRenderingEventAndData>(OnRenderThreadInvoke);

        /// <summary>
        /// Register a delegate to be invoked on the Unity render thread.
        /// </summary>
        /// <param name="commandBuffer">Command buffer.</param>
        /// <param name="action">Delegate to invoke on the Unity render thread.</param>
        private static void RunOnRenderThread (CommandBuffer commandBuffer, Action action) {
            var handle = GCHandle.Alloc(action, GCHandleType.Normal);
            commandBuffer.IssuePluginEventAndData(RenderThreadCallback, default, (IntPtr)handle);
        }

        [MonoPInvokeCallback(typeof(UnityRenderingEventAndData))]
        private static void OnRenderThreadInvoke (int _, IntPtr context) {
            // Check
            if (!VideoKit.IsAppDomainLoaded)
                return;
            try {
                // Invoke
                var handle = (GCHandle)context;
                var action = handle.Target as Action;
                handle.Free();
                action?.Invoke();
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        [MonoPInvokeCallback(typeof(VideoKitExt.ReadbackHandler))]
        private static void OnReadbackCompleted (IntPtr context, IntPtr pixelBuffer) {
            // Check
            if (!VideoKit.IsAppDomainLoaded)
                return;
            try {
                // Invoke
                var handle = (GCHandle)context;
                var handler = handle.Target as Action<IntPtr>;
                handle.Free();
                handler?.Invoke(pixelBuffer);
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void UnityRenderingEventAndData (int _, IntPtr data);
        #endregion
    }
}