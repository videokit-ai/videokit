/* 
*   VideoKit
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.UI {

    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// VideoKit UI component for displaying the camera preview from a camera manager.
    /// </summary>
    [Tooltip(@"VideoKit UI component for displaying the camera preview from a camera manager.")]
    [RequireComponent(typeof(RawImage), typeof(AspectRatioFitter)), DisallowMultipleComponent]
    public sealed class VideoKitCameraView : MonoBehaviour { // DOC
        
        #region --Enumerations--
        /// <summary>
        /// View mode.
        /// </summary>
        public enum ViewMode {
            /// <summary>
            /// Display the camera texture.
            /// </summary>
            CameraTexture   = 0,
            /// <summary>
            /// Display the human texture.
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
        /// View mode of the view.
        /// </summary>
        [Tooltip(@"")]
        public ViewMode viewMode = ViewMode.CameraTexture;
        #endregion


        #region --Operations--
        private RawImage rawImage;
        private AspectRatioFitter aspectFitter;

        private void Awake () {
            // Get components
            rawImage = GetComponent<RawImage>();
            aspectFitter = GetComponent<AspectRatioFitter>();
            // Listen for frames
            if (cameraManager)
                cameraManager.OnFrame.AddListener(OnCameraFrame);
        }

        private void Reset () => cameraManager = FindObjectOfType<VideoKitCameraManager>();

        private void OnCameraFrame (CameraFrame frame) {
            var texture = viewMode == ViewMode.HumanTexture ? (Texture)frame.humanTexture : frame.texture;
            rawImage.texture = texture;
            aspectFitter.aspectRatio = (float)texture.width / texture.height;
        }
        #endregion
    }
}