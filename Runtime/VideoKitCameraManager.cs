/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Events;
    using Unity.Collections.LowLevel.Unsafe;
    using NatML.Devices;
    using NatML.Devices.Outputs;
    using ML;
    using Internal;

    /// <summary>
    /// VideoKit camera manager for streaming video from camera devices.
    /// </summary>
    [Tooltip(@"VideoKit camera manager for streaming video from camera devices."), DisallowMultipleComponent]
    public sealed partial class VideoKitCameraManager : MonoBehaviour {

        #region --Inspector--
        [Header(@"Configuration")]
        /// <summary>
        /// Desired camera resolution.
        /// </summary>
        [Tooltip(@"The desired camera resolution.")]
        public Resolution resolution = Resolution._1280x720;

        /// <summary>
        /// Desired camera capabilities.
        /// </summary>
        [Tooltip(@"Desired camera capabilities.")]
        public Capabilities capabilities;

        /// <summary>
        /// Whether to start the camera preview as soon as the component awakes.
        /// </summary>
        [Tooltip(@"Whether to start the camera preview as soon as the component awakes.")]
        public bool playOnAwake = true;

        [Header(@"Camera Settings")]
        /// <summary>
        /// Desired camera focus mode.
        /// </summary>
        [Tooltip(@"Desired camera focus mode.")]
        public CameraDevice.FocusMode focusMode = CameraDevice.FocusMode.Continuous;

        /// <summary>
        /// Desired camera exposure mode.
        /// </summary>
        [Tooltip(@"Desired camera exposure mode.")]
        public CameraDevice.ExposureMode exposureMode = CameraDevice.ExposureMode.Continuous;

        [Header(@"Events")]
        /// <summary>
        /// Event raised when a new camera frame is available.
        /// </summary>
        [Tooltip(@"Event raised when a new camera frame is available.")]
        public UnityEvent<CameraFrame> OnCameraFrame;
        #endregion


        #region --Client API--
        /// <summary>
        /// Get ot set the camera device used to stream camera images.
        /// </summary>
        public CameraDevice cameraDevice {
            get => device;
            set {
                var restart = running;
                StopRunning();
                device = value;
                if (restart)
                    StartRunning();
            }
        }

        /// <summary>
        /// Whether the camera is running.
        /// </summary>
        public bool running => output != null;

        /// <summary>
        /// Start the camera preview.
        /// </summary>
        public async void StartRunning () {
            // Check
            if (!isActiveAndEnabled) {
                Debug.LogError(@"VideoKit: Camera manager failed to start running because component is disabled");
                return;
            }
            // Check
            if (running)
                return;
            // Get device
            device ??= await GetDefaultCameraDevice();
            if (device == null) {
                Debug.LogError(@"VideoKit: Camera manager failed to start because no camera device is available");
                return;
            }
            // Check capabilities
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                capabilities |= Capabilities.MachineLearning;
            // Create MatteKit
            if (capabilities.HasFlag(Capabilities.HumanTexture) && matteKit == null) {
                var model = await MLEdgeModel.Create("@natml/mattekit");
                matteKit = new MatteKitPredictor(model);
            }
            // Create output
            if (capabilities.HasFlag(Capabilities.MachineLearning)) {
                var textureOutput = new TextureOutput();
                textureOutput.OnFrame += OnCameraTexture;
                output = textureOutput;
            } else {
                var renderTextureOutput = new RenderTextureOutput();
                renderTextureOutput.OnFrame += OnCameraTexture;
                output = renderTextureOutput;
            }
            // Configure camera
            if (resolution != Resolution.Default)
                device.previewResolution = resolution.FrameSize();
            if (device.FocusModeSupported(focusMode))
                device.focusMode = focusMode;
            if (device.ExposureModeSupported(exposureMode))
                device.exposureMode = exposureMode;
            // Start running
            device.StartRunning(OnCameraImage);
        }

        /// <summary>
        /// Stop the camera preview.
        /// </summary>
        public void StopRunning () {
            device?.StopRunning();
            output?.Dispose();
            output = null;
        }
        #endregion


        #region --Operations--
        private CameraDevice device;
        private CameraOutput output;
        private MatteKitPredictor matteKit;
        private readonly Queue<CameraImage> imageQueue = new Queue<CameraImage>();

        private void Start () { // CHECK // Change to `Awake` when NatDevice #29 is fixed
            if (playOnAwake)
                StartRunning();
        }

        private void OnCameraImage (CameraImage image) {
            output.Update(image);
            lock ((imageQueue as ICollection).SyncRoot)
                imageQueue.Enqueue(image);
        }

        private void OnCameraTexture (TextureOutput output) {
            var image = GetOutputImage(output.texture, output.timestamp);
            var humanTexture = matteKit?.Predict(output.texture);
            var frame = new CameraFrame(image, output.texture, humanTexture);
            OnCameraFrame?.Invoke(frame);
        }

        private void OnCameraTexture (RenderTextureOutput output) {
            var image = GetOutputImage(output.texture, output.timestamp);
            var frame = new CameraFrame(image, output.texture, null);
            OnCameraFrame?.Invoke(frame);
        }

        private void OnDisable () => StopRunning();

        private void OnDestroy () => matteKit?.Dispose();

        private CameraImage GetOutputImage (Texture texture, long timestamp) {
            var texture2D = texture as Texture2D;
            lock ((imageQueue as ICollection).SyncRoot)
                while (imageQueue.TryDequeue(out var image))
                    if (image.timestamp == timestamp)
                        return new CameraImage(
                            image.device,
                            texture2D?.GetRawTextureData<byte>() ?? default,
                            texture2D ? CameraImage.Format.RGBA8888 : CameraImage.Format.Unknown,
                            texture.width,
                            texture.height,
                            (texture2D?.width * 4) ?? 0,
                            image.timestamp,
                            false,
                            null,
                            image.intrinsics,
                            image.exposureBias,
                            image.exposureDuration,
                            image.ISO,
                            image.focalLength,
                            image.fNumber,
                            image.brightness
                        );
            return default;
        }

        private static async Task<CameraDevice> GetDefaultCameraDevice () {
            // Request camera permissions
            var permissionStatus = await MediaDeviceQuery.RequestPermissions<CameraDevice>();
            if (permissionStatus != PermissionStatus.Authorized) {
                Debug.LogWarning(@"VideoKit: User did not grant camera permissions");
                return null;
            }
            // Discover the front camera
            var query = new MediaDeviceQuery(MediaDeviceCriteria.CameraDevice);
            var device = query.FirstOrDefault(device => device is CameraDevice camera && camera.frontFacing) ?? query.current;
            // Check
            if (device == null)
                Debug.LogWarning(@"VideoKit: Failed to discover any available camera device");
            // Return
            return device as CameraDevice;
        }
        #endregion
    }
}