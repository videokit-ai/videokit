/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Samples {

    using UnityEngine;
    using UnityEngine.UI;
    using Unity.Collections;
    using VideoKit.UI;

    public sealed class PhotoCaptureSample : MonoBehaviour {
        
        [Header(@"Camera Control")]
        public VideoKitCameraManager cameraManager;
        public VideoKitCameraView cameraView;
        public CameraDevice.FlashMode flashMode;

        [Header(@"UI")]
        public RawImage photoView;

        public void CapturePhoto () {
            // Check that we are not streaming from a multi-camera device
            if (cameraManager.device is not CameraDevice cameraDevice) {
                Debug.LogError("Cannot capture photo because active camera is not a `CameraDevice`");
                return;
            }
            // Capture a photo
            cameraDevice.flashMode = flashMode;
            cameraDevice.CapturePhoto(OnCapturePhoto);
        }

        public void DismissPhoto () {
            // Destroy photo texture and hide photo panel
            Texture2D.Destroy(photoView.texture);
            photoView.texture = null;
            photoView.gameObject.SetActive(false);
        }

        private async void OnCapturePhoto (PixelBuffer pixelBuffer) {
            // Copy the photo into a RGBA32 buffer
            var rotation = cameraView.rotation;
            var portrait = 
                rotation == PixelBuffer.Rotation._90 ||
                rotation == PixelBuffer.Rotation._270;
            var width = portrait ? pixelBuffer.height : pixelBuffer.width;
            var height = portrait ? pixelBuffer.width : pixelBuffer.height;
            using var data = new NativeArray<byte>(width * height * 4, Allocator.Persistent);
            using var rgbaBuffer = new PixelBuffer(width, height, PixelBuffer.Format.RGBA8888, data);
            pixelBuffer.CopyTo(rgbaBuffer, rotation);
            // Create a texture on the Unity main thread
            await Awaitable.MainThreadAsync();
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.GetRawTextureData<byte>().CopyFrom(rgbaBuffer.data);
            texture.Apply();
            // Display photo
            photoView.gameObject.SetActive(true);
            photoView.texture = texture;
            var aspectFitter = photoView.GetComponent<AspectRatioFitter>();
            aspectFitter.aspectRatio = (float)texture.width / texture.height;
        }
    }
}