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