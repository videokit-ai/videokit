/* 
*   VideoKit
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.UI {

    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// </summary>
    [RequireComponent(typeof(RawImage), typeof(AspectRatioFitter))]
    public sealed class VideoKitCameraView : MonoBehaviour {
        
        #region --Enumerations--
        /// <summary>
        /// </summary>
        public enum ViewMode {
            /// <summary>
            /// </summary>
            CameraTexture   = 0,
            /// <summary>
            /// </summary>
            HumanTexture    = 1
        }
        #endregion


        #region --Inspector--
        /// <summary>
        /// VideoKit camera manager.
        /// </summary>
        [Tooltip(@"VideoKit camera manager.")]
        public VideoKitCameraManager cameraManager;

        /// <summary>
        /// </summary>
        [Tooltip(@"")]
        public ViewMode viewMode = ViewMode.CameraTexture;
        #endregion


        #region --Operations--
        private RawImage rawImage;
        private AspectRatioFitter aspectFitter;

        private void Awake () {
            rawImage = GetComponent<RawImage>();
            aspectFitter = GetComponent<AspectRatioFitter>();
            cameraManager.OnFrame.AddListener(OnCameraFrame);
        }

        private void Reset () {
            cameraManager = FindObjectOfType<VideoKitCameraManager>();
        }

        private void OnCameraFrame (CameraFrame frame) {
            rawImage.texture = viewMode == ViewMode.HumanTexture ? frame.humanTexture : frame.texture;
            aspectFitter.aspectRatio = (float)frame.width / frame.height;
        }
        #endregion
    }
}