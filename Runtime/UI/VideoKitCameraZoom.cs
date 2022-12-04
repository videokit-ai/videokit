/* 
*   VideoKit
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.UI {

    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;

    /// <summary>
    /// VideoKit UI component for zooming a camera device with drag or pinch gestures.
    /// </summary>
    [Tooltip(@"VideoKit UI component for zooming a camera device with drag or pinch gestures.")]
    [RequireComponent(typeof(VideoKitCameraView), typeof(EventTrigger)), DisallowMultipleComponent]
    internal sealed class VideoKitCameraZoom : MonoBehaviour, IBeginDragHandler, IDragHandler { // INCOMPLETE

        #region --Enumerations--
        /// <summary>
        /// Zoom gesture mode.
        /// </summary>
        public enum GestureMode {
            /// <summary>
            /// Do not respond to any gestures.
            /// </summary>
            None = 0,
            /// <summary>
            /// Detect two-finger pinch gestures to zoom.
            /// </summary>
            Pinch = 1,
            /// <summary>
            /// Detect single-finger drag gestures to zoom.
            /// This gesture mode is recommended when the user is holding a button to record a video.
            /// </summary>
            Drag = 2,
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
        /// Zoom gesture mode.
        /// </summary>
        [Tooltip(@"Zoom gesture mode.")]
        public GestureMode gestureMode = GestureMode.Pinch;
        #endregion


        #region --Operations--

        private void Reset () => cameraManager = FindObjectOfType<VideoKitCameraManager>();

        void IBeginDragHandler.OnBeginDrag (PointerEventData data) {

        }

        void IDragHandler.OnDrag (PointerEventData data) {

        }
        #endregion
    }
}