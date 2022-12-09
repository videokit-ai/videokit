/* 
*   VideoKit
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using Unity.Collections.LowLevel.Unsafe;
    using NatML.Devices;
    using NatML.Devices.Outputs;
    using ML;
    using Internal;

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
            /// 4K UHD resolution.
            /// </summary>
            _4K         = 5,
            /// <summary>
            /// Highest resolution supported by the camera device.
            /// Using this resolution is strongly not recommended.
            /// </summary>
            Highest     = 10
        }

        /// <summary>
        /// Camera manager capabilities.
        /// </summary>
        [Flags]
        public enum Capabilities : int {
            /// <summary>
            /// Stream depth data along with the camera preview data.
            /// This flag adds a minimal performance cost, so enable it only when necessary.
            /// This flag is only supported on iOS and Android.
            /// </summary>
            Depth           = 1,
            /// <summary>
            /// Ensure that the camera preview data can be used for machine learning predictions.
            /// Enabling this adds a minimal performance impact so it should be disabled when not needed.
            /// This flag is always enabled on WebGL.
            /// </summary>
            MachineLearning = 2,
            /// <summary>
            /// Generate a human segmentation texture from the camera preview stream.
            /// This flag adds a variable performance cost, so enable it only when necessary.
            /// </summary>
            HumanTexture    = 4 | Capabilities.MachineLearning,
            /// <summary>
            /// Detect human poses in the camera preview stream.
            /// This flag adds a variable performance cost, so enable it only when necessary.
            /// </summary>
            PoseDetection   = 8 | Capabilities.MachineLearning,
            /// <summary>
            /// Detect faces in the camera preview stream.
            /// This flag adds a variable performance cost, so enable it only when necessary.
            /// </summary>
            FaceDetection   = 16 | Capabilities.MachineLearning,
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
            if (!isActiveAndEnabled) {
                Debug.LogError(@"Cannot start running because component is disabled");
                return;
            }
            // Check
            if (running)
                return;
            // Get device
            device ??= await VideoKitUser.CreateCameraDevice();
            if (device == null) {
                Debug.LogError(@"VideoKit Error: Camera manager could not start because no camera device was available");
                return;
            }
            // Check capabilities
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                capabilities |= Capabilities.MachineLearning;
            // Create MatteKit
            if (capabilities.HasFlag(Capabilities.HumanTexture) && matteKit == null) {
                var matteKitData = await MLModelData.FromHub("@natml/mattekit");
                matteKit = new MatteKitPredictor(matteKitData);
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
            // Play
            if (resolution != Resolution.Default)
                device.previewResolution = GetResolutionSize(resolution);
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

        private void Start () {
            if (playOnAwake)
                StartRunning();
        }

        private void OnCameraImage (CameraImage image) {
            output.Update(image);
            lock ((imageQueue as ICollection).SyncRoot)
                imageQueue.Enqueue(image);
        }

        private void OnCameraTexture (TextureOutput output) {
            var image = GetOutputImage(output.texture, output.timestamp, imageQueue);
            var humanTexture = matteKit?.Predict(output.texture);
            var frame = new CameraFrame(image, output.texture, humanTexture);
            OnFrame?.Invoke(frame);
        }

        private void OnCameraTexture (RenderTextureOutput output) {
            var image = GetOutputImage(output.texture, output.timestamp, imageQueue);
            var frame = new CameraFrame(image, output.texture, null);
            OnFrame?.Invoke(frame);
        }

        private void OnDisable () => StopRunning();

        private void OnDestroy () => matteKit?.Dispose();

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
            Resolution._4K          => (3840, 2160),
            Resolution.Highest      => (5120, 2880),
            _                       => (1280, 720),
        };
        #endregion
    }
}