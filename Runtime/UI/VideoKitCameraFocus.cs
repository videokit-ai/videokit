/* 
*   VideoKit
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.UI {

    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>
    /// VideoKit UI component for focusing a camera device with tap gestures.
    /// </summary>
    [Tooltip(@"VideoKit UI component for focusing a camera device with tap gestures.")]
    [RequireComponent(typeof(VideoKitCameraView), typeof(EventTrigger)), DisallowMultipleComponent]
    public sealed class VideoKitCameraFocus : MonoBehaviour, IPointerUpHandler {

        #region --Enumerations--
        /// <summary>
        /// Focus mode.
        /// </summary>
        [Flags]
        public enum FocusMode {
            /// <summary>
            /// Do not focus.
            /// </summary>
            None = 0,
            /// <summary>
            /// Set the camera focus point.
            /// </summary>
            Focus = 1,
            /// <summary>
            /// Set the camera exposure point.
            /// </summary>
            Exposure = 2
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
        /// Focus mode.
        /// </summary>
        [Tooltip(@"Focus mode.")]
        public FocusMode focusMode = FocusMode.Focus | FocusMode.Exposure;
        #endregion


        #region --Operations--

        private void Reset () => cameraManager = FindObjectOfType<VideoKitCameraManager>();

        void IPointerUpHandler.OnPointerUp (PointerEventData data) {
            // Check focus mode
            if (focusMode == FocusMode.None)
                return;
            // Check manager
            if (!cameraManager)
                return;
            // Check if focus is supported
            var cameraDevice = cameraManager.cameraDevice;
            if (!cameraDevice.focusPointSupported)
                return;
            // Get press position
            var rectTransform = transform as RectTransform;
            var validPress = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                data.position,
                data.pressEventCamera, // or `enterEventCamera`
                out var localPoint
            );
            // Check
            if (!validPress)
                return;
            // Focus
            var point = Rect.PointToNormalized(rectTransform.rect, localPoint);
            if (focusMode.HasFlag(FocusMode.Focus))
                cameraDevice.SetFocusPoint(point.x, point.y);
            if (focusMode.HasFlag(FocusMode.Exposure))
                cameraDevice.SetExposurePoint(point.x, point.y);
        }
        #endregion
    }
}