/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.UI {

    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.Serialization;
    using UnityEngine.UI;

    /// <summary>
    /// VideoKit UI component for displaying the camera preview from a camera manager.
    /// </summary>
    [Tooltip(@"VideoKit UI component for displaying the camera preview from a camera manager.")]
    [RequireComponent(typeof(RawImage), typeof(AspectRatioFitter), typeof(EventTrigger))]
    [HelpURL(@"https://videokit.ai/reference/videokitcameraview")]
    [DisallowMultipleComponent]
    public sealed partial class VideoKitCameraView : MonoBehaviour, IPointerUpHandler, IBeginDragHandler, IDragHandler {

        #region --Enumerations--
        /// <summary>
        /// View mode.
        /// </summary>
        public enum ViewMode : int {
            /// <summary>
            /// Display the camera texture.
            /// </summary>
            CameraTexture       = 0,
            /// <summary>
            /// Display the human texture.
            /// </summary>
            HumanTexture        = 1,
        }

        /// <summary>
        /// Gesture mode.
        /// </summary>
        public enum GestureMode : int {
            /// <summary>
            /// Do not respond to gestures.
            /// </summary>
            None = 0,
            /// <summary>
            /// Detect tap gestures.
            /// </summary>
            Tap = 1,
            /// <summary>
            /// Detect two-finger pinch gestures.
            /// </summary>
            Pinch = 2,
            /// <summary>
            /// Detect single-finger drag gestures.
            /// This gesture mode is recommended when the user is holding a button to record a video.
            /// </summary>
            Drag = 3,
        }
        #endregion


        #region --Inspector--
        [Header(@"Configuration")]
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

        [Header(@"Gestures")]
        /// <summary>
        /// Focus gesture.
        /// </summary>
        [Tooltip(@"Focus gesture."), FormerlySerializedAs(@"focusMode")]
        public GestureMode focusGesture = GestureMode.None;

        /// <summary>
        /// Exposure gesture.
        /// </summary>
        [Tooltip(@"Exposure gesture."), FormerlySerializedAs(@"exposureMode")]
        public GestureMode exposureGesture = GestureMode.None;

        /// <summary>
        /// Zoom gesture.
        /// </summary>
        [Tooltip(@"Zoom gesture."), FormerlySerializedAs(@"zoomMode")]
        public GestureMode zoomGesture = GestureMode.None;

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
                cameraManager.OnCameraFrame.AddListener(OnCameraFrame);
        }

        private void Update () => presented &= rawImage.texture;

        private void OnCameraFrame () {
            // Check
            if (!isActiveAndEnabled)
                return;
            // Get texture
            var texture = viewMode == ViewMode.HumanTexture ? cameraManager.humanTexture : cameraManager.texture;
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

        void IPointerUpHandler.OnPointerUp (PointerEventData data) {
            // Check manager
            if (!cameraManager)
                return;
            // Check focus mode
            if (focusGesture != GestureMode.Tap && exposureGesture != GestureMode.Tap)
                return;
            // Get press position
            var rectTransform = transform as RectTransform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                data.position,
                data.pressEventCamera, // or `enterEventCamera`
                out var localPoint
            ))
                return;
            // Focus
            var cameraDevice = cameraManager.device;
            var point = Rect.PointToNormalized(rectTransform.rect, localPoint);
            if (cameraDevice.focusPointSupported && focusGesture == GestureMode.Tap)
                cameraDevice.SetFocusPoint(point.x, point.y);
            if (cameraDevice.exposurePointSupported && exposureGesture == GestureMode.Tap)
                cameraDevice.SetExposurePoint(point.x, point.y);
        }

        void IBeginDragHandler.OnBeginDrag (PointerEventData data) {

        }

        void IDragHandler.OnDrag (PointerEventData data) {

        }
        #endregion
    }
}