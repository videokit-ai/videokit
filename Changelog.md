## 0.0.4
+ Added `CropTextureInput` for recording a cropped area of the recording.
+ Added `WatermarkTextureInput` for adding a watermark to recorded videos.
+ Added `VideoKitRecorder.VideoMode.CameraDevice` video mode for recording videos directly from a camera device.
+ Added `VideoKitRecorder.destinationPathPrefix` property for specifying recording directory.
+ Added `VideoKitRecorder.Resolution._2K` resolution preset for recording at 2K WQHD.
+ Added `VideoKitRecorder.Resolution._4K` resolution preset for recording at 4K UHD.
+ Added `VideoKitCameraView.OnPresent` event to be notified when the view presents the camera preview to the user.
+ Added `VideoKitCameraFocus` UI component for focusing a camera device with tap gestures.
+ Fixed `CameraFrame.feature` property returnning new feature instance on every access.
+ Refactored `MicrophoneInput` class to `AudioDeviceInput`.
+ Refactored `VideoKitRecorder.AudioMode.Microphone` enumeration member to `AudioMode.AudioDevice`.

## 0.0.3
+ Fixed `NullReferenceException` when running camera with `Capabilities.MachineLearning` enabled.
+ Fixed rare crash when using running camera with `Capabilities.HumanTexture` enabled.
+ Fixed recording session not being ended when `VideoKitRecorder` component is disabled or destroyed.

## 0.0.2
+ Added `VideoKitCameraManager.Resolution.Default` resolution preset to leave camera resolution unchanged.
+ Added `VideoKitCameraManager.Capabilities.DepthTexture` enumeration member for streaming camera depth.
+ Added `MicrophoneInput` recorder input for recording audio frames from an `AudioDevice`.
+ Added implicit conversion from `CameraFrame` to `CameraImage`.
+ Fixed `CameraFrame.image` being uninitialized in `VideoKitCameraManager.OnFrame`.
+ Refactored `VideoKitCameraManager.Play` method to `StartRunning`.
+ Refactored `VideoKitCameraManager.Stop` method to `StopRunning`.
+ Removed `CameraFrame.width` property. Use `CameraFrame.image.width` instead.
+ Removed `CameraFrame.height` property. Use `CameraFrame.image.height` instead.
+ Removed `CameraFrame.pixelBuffer` property.
+ Removed `CameraFrame.timestamp` property.
+ Removed `VideoKitCameraManager.Capabilities.PixelData` enumeration member.

## 0.0.1
+ First pre-release.