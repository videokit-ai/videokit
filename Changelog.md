## 0.0.17
+ Fixed C# compiler error on iOS.

## 0.0.16
+ Added 32-bit `x86` architecture support on Android (#75).
+ Fixed `NullReferenceException` on startup when running on Windows (#78).
+ Fixed crash when calling `MediaAsset.SaveToCameraRoll` on macOS (#85).
+ Changed `VideoKitAudioManager.OnAudioBuffer` event type from `UnityEvent` to a plain `event` (#69).
+ Removed `MediaDeviceFilters` class.
+ Reduced minimum requirement to macOS 10.15 (#84).

## 0.0.15
+ Added `MediaAsset.ToValue` method for creating Function prediction values from media assets.
+ Added `VideoKitAudioManager.OnAudioBuffer` event for receiving audio buffers from audio devices.
+ Added `VideoKitCameraManager.OnCameraImage` event for receiving camera images directly from the streaming camera device.
+ Added `VideoKitCameraManager.texture` property for accessing the camera preview texture.
+ Added `VideoKitCameraManager.pixelBuffer` property for accessing the camera preview pixel buffer.
+ Added `VideoKitCameraManager.humanTexture` property for accessing the camera human texture.
+ Added `VideoKitCameraManager.imageFeature` property for accessing the camera preview as an ML feature.
+ Added `VideoKitRecordButton.OnStartRecording` event.
+ Added `VideoKitRecordButton.OnStopRecording` event.
+ Fixed `MediaAsset.path` property containing invalid characters on Windows.
+ Fixed `MediaAsset.Share` task never completing when exception is raised on Android.
+ Fixed `MediaAsset.Share` failing for apps that use Vuforia on Android.
+ Fixed `MediaAsset.SaveToCameraRoll` method failing because of missing write permissions on older versions of Android.
+ Fixed `MediaAsset.FromFile` method failing on WebGL due to URL mishandling.
+ Fixed `CameraDevice.WhiteBalanceModeSupported` always returning false for `WhiteBalanceMode.Continuous` on Android.
+ Fixed `CameraDevice.videoStabilizationMode` getter property causing hard crash on some Android devices.
+ Fixed `DllNotFoundException` when importing VideoKit in Linux editor.
+ Fixed rare crash due to frame rate setting when `CameraDevice.Discover` is invoked.
+ Fixed rare crash when recording is started when rendering with OpenGL ES3 on Android.
+ Fixed rare crash when entering play mode in the Unity Editor because the app domain is reloaded.
+ Removed `IMediaOutput` interface.
+ Removed `SampleBuffer` struct. Use `AudioBuffer` struct instead.
+ Removed `VideoKitAudioManager.OnSampleBuffer` event. Use `OnAudioBuffer` event instead.
+ Removed `CameraImage` parameter from `VideoKitCameraManager.OnCameraFrame` event.

## 0.0.14
+ Added audio captioning using AI with the `AudioAsset.Caption` method.
+ Added ability to parse an arbitrary `struct` from text using AI with the `TextAsset.To` method.
+ Added ability to pick images and videos from the camera roll with the `MediaAsset.FromCameraRoll<T>` method.
+ Added `MediaAsset` class for loading, inspecting, and sharing media.
+ Added `TextAsset` class for loading, inspecting, and extracting models from text.
+ Added `ImageAsset` class for loading, modifying, and sharing images.
+ Added `VideoAsset` class for loading, inspecting, and sharing videos.
+ Added `AudioAsset` class for loading, inspecting, and sharing audio.
+ Added `MediaRecorder` class to consolidate working with recorders.
+ Added `MediaFormat` enumeration for identifying and working with media formats.
+ Added `AudioDevice.Discover` static method for discovering available microphones.
+ Added `CameraDevice.Discover` static method for discovering available cameras.
+ Added `CameraDevice.exposureDuration` property to get the current camera exposure duration in seconds.
+ Added `CameraDevice.ISO` property to get the current camera exposure sensitivity.
+ Added `VideoKitProjectSettings` class for managing VideoKit settings in the current Unity project.
+ Added `VideoKitRecorder.frameRate` property for setting the frame rate of recorded GIF images.
+ Added `VideoKitRecordButton.recorder` property for getting and setting the recorder on which the button acts.
+ Added automatic camera pausing and resuming when app is suspended and resumed in `VideoKitCameraManager`.
+ Added native sharing support on macOS.
+ Added native sharing support on WebGL for browsers that are WebShare compliant.
+ Fixed `VideoKitRecorder.Resolution._240xAuto`, `_720xAuto`, and `_1080xAuto` constants resulting in incorrect resolutions.
+ Fixed visible artifacts when recording camera that only clears depth or doesn't clear at all (#32).
+ Fixed camera permissions not being requested when calling `CameraDevice.CheckPermissions` on fresh Android app install.
+ Fixed `CameraDevice` preview stream being frozen in the Safari browser on macOS.
+ Fixed `CameraDevice` focus being lost when setting `FocusMode.Locked` on Android.
+ Fixed `mimeType not supported` exception when creating a `WEBMRecorder` in the Safari browser.
+ Fixed `std::bad_function_call` exception when `AudioDevice.StopRunning` is called on WebGL.
+ Fixed `CommitFrame` exception when recording audio to a `WAV` file with `VideoKitRecorder` class.
+ Fixed media preview in native share UI not showing when sharing an image or video on Android.
+ Updated `VideoKitCameraManager.StartRunning` method to return a `Task` that can be awaited.
+ Updated `VideoKitAudioManager.StartRunning` method to return a `Task` that can be awaited.
+ Updated `VideoKitRecorder.StartRecording` method to return a `Task` that can be awaited.
+ Updated `JPEGRecorder.FinishWriting` to return path to all recorded image files separated by `Path.PathSeparator` character.
+ Refactored `IMediaDevice` interface to `MediaDevice` class.
+ Refactored `MediaDeviceCriteria` class to `MediaDeviceFilters`.
+ Refactored `DeviceLocation` enumeration to `MediaDevice.Location`.
+ Refactored `PermissionStatus` enumeration to `MediaDevice.PermissionStatus`.
+ Refactored `VideoKitCameraManager.Capabilities.MachineLearning` enumeration member to `Capabilities.AI`.
+ Removed `IMediaRecorder` interface. Use `MediaRecorder` class instead.
+ Removed `MP4Recorder` class. Use `MediaRecorder.Create` with `MediaFormat.MP4` instead.
+ Removed `HEVCRecorder` class. Use `MediaRecorder.Create` with `MediaFormat.HEVC` instead.
+ Removed `GIFRecorder` class. Use `MediaRecorder.Create` with `MediaFormat.GIF` instead.
+ Removed `WAVRecorder` class. Use `MediaRecorder.Create` with `MediaFormat.WAV` instead.
+ Removed `WEBMRecorder` class. Use `MediaRecorder.Create` with `MediaFormat.WEBM` instead.
+ Removed `JPEGRecorder` class. Use `MediaRecorder.Create` with `MediaFormat.JPEG` instead.
+ Removed `MediaDeviceQuery` class. Use `AudioDevice.Discover` and `CameraDevice.Discover` methods.
+ Removed `SharePayload` class. Use `MediaAsset.Share` method instead.
+ Removed `SavePayload` class. Use `MediaAsset.SaveToCameraRoll` method instead.
+ Removed `AudioSpectrumOutput` class.
+ Removed `IEquatable` interface inheritance from `MediaDevice` class.
+ Removed `AudioDevice.Equals` method as audio devices no longer define a custom equality method.
+ Removed `CameraDevice.Equals` method as camera devices no longer define a custom equality method.
+ Removed `VideoKitRecorder.frameDuration` property. Use `VideoKitRecorder.frameRate` property instead.
+ Removed `VideoKitRecorder.Format` enumeration. Use `MediaFormat` enumeration instead.
+ Removed `VideoKitRecordButton.OnTouchDown` event.
+ Removed `VideoKitRecordButton.OnTouchUp` event.
+ Updated top-level namespace from `NatML.VideoKit` to `VideoKit`.
+ VideoKit now requires iOS 13+.
+ VideoKit now requires macOS 11+.

## 0.0.13
+ Fixed crash when rapidly switching cameras on WebGL (#23).
+ Fixed rare memory exception when discovering audio devices on WebGL (#24).
+ Fixed `VideoKitCameraManager.StopRunning` not stopping camera device on Safari (#25).

## 0.0.12
+ Fixed resolution and frame rate settings not being set when restarting `VideoKitCameraManager` (#19).
+ Fixed `VideoKitCameraManager` error when switching scenes in WebGL (#17).
+ Fixed `VideoKitRecorder.prepareOnAwake` setting still causing stutter on first recording (#20).

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