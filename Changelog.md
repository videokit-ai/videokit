## 0.0.23
+ Improved camera streaming performance especially when streaming with high frame rate or with human texture.
+ Added `MultiCameraDevice` class for streaming pixel buffers from multiple camera devices simultaneously (#140).
+ Added `VideoKitCameraManager.facingRequired` field to require a camera device with the requested facing instead of using a fallback camera device.
+ Added support for frame skipping in `VideoKitRecorder` when `videoMode` is set to `VideoMode.CameraDevice`.
+ Fixed watermarks not being applied when using `TextureSource.Append` method (#149).
+ Fixed `VideoKitCameraManager.StartRunning` method crashing on some Android devices when `Capabilities.HumanTexture` capability is enabled (#157).
+ Fixed missing native library errors in Xcode when building universal macOS binary (#154).
+ Fixed human texture predictor embedding errors causing Unity build to fail (#156).
+ Updated `MediaAsset.Share` method to return `null` instead of throw a `NotImplementedException` on platforms where it is not supported.
+ Updated `MediaAsset.SaveToCameraRoll` method to return `false` instead of throw a `NotImplementedException` on platforms where it is not supported.
+ Refactored `AudioBuffer.sampleBuffer` field to `AudioBuffer.data`.
+ Refactored `TextureSource.cropRect` field to `regionOfInterest`.
+ Refactored `VideoKitViewManager.exposureMode` field to `exposureGesture`.
+ Refactored `VideoKitViewManager.focusMode` field to `focusGesture`.
+ Refactored `VideoKitViewManager.zoomMode` field to `zoomGesture`.
+ Refactored `VideoKitCameraManager.Facing.RequireUser` enumeration member to `Facing.User`.
+ Refactored `VideoKitCameraManager.Facing.PreferWorld` enumeration member to `Facing.World`.
+ Removed `VideoKitCameraManager.Facing.RequireWorld` enumeration member. Use `Facing.World` enumeration member instead.
+ Removed `VideoKitCameraManager.Facing.PreferUser` enumeration member. Use `Facing.User` enumeration member instead.
+ Removed `VideoKitCameraManager.OnCameraFrame` event. Use `VideoKitCameraView.OnCameraFrame` event instead.
+ Removed `VideoKitCameraManager.texture` field. Use `VideoKitCameraView.texture` field instead.
+ Removed `VideoKitCameraManager.humanTexture` field. Use `VideoKitCameraView.texture` field with `VideoKitCameraView.ViewMode.HumanTexture` view mode instead.
+ Removed `VideoKitCameraManager.rotation` field. Use `VideoKitCameraView.rotation` field instead.
+ Removed `VideoKitRecorder.cameraManager` field. Use `VideoKitRecorder.cameraView` field instead.

## 0.0.22
+ Improved native error propagation into C# code allowing you to `try..catch` more errors.
+ Improved recording performance when using `VideoMode.CameraDevice` with the `VideoKitRecorder` component.
+ Unsealed the `MediaRecorder` class, allowing for custom derived classes to override recording behaviour.
+ Added `VideoKitCameraManager.StartRunningAsync` method which returns a `Task` for async completion and error handling.
+ Added `VideoKitAudioManager.StartRunningAsync` method which returns a `Task` for async completion and error handling.
+ Added `VideoKitRecorder.StartRecordingAsync` method which returns a `Task` for async completion and error handling.
+ Added `VideoKitRecorder.StopRecordingAsync` method which returns a `Task` for async completion and error handling.
+ Fixed `CameraDevice.StopRunning` method causing app hanging when camera device is unsupported (#126).
+ Fixed `MediaAsset.FromCameraRoll` method causing app crash when the user does not pick an asset on Android (#146).
+ Fixed `MediaAsset.FromCameraRoll` task never completing when user does not pick an asset on iOS (#146).
+ Fixed camera preview not resuming after app is suspended for more than a few seconds on Android (#56).
+ Refactored `MediaAsset.Caption<T>` method to `MediaAsset.Parse<T>` for structured parsing.
+ Refactored `MediaAsset.Caption` method to `MediaAsset.Transcribe` for performing speech-to-text.
+ Refactored `MediaAsset.FromSpeechPrompt` method to `MediaAsset.FromGeneratedSpeech` for performing text-to-speech.
+ Removed `PixelBuffer.Region` method.
+ Removed `PixelBuffer.ToImage` method.
+ Removed video recording in the free tier. Subscribe to a VideoKit plan to use video recording.
+ VideoKit now requires Unity 6 when building for WebGL (#150).
+ VideoKit now requires iOS 14+.
+ VideoKit now requires macOS 12+.

## 0.0.21
+ Fixed `WebException: The request was aborted: The request was canceled` when building for Android.

## 0.0.20
+ Added support for streaming from USB UVC cameras on iOS and iPadOS (#135).
+ Added `VideoKitRecorder.CaptureScreenshot` method for capturing a screenshot image to a `JPEG` media asset (#132).
+ Fixed unrecoverable error when recording `webm` videos on WebGL.
+ Fixed sporadic crash when stopping recording from camera device on iOS (#131).
+ Fixed Unity Editor freezing when recording video with audio (#128).
+ Refactored `MediaType` enum to `MediaAsset.MediaType`.
+ Refactored `MediaAsset.Narrate` instance method to `MediaAsset.FromSpeechPrompt` static method.
+ Removed `PixelBuffer.CopyTo(PixelBuffer, PixelBuffer)` method overload for alpha blending images.

## 0.0.19
+ Added more descriptive resolution enum labels in `VideoKitRecorder` and `VideoKitCameraManager` (#130).
+ Fixed VideoKit not functioning on iOS due to internet unreachability errors (#127).
+ Fixed `Some items were not destroyed` error when exiting play mode in the Unity Editor.
+ Fixed `ArgumentException` error when recording videos on the free plan (#129).

## 0.0.18
+ Added experimental support for recording Apple ProRes4444 with Alpha on iOS and macOS (#89).
+ Added experimental support for recording AV1 videos on Android 14+.
+ Added experimental support for visionOS (#109).
+ Added `PixelBuffer` struct for working with pixel buffers.
+ Added `AudioBuffer` struct for working with audio buffers.
+ Added `MediaRecorder.Append(PixelBuffer)` method to append a video frame to a recorder.
+ Added `MediaRecorder.Append(AudioBuffer)` method to append an audio frame to a recorder.
+ Added `MediaRecorder.width` property for getting the recorder video width.
+ Added `MediaRecorder.height` property for getting the recorder video height.
+ Added `MediaRecorder.sampleRate` property for getting the recorder audio sample rate.
+ Added `MediaRecorder.channelCount` property for getting the recorder audio channel count.
+ Added `MediaRecorder.CanCreate` static method to check whether a recorder with the given format can be created.
+ Added `MediaRecorder.CanAppend<T>` static method to check whether the given format supports encoding `T` buffers.
+ Added `CameraDevice.depthStreamingSupported` property to check whether a camera device supports streaming depth.
+ Added `VideoKitRecorder.AudioMode.AudioSource` enumeration member for recording audio from an in-game audio source.
+ Added `VideoKitRecorder.audioListener` field for specifying an audio listener to record audio from.
+ Added `VideoKitRecorder.audioSource` field for specifying an audio source to record audio from.
+ Added `MediaAsset.assets` property for inspecting child assets of sequence assets.
+ Added `MediaAsset.Read` method for reading pixel buffers or audio buffers from a media asset (#103).
+ Added `MediaAsset.FromStreamingAssets` static method for creating a media asset from a file in streaming assets.
+ Added support for recording 120FPS and 240FPS videos with `VideoKitRecorder` component on iOS.
+ Added `PrivacyInfo.xcprivacy` iOS privacy manifest in `VideoKit.framework` (#118).
+ Improved MP4 and HEVC recording performance on Windows.
+ Drastically reduced Android native library size from 9.3MB to 3.7MB.
+ Fixed crash when calling `MediaDevice.CheckPermissions` method on Android (#83).
+ Fixed `CameraDevice.focusPointSupported` property always returning `false` on Android (#93).
+ Fixed front `CameraDevice` preview being wrongly flipped on certain Android devices (#45).
+ Fixed recording failures for long recordings due to timestamp drift (#67).
+ Fixed `ArgumentNullException` when setting the `VideoKitCameraManager.facing` before `StartRunning` is called.
+ Fixed intermittent crash when ending video recording with microphone audio on Android (#101).
+ Fixed bug in `MediaAsset.Share` method where a provided message is not shared with the asset.
+ Fixed Android manifest merging error when using other libraries that support native sharing.
+ Fixed Android Live Wallpaper crash in apps that use VideoKit (#94, #96).
+ Fixed potential crash on failed async GPU readback (thanks Clemens!).
+ Updated `RealtimeClock` clock to use native high-resolution, monotonic clock for more precise timestamps (#67).
+ Updated default `videoBitRate` in `VideoKitRecorder` and `MediaRecorder` classes to 20 megabits per second (#88).
+ Refactored `MediaDevice.UniqueID` field to `uniqueId`.
+ Refactored `MediaDevice.ExposureModeSupported` method to `MediaDevice.IsExposureModeSupported`.
+ Refactored `MediaDevice.FocusModeSupported` method to `MediaDevice.IsFocusModeSupported`.
+ Refactored `MediaDevice.WhiteBalanceModeSupported` method to `MediaDevice.IsWhiteBalanceModeSupported`.
+ Refactored `MediaDevice.VideoStabilizationModeSupported` method to `MediaDevice.IsVideoStabilizationModeSupported`.
+ Refactored `MediaRecorder.FinishWriting` method to return `Task<MediaAsset>` instead of `Task<string>`.
+ Refactored `VideoKit.Assets.MediaAsset` class to `VideoKit.MediaAsset`.
+ Refactored `VideoKit.Devices.MediaDevice` class to `VideoKit.MediaDevice`.
+ Refactored `VideoKit.Devices.CameraDevice` class to `VideoKit.CameraDevice`.
+ Refactored `VideoKit.Devices.AudioDevice` class to `VideoKit.AudioDevice`.
+ Refactored `VideoKit.Recorders.Clocks.IClock` interface to `VideoKit.Clocks.IClock`.
+ Refactored `VideoKit.Recorders.Clocks.FixedIntervalClock` class to `VideoKit.Clocks.FixedClock`.
+ Refactored `VideoKit.Recorders.Clocks.RealtimeClock` class to `VideoKit.Clocks.RealtimeClock`.
+ Refactored `VideoKit.MediaFormat` enumeration to `VideoKit.Recorders.MediaRecorder.Format`.
+ Refactored `VideoKit.Assets.MediaAsset.AssetType` enumeration to `VideoKit.MediaAsset.MediaType`.
+ Refactored `VideoKit.Recorders.Inputs.AudioInput` class to `VideoKit.Sources.SceneAudioSource`.
+ Refactored `VideoKit.Recorders.Inputs.CameraInput` class to `VideoKit.Sources.SceneCameraSource`.
+ Refactored `VideoKit.Recorders.Inputs.ScreenInput` class to `VideoKit.Sources.ScreenSource`.
+ Refactored `VideoKit.MediaAsset` class to no longer implement `IEnumerable<MediaAsset>`.
+ Removed `AudioAsset` class. Use the `MediaAsset` class instead.
+ Removed `ImageAsset` class. Use the `MediaAsset` class instead.
+ Removed `VideoAsset` class. Use the `MediaAsset` class instead.
+ Removed `TextAsset` class. Use the `MediaAsset` class instead.
+ Removed `MediaSequenceAsset` class. Use the `MediaAsset` class instead.
+ Removed `VideoKit.Devices.CameraImage` struct. Use `VideoKit.PixelBuffer` struct instead.
+ Removed `VideoKit.Devices.AudioBuffer` struct. Use `VideoKit.AudioBuffer` struct instead.
+ Removed `TextureInput` class. Use `PixelBuffer.FromTexture` static method instead.
+ Removed `AsyncTextureInput` class. Use `PixelBuffer.FromTexture` static method instead.
+ Removed `GLESTextureInput` class. Unity now supports performing async GPU readbacks on OpenGLES 3+.
+ Removed `CropTextureInput` class. Use `TextureSource.cropRect` property to crop texture being recorded.
+ Removed `WatermarkTextureInput` class. Use `TextureSource.watermark` and `TextureSource.watermarkRect` properties to watermark texture being recorded.
+ Removed `VideoKitExtensions` class.
+ Removed `VideoKitCameraManager.image` property.
+ Removed `VideoKitCameraManager.imageFeature` property.
+ Removed `VideoKitCameraManager.pixelBuffer` property. Use `VideoKitCameraManager.texture.GetRawTextureData<T>` method.
+ Removed `VideoKitCameraManager.Capabilities.AI` enumeration member because it is now always enabled.
+ Removed `VideoKitCameraManager.Capabilities.FaceDetection` enumeration member.
+ Removed `VideoKitCameraManager.Capabilities.PoseDetection` enumeration member.
+ Removed `VideoKitRecorder.RecorderAction.Delete` enumeration member. Manually delete the media asset if desired.
+ Removed `MediaRecorder.frameSize` property. Use `MediaRecorder.width` and `MediaRecorder.height` properties instead.
+ Removed `CameraOutput.image` property.
+ Removed `AudioOutput.buffer` property.
+ Removed `Image.device` property.
+ Removed `AudioBuffer.device` property.
+ Removed `Image.Clone` method.
+ Removed `AudioBuffer.Clone` method.
+ Removed `MediaRecorder.CommitFrame` method. Use `MediaRecorder.Append(Image)` method instead.
+ Removed `MediaRecorder.CommitSamples` method. Use `MediaRecorder.Append(AudioBuffer)` method instead.

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