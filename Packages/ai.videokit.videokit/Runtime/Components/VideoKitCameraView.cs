/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.UI {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.Serialization;
    using UnityEngine.UI;
    using Unity.Collections.LowLevel.Unsafe;
    using Muna;
    using Internal;
    using Facing = VideoKitCameraManager.Facing;
    using Image = Muna.Image;

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
        public VideoKitCameraManager? cameraManager;

        /// <summary>
        /// Desired camera facing to display.
        /// </summary>
        [Tooltip(@"Desired camera facing to display.")]
        public Facing facing = Facing.User | Facing.World;

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
        /// Event raised when a new camera frame is available in the preview texture.
        /// </summary>
        [Tooltip(@"Event raised when a new camera frame is available.")]
        public UnityEvent? OnCameraFrame;
        #endregion


        #region --Client API--
        /// <summary>
        /// Get the camera device that this view displays.
        /// </summary>
        internal CameraDevice? device => VideoKitCameraManager // CHECK // Should we make this public??
            .EnumerateCameraDevices(cameraManager?.device)
            .FirstOrDefault(device => facing.HasFlag(VideoKitCameraManager.GetCameraFacing(device)));

        /// <summary>
        /// Get the camera preview texture.
        /// </summary>
        public Texture2D? texture { get; private set; }

        /// <summary>
        /// Get or set the camera preview rotation.
        /// This is automatically reset when the component is enabled.
        /// </summary>
        public PixelBuffer.Rotation rotation { get; set; }

        /// <summary>
        /// Event raised when a new pixel buffer is available.
        /// Unlike the `VideoKitCameraManager`, this pixel buffer has an `RGBA8888` format and is rotated upright.
        /// This event is invoked on a dedicated camera thread, NOT the Unity main thread.
        /// </summary>
        public event Action<PixelBuffer>? OnPixelBuffer;
        #endregion


        #region --Operations--
        private PixelBuffer pixelBuffer;
        private RawImage rawImage;
        private AspectRatioFitter aspectFitter;
        private readonly object fence = new();
        private static readonly List<RuntimePlatform> OrientationSupport = new() {
            RuntimePlatform.Android,
            RuntimePlatform.IPhonePlayer
        };

        private void Reset() {
            cameraManager = FindFirstObjectByType<VideoKitCameraManager>();
        }

        private void Awake() {
            rawImage = GetComponent<RawImage>();
            aspectFitter = GetComponent<AspectRatioFitter>();
        }

        private void OnEnable() {
            rotation = GetPreviewRotation(Screen.orientation);
            if (cameraManager != null)
                cameraManager.OnPixelBuffer += OnCameraBuffer;
        }

        private unsafe void Update() {
            bool upload = false;
            lock (fence) {
                if (pixelBuffer == IntPtr.Zero)
                    return;
                if (
                    texture != null &&
                    (texture.width != pixelBuffer.width || texture.height != pixelBuffer.height)
                ) {
                    Texture2D.Destroy(texture);
                    texture = null;
                }
                if (texture == null)
                    texture = new Texture2D(
                        pixelBuffer.width,
                        pixelBuffer.height,
                        TextureFormat.RGBA32,
                        false
                    );
                if (viewMode == ViewMode.CameraTexture) {
                    using var buffer = new PixelBuffer(texture);
                    pixelBuffer.CopyTo(buffer);
                    upload = true;
                } else if (viewMode == ViewMode.HumanTexture) {
                    var muna = VideoKitClient.Instance!.muna;
                    var prediction = muna.Predictions.Create(
                        tag: VideoKitCameraManager.HumanTextureTag,
                        inputs: new () {
                            ["image"] = new Image(
                                (byte*)pixelBuffer.data.GetUnsafePtr(),
                                pixelBuffer.width,
                                pixelBuffer.height,
                                4
                            )
                        }
                    ).Throw().Result;
                    var image = (Image)prediction.results![0]!;
                    image.ToTexture(texture);
                    upload = true;
                }
            }
            if (upload)
                texture.Apply();
            rawImage.texture = texture;
            aspectFitter.aspectRatio = (float)texture.width / texture.height;
            OnCameraFrame?.Invoke();
        }

        private unsafe void OnCameraBuffer(
            CameraDevice cameraDevice,
            PixelBuffer cameraBuffer
        ) {

            Debug.Log($"Camera front {cameraDevice.frontFacing} mirrored: {cameraBuffer.verticallyMirrored}");

            if ((VideoKitCameraManager.GetCameraFacing(cameraDevice) & facing) == 0)
                return;
            var (width, height) = GetPreviewTextureSize(
                cameraBuffer.width,
                cameraBuffer.height,
                rotation
            );
            lock (fence) {
                if (
                    pixelBuffer != IntPtr.Zero &&
                    (pixelBuffer.width != width || pixelBuffer.height != height)
                ) {
                    pixelBuffer.Dispose();
                    pixelBuffer = default;
                }
                if (pixelBuffer == IntPtr.Zero)
                    pixelBuffer = new PixelBuffer(
                        width,
                        height,
                        PixelBuffer.Format.RGBA8888,
                        mirrored: true
                    );
                cameraBuffer.CopyTo(pixelBuffer, rotation: rotation);
            }
            OnPixelBuffer?.Invoke(pixelBuffer);
        }

        private void OnDisable() {
            if (cameraManager != null)
                cameraManager.OnPixelBuffer -= OnCameraBuffer;
        }

        private void OnDestroy() {
            pixelBuffer.Dispose();
            pixelBuffer = default;
        }
        #endregion


        #region --Handlers--

        void IPointerUpHandler.OnPointerUp(PointerEventData data) {
            // Check device
            var device = this.device;
            if (device == null)
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
            var point = Rect.PointToNormalized(rectTransform!.rect, localPoint);
            if (device.focusPointSupported && focusGesture == GestureMode.Tap)
                device.SetFocusPoint(point.x, point.y);
            if (device.exposurePointSupported && exposureGesture == GestureMode.Tap)
                device.SetExposurePoint(point.x, point.y);
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData data) {

        }

        void IDragHandler.OnDrag(PointerEventData data) {

        }
        #endregion


        #region --Utilities--

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static PixelBuffer.Rotation GetPreviewRotation(ScreenOrientation orientation) => orientation switch {
            var _ when !OrientationSupport.Contains(Application.platform)   => PixelBuffer.Rotation._0,
            ScreenOrientation.LandscapeLeft                                 => PixelBuffer.Rotation._0,
            ScreenOrientation.Portrait                                      => PixelBuffer.Rotation._90,
            ScreenOrientation.LandscapeRight                                => PixelBuffer.Rotation._180,
            ScreenOrientation.PortraitUpsideDown                            => PixelBuffer.Rotation._270,
            _                                                               => PixelBuffer.Rotation._0
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (int width, int height) GetPreviewTextureSize(
            int width,
            int height,
            PixelBuffer.Rotation rotation
        ) => rotation == PixelBuffer.Rotation._90 || rotation == PixelBuffer.Rotation._270 ?
            (height, width) :
            (width, height);
        #endregion
    }
}