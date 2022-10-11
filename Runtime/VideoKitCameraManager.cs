/* 
*   VideoKit
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit {

    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Events;
    using Unity.Collections.LowLevel.Unsafe;
    using NatML.Devices;
    using NatML.Devices.Outputs;
    using NatML.Features;
    using NatML.Internal;

    /// <summary>
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VideoKitCameraManager : MonoBehaviour {

        #region --Enumerations--
        /// <summary>
        /// Camera resolution presets.
        /// </summary>
        public enum Resolution {
            /// <summary>
            /// Lowest resolution supported by the camera device.
            /// </summary>
            Lowest      = 0,
            /// <summary>
            /// SD resolution.
            /// </summary>
            _640x480    = 1,
            /// <summary>
            /// HD resolution.
            /// </summary>
            _1280x720   = 2,
            /// <summary>
            /// Full HD resolution.
            /// </summary>
            _1920x1080  = 3,
            /// <summary>
            /// Highest resolution supported by the camera device.
            /// Using this resolution is strongly not recommended.
            /// </summary>
            Highest     = 4
        }

        /// <summary>
        /// Camera manager capabilities.
        /// </summary>
        [Flags]
        public enum Capabilities {
            /// <summary>
            /// Ensure that the camera preview data is available on the CPU in system memory.
            /// When this is disabled, the preview is available only on the GPU.
            /// Enabling this adds a minimal performance impact so it should be disabled when not needed.
            /// This flag is always enabled on WebGL.
            /// </summary>
            PixelData       = 1 << 0,
            /// <summary>
            /// Ensure that the camera preview data can be used for machine learning predictions.
            /// This flag implicitly enables the `PixelData` flag and adds no performance cost of its own.
            /// </summary>
            MachineLearning = (1 << 1) | Capabilities.PixelData,
            /// <summary>
            /// Generate a human segmentation texture while streaming the camera preview.
            /// This is useful for building certain types of video filters.
            /// This flag adds a variable performance cost, so enable it only when necessary.
            /// </summary>
            HumanTexture    = (1 << 2) | Capabilities.MachineLearning,
        }
        #endregion


        #region --Inspector--
        [Header(@"Preview")]
        /// <summary>
        /// Desired camera resolution.
        /// </summary>
        [Tooltip(@"The desired camera resolution.")]
        public Resolution resolution = Resolution._1280x720;

        /// <summary>
        /// Whether to start the camera preview as soon as the component awakes.
        /// </summary>
        [Tooltip(@"Whether to start the camera preview as soon as the component awakes.")]
        public bool playOnAwake = true;

        [Header(@"Configuration")]
        /// <summary>
        /// Desired camera manager capabilities.
        /// </summary>
        [Tooltip(@"Desired camera manager capabilities.")]
        public Capabilities capabilities;

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
                Stop();
                device = value;
                if (restart)
                    Play();
            }
        }

        /// <summary>
        /// Whether the camera is running.
        /// </summary>
        public bool running => textureOutput != null || renderTextureOutput != null;

        /// <summary>
        /// Start the camera preview.
        /// </summary>
        public async void Play () {
            // Check
            if (running)
                return;
            // Get device
            device ??= await GetDefaultCamera();
            if (device == null)
                return;
            // Check capabilities
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                capabilities |= Capabilities.PixelData;
            // Create mattekit
            if (capabilities.HasFlag(Capabilities.HumanTexture) && mattekit == null) {
                var mattekitData = await MLModelData.FromHub("@natml/mattekit");
                mattekit = new MLEdgeModel(mattekitData);
            }
            // Create output
            if (capabilities.HasFlag(Capabilities.PixelData))
                textureOutput = new TextureOutput();
            else
                renderTextureOutput = new RenderTextureOutput();
            // Play
            device.StartRunning(OnCameraImage);
        }

        /// <summary>
        /// Stop the camera preview.
        /// </summary>
        public void Stop () {
            // Check
            if (!running)
                return;
            // Stop
            device?.StopRunning();
            textureOutput?.Dispose();
            renderTextureOutput?.Dispose();
            Texture2D.Destroy(humanTexture);
            textureOutput = null;
            renderTextureOutput = null;
            humanTexture = null;
        }
        #endregion


        #region --Operations--
        private CameraDevice device;
        private MLEdgeModel mattekit;
        private RenderTextureOutput renderTextureOutput;
        private TextureOutput textureOutput;
        private Texture2D humanTexture;

        private void Start () {
            if (playOnAwake)
                Play();
        }

        private unsafe void Update () {
            if (!textureOutput?.texture && !renderTextureOutput?.texture)  
                return;
            var texture = capabilities.HasFlag(Capabilities.PixelData) ? (Texture)textureOutput.texture : renderTextureOutput.texture;
            var pixelBuffer = capabilities.HasFlag(Capabilities.PixelData) ? textureOutput.texture.GetRawTextureData<Color32>() : default;
            var feature = capabilities.HasFlag(Capabilities.MachineLearning) ? new MLImageFeature(textureOutput.texture) : null;
            if (capabilities.HasFlag(Capabilities.HumanTexture)) {
                humanTexture = humanTexture ? humanTexture : new Texture2D(texture.width, texture.height, TextureFormat.RGBAFloat, false);
                var size = texture.width * texture.height * 4 * sizeof(float);
                using var inputFeature = (new MLImageFeature(textureOutput.texture) as IMLEdgeFeature).Create(mattekit.inputs[0]);
                using var outputFeature = mattekit.Predict(inputFeature);
                UnsafeUtility.MemCpy(humanTexture.GetRawTextureData<float>().GetUnsafePtr(), outputFeature[0].data, size);
                humanTexture.Apply();
            }
            var frame = new CameraFrame(default, texture, pixelBuffer, feature, humanTexture);
            OnFrame?.Invoke(frame);
        }

        private void OnCameraImage (CameraImage image) {
            if (capabilities.HasFlag(Capabilities.PixelData))
                textureOutput.Update(image);
            else
                renderTextureOutput.Update(image);  
        }

        private void OnDisable () {
            Stop();
            mattekit?.Dispose();
        }

        private static async Task<CameraDevice> GetDefaultCamera () {
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
        #endregion
    }
}