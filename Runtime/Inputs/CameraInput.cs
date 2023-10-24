/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Recorders.Inputs {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;
    using Clocks;

    /// <summary>
    /// Recorder input for recording video frames from one or more game cameras.
    /// </summary>
    public class CameraInput : IDisposable {

        #region --Client API--
        /// <summary>
        /// Cameras being recorded from.
        /// </summary>
        public readonly IReadOnlyList<Camera> cameras;

        /// <summary>
        /// Control number of successive camera frames to skip while recording.
        /// This is very useful for GIF recording, which typically has a lower framerate appearance.
        /// </summary>
        public int frameSkip;

        /// <summary>
        /// Create a video recording input from one or more game cameras.
        /// </summary>
        /// <param name="recorder">Media recorder to receive video frames.</param>
        /// <param name="clock">Recording clock for generating timestamps.</param>
        /// <param name="cameras">Game cameras to record.</param>
        public CameraInput (MediaRecorder recorder, IClock clock, params Camera[] cameras) : this(TextureInput.CreateDefault(recorder), clock, cameras) { }
        
        /// <summary>
        /// Create a video recording input from one or more game cameras.
        /// </summary>
        /// <param name="recorder">Media recorder to receive video frames.</param>
        /// <param name="cameras">Game cameras to record.</param>
        public CameraInput (MediaRecorder recorder, params Camera[] cameras) : this(recorder, default, cameras) { }

        /// <summary>
        /// Create a video recording input from one or more game cameras.
        /// </summary>
        /// <param name="input">Texture input to receive video frames.</param>
        /// <param name="clock">Recording clock for generating timestamps.</param>
        /// <param name="cameras">Game cameras to record.</param>
        public CameraInput (TextureInput input, IClock clock, params Camera[] cameras) {
            // Sort cameras by depth
            Array.Sort(cameras, (a, b) => (int)(100 * (a.depth - b.depth)));
            var (width, height) = input.frameSize;
            // Save state
            this.input = input;
            this.clock = clock;
            this.cameras = cameras;
            this.descriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGBHalf, 24) {
                sRGB = true,
                msaaSamples = Mathf.Max(QualitySettings.antiAliasing, 1)
            };
            // Start recording
            attachment = new GameObject(@"VideoKit.CameraInput.Attachment").AddComponent<CameraInputAttachment>();
            attachment.StartCoroutine(CommitFrames());
        }

        /// <summary>
        /// Create a video recording input from one or more game cameras.
        /// </summary>
        /// <param name="input">Texture input to receive video frames.</param>
        /// <param name="cameras">Game cameras to record.</param>
        public CameraInput (TextureInput input, params Camera[] cameras) : this(input, default, cameras) { }

        /// <summary>
        /// Stop recorder input and release resources.
        /// </summary>
        public void Dispose () {
            GameObject.DestroyImmediate(attachment.gameObject);
            input.Dispose();
        }
        #endregion


        #region --Operations--
        private readonly TextureInput input;
        private readonly IClock clock;
        private readonly RenderTextureDescriptor descriptor;
        private readonly CameraInputAttachment attachment;
        private int frameCount;

        private IEnumerator CommitFrames () {
            var yielder = new WaitForEndOfFrame();
            for (;;) {
                // Check frame index
                yield return yielder;
                if (frameCount++ % (frameSkip + 1) != 0)
                    continue;
                // Render cameras
                var frameBuffer = RenderTexture.GetTemporary(descriptor);
                ClearFrame(frameBuffer);
                for (var i = 0; i < cameras.Count; i++)
                    CommitFrame(cameras[i], frameBuffer);
                // Commit
                input.CommitFrame(frameBuffer, clock?.timestamp ?? 0L);
                RenderTexture.ReleaseTemporary(frameBuffer);
            }
        }

        protected virtual void ClearFrame (RenderTexture renderTexture) {
            var prevActive = RenderTexture.active;
            RenderTexture.active = renderTexture;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = prevActive;
        }

        protected virtual void CommitFrame (Camera source, RenderTexture destination) {
            var prevTarget = source.targetTexture;
            source.targetTexture = destination;
            source.Render();
            source.targetTexture = prevTarget;
        }

        private sealed class CameraInputAttachment : MonoBehaviour { }
        #endregion


        #region --DEPRECATED--
        [Obsolete(@"Deprecated in NatCorder 1.9.3. This property is no longer necessary.")]
        public bool HDR;
        #endregion
    }
}