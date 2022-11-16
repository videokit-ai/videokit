/* 
*   VideoKit
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
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
    using NatML.Features;
    using NatML.Internal;
    using NatML.Types;

    /// <summary>
    /// VideoKit camera manager for streaming the camera.
    /// </summary>
    [Tooltip(@"VideoKit camera manager for streaming the camera."), DisallowMultipleComponent]
    public sealed class VideoKitCameraManager : MonoBehaviour {

        #region --Enumerations--
        /// <summary>
        /// Camera resolution presets.
        /// </summary>
        public enum Resolution {
            /// <summary>
            /// Use the default camera resolution.
            /// With this preset, the camera resolution will not be set.
            /// </summary>
            Default     = 0,
            /// <summary>
            /// Lowest resolution supported by the camera device.
            /// </summary>
            Lowest      = 1,
            /// <summary>
            /// SD resolution.
            /// </summary>
            _640x480    = 2,
            /// <summary>
            /// HD resolution.
            /// </summary>
            _1280x720   = 3,
            /// <summary>
            /// Full HD resolution.
            /// </summary>
            _1920x1080  = 4,
            /// <summary>
            /// Highest resolution supported by the camera device.
            /// Using this resolution is strongly not recommended.
            /// </summary>
            Highest     = 5
        }

        /// <summary>
        /// Camera manager capabilities.
        /// </summary>
        [Flags]
        public enum Capabilities {
            /// <summary>
            /// Ensure that the camera preview data can be used for machine learning predictions.
            /// Enabling this adds a minimal performance impact so it should be disabled when not needed.
            /// This flag is always enabled on WebGL.
            /// </summary>
            MachineLearning = 1 << 0,
            /// <summary>
            /// Generate a human segmentation texture while streaming the camera preview.
            /// This is useful for building certain types of video filters.
            /// This flag adds a variable performance cost, so enable it only when necessary.
            /// </summary>
            HumanTexture    = (1 << 2) | Capabilities.MachineLearning,
            /// <summary>
            /// Stream depth data along with the camera preview data.
            /// This flag adds a minimal performance cost, so enable it only when necessary.
            /// </summary>
            DepthTexture    = 1 << 3,
        }
        #endregion


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
        /// Event raised when the camera preview starts.
        /// </summary>
        [Tooltip(@"Event raised when the camera preview starts.")]
        public UnityEvent<CameraFrame> OnFrame;
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
            if (running)
                return;
            // Get device
            device ??= await CreateCameraDevice();
            if (device == null) {
                Debug.LogError(@"VideoKit Error: Camera manager could not start because no camera device was available");
                return;
            }
            // Check capabilities
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                capabilities |= Capabilities.MachineLearning;
            // Create human segmentation model
            if (capabilities.HasFlag(Capabilities.HumanTexture) && humanSegmentationModel == null) {
                var mattekitData = await MLModelData.FromHub("@natml/mattekit");
                humanSegmentationModel = new MLEdgeModel(mattekitData);
            }
            // Create output
            if (capabilities.HasFlag(Capabilities.MachineLearning)) {
                var textureOutput = new TextureOutput();
                textureOutput.onFrame += OnCameraOutputFrame;
                output = textureOutput;
            } else {
                var renderTextureOutput = new RenderTextureOutput();
                renderTextureOutput.onFrame += OnCameraOutputFrame;
                output = renderTextureOutput;
            }
            // Play
            if (resolution != Resolution.Default)
                device.previewResolution = GetResolutionSize(resolution);
            device.StartRunning(OnCameraImage);
        }

        /// <summary>
        /// Stop the camera preview.
        /// </summary>
        public void StopRunning () {
            // Check
            if (!running)
                return;
            // Stop
            device?.StopRunning();
            output?.Dispose();
            Texture2D.Destroy(humanTexture);
            output = null;
            humanTexture = null;
        }
        #endregion


        #region --Operations--
        private CameraDevice device;
        private CameraOutput output;
        private MLEdgeModel humanSegmentationModel;
        private Texture2D humanTexture;
        private readonly Queue<CameraImage> imageQueue = new Queue<CameraImage>();

        private void Start () {
            if (playOnAwake)
                StartRunning();
        }

        private void OnCameraImage (CameraImage image) {
            output.Update(image);
            lock ((imageQueue as ICollection).SyncRoot)
                imageQueue.Enqueue(image);
        }

        private void OnCameraOutputFrame () {
            var (texture, timestamp) = GetOutputTexture(output);
            var image = GetOutputImage(texture, timestamp, imageQueue);
            humanTexture = CreateHumanTexture(texture, humanTexture);
            var frame = new CameraFrame(image, texture, humanTexture);
            OnFrame?.Invoke(frame);
        }

        private void OnDisable () {
            StopRunning();
            humanSegmentationModel?.Dispose();
            humanSegmentationModel = null;
        }

        private Texture2D CreateHumanTexture (Texture texture, Texture2D result = null) {
            // Check model
            if (humanSegmentationModel == null)
                return result;
            // Check frame
            if (!(texture is Texture2D frame))
                return result;
            // Predict
            var feature = new MLImageFeature(frame) { aspectMode = MLImageFeature.AspectMode.AspectFill };
            var inputType = humanSegmentationModel.inputs[0] as MLImageType;
            using var inputFeature = (feature as IMLEdgeFeature).Create(inputType);
            using var outputFeatures = humanSegmentationModel.Predict(inputFeature);
            var outputFeature = new MLArrayFeature<float>(outputFeatures[0]);
            var outputType = MLImageType.FromType(outputFeature.type);
            // Copy
            var width = outputType.width;
            var height = outputType.height;
            result = result ? result : new Texture2D(width, height, TextureFormat.RGBAFloat, false);
            if (result.width != width || result.height != height)
                result.Reinitialize(width, height);
            outputFeature.CopyTo(result.GetRawTextureData<float>());
            result.Apply();
            return result;
        }

        private static async Task<CameraDevice> CreateCameraDevice () {
            // Request camera permissions
            var permissionStatus = await MediaDeviceQuery.RequestPermissions<CameraDevice>();
            if (permissionStatus != PermissionStatus.Authorized) {
                Debug.LogError(@"VideoKit: User did not grant camera permissions");
                return null;
            }
            // Discover the front camera
            var query = new MediaDeviceQuery(MediaDeviceCriteria.CameraDevice);
            var device = query.FirstOrDefault(device => device is CameraDevice camera && camera.frontFacing) ?? query.current;
            // Check
            if (device == null) {
                Debug.LogError(@"VideoKit: Failed to discover any available camera devices");
                return null;
            }
            // Return
            return device as CameraDevice;
        }

        private static (Texture, long) GetOutputTexture (CameraOutput output) => output switch {
            TextureOutput textureOutput             => (textureOutput.texture, textureOutput.timestamp),
            RenderTextureOutput renderTextureOutput => (renderTextureOutput.texture, renderTextureOutput.timestamp),
            _                                       => (default, default),
        };

        private static CameraImage GetOutputImage (Texture texture, long timestamp, Queue<CameraImage> queue) {
            var texture2D = texture as Texture2D;
            lock ((queue as ICollection).SyncRoot)
                while (queue.TryDequeue(out var image))
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

        private static (int width, int height) GetResolutionSize (Resolution resolution) => resolution switch {
            Resolution.Lowest       => (176, 144),
            Resolution._640x480     => (640, 480),
            Resolution._1280x720    => (1280, 720),
            Resolution._1920x1080   => (1920, 1080),
            Resolution.Highest      => (5120, 2880),
            _                       => (1280, 720),
        };
        #endregion
    }
}