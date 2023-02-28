## 0.0.11
+ Added GPU acceleration for background removal capability in `VideoKitCameraManager` on Android.
+ Added `VideoKitCameraManager.frameRate` property for setting the camera preview frame rate.
+ Added `VideoKitRecordButton` UI prefab for building recording UIs similar to Instagram.
+ Added `VideoKitRecorder.Destination.Playback` enumeration member for immediately playing back recorded media.
+ Added help URLs to VideoKit components in the Unity inspector.
+ Fixed `VideoKitCameraManager.device` property ignoring new values when the manager is not running.
+ Fixed sporadic crash when using `HumanTexture` capability with `VideoKitCameraManager`.
+ Fixed crash when creating a `WEBMRecorder` with audio on WebGL.
+ Removed `VideoKitRecorder.OrientationMode` enumeration.
+ Removed `VideoKitRecorder.AspectMode` enumeration.

## 0.0.10
+ Added support for realtime background removal using machine learning. [See the docs](https://docs.videokit.ai/videokit/workflows/background).
+ Added `MatteKitPredictor` for predicting a human texture from a given image.
+ Added `VideoKitCameraManager.facing` property for specifying a desired camera facing.
+ Added `VideoKitCameraManager.Facing` enumeration for specifying a desired camera facing.
+ Fixed camera preview being vertically mirrored when streaming the front camera on Android devices.
+ Refactored `VideoKitRecorder.Resolution._2K` enumeration member to `Resolution._2560xAuto`.
+ Refactored `VideoKitRecorder.Resolution._4K` enumeration member to `Resolution._3840xAuto`.
+ Refactored `VideoKitCameraManager.cameraDevice` property to `VideoKitCameraManager.device`.
+ Refactored `VideoKitAudioManager.audioDevice` property to `VideoKitAudioManager.device`.

## 0.0.9
+ Upgraded to NatML 1.1.

## 0.0.8
+ Added `VideoKitRecorder.Resolution.Custom` resolution preset for specifying custom recording resolution.
+ Added `VideoKitRecorder.customResolution` property for setting custom recording resolution.
+ Added `VideoKitCameraView.focusMode` setting for specifying how to handle camera focus gestures.
+ Added `VideoKitCameraView.exposureMode` setting for specifying how to handle camera exposure gestures.
+ Added `VideoKitCameraView.zoomMode` setting for specifying how to handle camera zoom gestures.
+ Fixed bug where VideoKit components could not be added in the Unity 2022 editor.
+ Removed `VideoKitCameraFocus` component. Use `VideoKitCameraView.focusMode` setting instead.
+ Removed `VideoKitCameraZoom` component. Use `VideoKitCameraView.zoomMode` setting instead.

## 0.0.7
+ Added `VideoKitRecorder.frameSkip` property for recording every `n` frames during recording.
+ Fixed `VideoKitRecorder.StartRecording` throwing error on Android with OpenGL ES3.
+ Fixed `VideoKitRecorder` exception when stopping recording session on WebGL.
+ Fixed `NullReferenceException` in `VideoKitRecorder` when stopping recording without `audioManager` assigned.
+ Refactored `VideoKitAudioManager.SampleRate._160000` to `SampleRate._16000`.

## 0.0.6
+ Added `VideoKitAudioManager` component for managing streaming audio from audio devices.
+ Added `VideoKitRecorder.RecordingSession` struct for receiving richer information about a completed recording session.
+ Added `VideoKitRecorder.audioManager` property for managing recording audio from audio devices.
+ Added `VideoKitRecorder.Resolution._320x240` resolution preset.
+ Added `VideoKitRecorder.Resolution._480x320` resolution preset.
+ Fixed `VideoKitRecorder` not allowing developer to select `Destination.PromptUser` destination.
+ Fixed `VideoKitRecorder` incorrect video size orientation when using `Resolution.Screen` and `Orientation.Portrait`.
+ Refactored `VideoKitRecorder.orientation` property to `VideoKitRecorder.orientationMode`.
+ Refactored `VideoKitRecorder.aspect` property to `VideoKitRecorder.aspectMode`.
+ Refactored `VideoKitRecorder.videoKeyframeInterval` property to `VideoKitRecorder.keyframeInterval`.
+ Refactored `VideoKitCameraManager.OnFrame` event to `OnCameraFrame`.
+ Removed `VideoKitRecorder.OnRecordingFailed` event. Use `OnRecordingCompleted` event instead.

## 0.0.5
+ Added `VideoKitRecorder.videoBitRate` property for specifying the video bitrate for applicable formats.
+ Added `VideoKitRecorder.videoKeyframeInterval` property for specifying the keyframe interval for applicable formats.
+ Added `VideoKitRecorder.audioBitRate` property for specifying the audio bitrate for applicable formats.

## 0.0.4
+ Added `CropTextureInput` for recording a cropped area of the recording.
+ Added `WatermarkTextureInput` for adding a watermark to recorded videos.
+ Added `VideoKitRecorder.VideoMode.CameraDevice` video mode for recording videos directly from a camera device.
+ Added `VideoKitRecorder.destinationPathPrefix` property for specifying recording directory.
+ Added `VideoKitRecorder.Resolution._2K` resolution preset for recording at 2K WQHD.
+ Added `VideoKitRecorder.Resolution._4K` resolution preset for recording at 4K UHD.
+ Added `VideoKitCameraView.OnPresent` event to be notified when the view presents the camera preview to the user.
+ Added `VideoKitCameraFocus` UI component for focusing a camera device with tap gestures.
+ Fixed `CameraFrame.feature` property returning new feature instance on every access.
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