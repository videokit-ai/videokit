/* 
*   VideoKit
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.UI {

    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    /// <summary>
    /// VideoKit UI component for displaying the camera preview from a camera manager.
    /// </summary>
    [Tooltip(@"VideoKit UI component for displaying the camera preview from a camera manager.")]
    [RequireComponent(typeof(RawImage), typeof(AspectRatioFitter)), DisallowMultipleComponent]
    public sealed class VideoKitCameraView : MonoBehaviour {
        
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
        [Header(@"Configuratuon")]
        /// <summary>
        /// VideoKit camera manager.
        /// </summary>
        [Tooltip(@"VideoKit camera manager.")]
        public VideoKitCameraManager cameraManager;

        /// <summary>
        /// View mode of the view.
        /// </summary>
        [Tooltip(@"View mode of the view.")]
        public ViewMode viewMode = ViewMode.CameraTexture;

        [Header(@"Events")]
        /// <summary>
        /// Event raised when the camera preview is presented on the UI panel.
        /// </summary>
        [Tooltip(@"Event raised when the camera preview is presented on the UI panel.")]
        public UnityEvent<VideoKitCameraView> OnPresent;
        #endregion


        #region --Operations--
        private RawImage rawImage;
        private AspectRatioFitter aspectFitter;
        private bool presented;

        private void Reset () => cameraManager = FindObjectOfType<VideoKitCameraManager>();

        private void Awake () {
            // Get components
            rawImage = GetComponent<RawImage>();
            aspectFitter = GetComponent<AspectRatioFitter>();
            presented = false;
            // Listen for frames
            if (cameraManager)
                cameraManager.OnFrame.AddListener(OnCameraFrame);
        }

        private void Update () => presented &= rawImage.texture;

        private void OnCameraFrame (CameraFrame frame) {
            // Check
            if (!isActiveAndEnabled)
                return;
            // Get texture
            var texture = viewMode == ViewMode.HumanTexture ? (Texture)frame.humanTexture : frame.texture;
            if (!texture)
                return;
            // Display
            rawImage.texture = texture;
            aspectFitter.aspectRatio = (float)texture.width / texture.height;
            // Invoke event
            if (!presented)
                OnPresent?.Invoke(this);
            presented = true;
        }

        private void OnDisable () => presented = false;
        #endregion
    }
}