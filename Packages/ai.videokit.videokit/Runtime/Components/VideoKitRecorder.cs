/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.Serialization;
    using Unity.Collections;
    using Clocks;
    using Sources;
    using UI;
    using MediaFormat = MediaRecorder.Format;
    using MediaType = MediaAsset.MediaType;

    /// <summary>
    /// VideoKit recorder for recording videos.
    /// </summary>
    [Tooltip(@"VideoKit recorder for recording videos.")]
    [HelpURL(@"https://videokit.ai/reference/videokitrecorder")]
    [DisallowMultipleComponent]
    public sealed partial class VideoKitRecorder : MonoBehaviour {

        #region --Enumerations--
        /// <summary>
        /// Video recording mode.
        /// </summary>
        public enum VideoMode : int {
            /// <summary>
            /// Don't record video.
            /// </summary>
            None            = 0,
            /// <summary>
            /// Record video frames from one or more game cameras.
            /// </summary>
            Camera          = 1,
            /// <summary>
            /// Record video frames from the screen.
            /// </summary>
            Screen          = 2,
            /// <summary>
            /// Record video frames from a texture.
            /// </summary>
            Texture         = 3,
            /// <summary>
            /// Record video frames from a camera device.
            /// </summary>
            CameraDevice    = 4,
        }

        /// <summary>
        /// Audio recording mode.
        /// </summary>
        public enum AudioMode : int {
            /// <summary>
            /// Don't record audio.
            /// </summary>
            None            = 0b0000,
            /// <summary>
            /// Record audio from an audio device.
            /// </summary>
            AudioDevice     = 0b0010,
            /// <summary>
            /// Record audio from an audio listener.
            /// </summary>
            AudioListener   = 0b0001,
            /// <summary>
            /// Record audio from an audio source.
            /// </summary>
            AudioSource     = 0b0100,
        }

        /// <summary>
        /// Video recording resolution presets.
        /// </summary>
        public enum Resolution : int {
            /// <summary>
            /// QVGA resolution.
            /// </summary>
            _240xAuto   = 11,
            /// <summary>
            /// QVGA resolution.
            /// </summary>
            _320xAuto   = 5,
            /// <summary>
            /// Portrait SD resolution.
            /// </summary>
            [InspectorName(@"480p Portrait")]
            _480xAuto   = 6,
            /// <summary>
            /// SD resolution.
            /// </summary>
            [InspectorName(@"480p Landscape")]
            _640xAuto   = 0,
            /// <summary>
            /// Potrait HD resolution.
            /// </summary>
            [InspectorName(@"720p Portrait")]
            _720xAuto   = 7,
            /// <summary>
            /// HD resolution.
            /// </summary>
            [InspectorName(@"720p Landscape")]
            _1280xAuto  = 1,
            /// <summary>
            /// Portrait Full HD resolution.
            /// </summary>
            [InspectorName(@"1080p Portrait")]
            _1080xAuto  = 12,
            /// <summary>
            /// Full HD resolution.
            /// </summary>
            [InspectorName(@"1080p Landscape")]
            _1920xAuto  = 2,
            /// <summary>
            /// Portrait 2K WQHD resolution.
            /// </summary>
            [InspectorName(@"2K Portrait")]
            _1440xAuto = 13,
            /// <summary>
            /// 2K WQHD resolution.
            /// </summary>
            [InspectorName(@"2K Landscape")]
            _2560xAuto  = 3,
            /// <summary>
            /// Portrait 4K UHD resolution.
            /// </summary>
            [InspectorName(@"4K Portrait")]
            _2160xAuto = 14,
            /// <summary>
            /// 4K UHD resolution.
            /// </summary>
            [InspectorName(@"4K Landscape")]
            _3840xAuto  = 4,
            /// <summary>
            /// Screen resolution.
            /// </summary>
            Screen      = 9,
            /// <summary>
            /// Half of the screen resolution.
            /// </summary>
            HalfScreen  = 10,
            /// <summary>
            /// Custom resolution.
            /// </summary>
            Custom      = 8,
        }

        /// <summary>
        /// Recorder status.
        /// </summary>
        public enum Status : int {
            /// <summary>
            /// No recording session is in progress.
            /// </summary>
            Idle        = 0,
            /// <summary>
            /// Recording session is in progress.
            /// </summary>
            Recording   = 1,
            /// <summary>
            /// Recording session is in progress but is paused.
            /// </summary>
            Paused      = 2,
        }

        /// <summary>
        /// Video watermark mode.
        /// </summary>
        public enum WatermarkMode : int {
            /// <summary>
            /// No watermark.
            /// </summary>
            None        = 0,
            /// <summary>
            /// Place watermark in the bottom-left of the frame.
            /// </summary>
            BottomLeft  = 1,
            /// <summary>
            /// Place watermark in the bottom-right of the frame.
            /// </summary>
            BottomRight = 2,
            /// <summary>
            /// Place watermark in the upper-left of the frame.
            /// </summary>
            UpperLeft   = 3,
            /// <summary>
            /// Place watermark in the upper-right of the frame.
            /// </summary>
            UpperRight  = 4,
            /// <summary>
            /// Place watermark in a user-defined rectangle.
            /// Set the rect with the `watermarkRect` property.
            /// </summary>
            Custom      = 5,
        }

        /// <summary>
        /// Recording action.
        /// </summary>
        [Flags]
        public enum RecordingAction : int {
            /// <summary>
            /// Nothing.
            /// </summary>
            None        = 0,
            /// <summary>
            /// Save the media asset to the camera roll.
            /// </summary>
            CameraRoll  = 1 << 1,
            /// <summary>
            /// Prompt the user to share the media asset with the native sharing UI.
            /// </summary>
            Share       = 1 << 2,
            /// <summary>
            /// Playback the video with the platform default media player.
            /// </summary>
            Playback    = 1 << 3,
            /// <summary>
            /// Define a custom callback to receive the media asset.
            /// NOTE: This is mutually exclusive with all other recording actions.
            /// </summary>
            Custom      = 1 << 5,
        }
        #endregion


        #region --Inspector--
        [Header(@"Format")]
        /// <summary>
        /// Recording format.
        /// </summary>
        [Tooltip(@"Recording format.")]
        public MediaFormat format = MediaFormat.MP4;

        /// <summary>
        /// Prepare the hardware encoders on awake.
        /// This prevents a noticeable stutter that occurs on the very first recording.
        /// </summary>
        [Tooltip(@"Prepare the hardware encoders on awake. This prevents a noticeable stutter that occurs on the very first recording.")]
        public bool prepareOnAwake = false;

        [Header(@"Video")]
        /// <summary>
        /// Video recording mode.
        /// </summary>
        [Tooltip(@"Video recording mode.")]
        public VideoMode videoMode = VideoMode.Camera;

        /// <summary>
        /// Video recording resolution.
        /// NOTE: This is not supported with `VideoMode.CameraDevice`.
        /// </summary>
        [Tooltip(@"Video recording resolution.")]
        public Resolution resolution = Resolution._1280xAuto;

        /// <summary>
        /// Video recording custom resolution.
        /// NOTE: This is only used when `resolution` is set to `Resolution.Custom`.
        /// </summary>
        [Tooltip(@"Video recording custom resolution.")]
        public Vector2Int customResolution = new Vector2Int(1280, 720);

        /// <summary>
        /// Game cameras to record.
        /// </summary>
        [Tooltip(@"Game cameras to record.")]
        public Camera[] cameras = new Camera[0];

        /// <summary>
        /// Recording texture for recording video frames from a texture.
        /// </summary>
        [Tooltip(@"Recording texture for recording video frames from a texture.")]
        public Texture? texture;

        /// <summary>
        /// Camera view for recording video frames from a camera device.
        /// </summary>
        [Tooltip(@"Camera view for recording video frames from a camera device.")]
        public VideoKitCameraView? cameraView;

        /// <summary>
        /// Frame rate for animated GIF images.
        /// This only applies when recording GIF images.
        /// </summary>
        [Tooltip(@"Frame rate for animated GIF images."), Range(5f, 30f), FormerlySerializedAs(@"frameRate")]
        public float _frameRate = 10f;

        /// <summary>
        /// Number of successive camera frames to skip while recording.
        /// </summary>
        [Tooltip(@"Number of successive camera frames to skip while recording."), Range(0, 5)]
        public int frameSkip = 0;

        [Header(@"Watermark")]
        /// <summary>
        /// Recording watermark mode for adding a watermark to videos.
        /// NOTE: Watermarking is not supported with the `VideoMode.CameraDevice` video mode.
        /// </summary>
        [Tooltip(@"Recording watermark mode for adding a watermark to videos.")]
        public WatermarkMode watermarkMode = WatermarkMode.None;

        /// <summary>
        /// Recording watermark.
        /// </summary>
        [SerializeField, FormerlySerializedAs(@"watermark"), Tooltip(@"Recording watermark.")]
        private Texture? _watermark;

        /// <summary>
        /// Watermark display rect when `watermarkMode` is set to `WatermarkMode.Custom`.
        /// </summary>
        [SerializeField, FormerlySerializedAs(@"watermarkRect"), Tooltip(@"Watermark display rect when `watermarkMode` is set to `WatermarkMode.Custom`")]
        private Rect _watermarkRect;

        [Header(@"Audio")]
        /// <summary>
        /// Audio recording mode.
        /// </summary>
        [Tooltip(@"Audio recording mode.")]
        public AudioMode audioMode = AudioMode.None;

        /// <summary>
        /// Audio manager for recording audio from an audio device.
        /// </summary>
        [Tooltip(@"Audio manager for recording audio from an audio device.")]
        public VideoKitAudioManager? audioManager;

        /// <summary>
        /// Whether the recorder can configure the audio manager for recording.
        /// Unless you intend to override the audio manager configuration, leave this `true`.
        /// </summary>
        [Tooltip(@"Whether the recorder can configure the audio manager for recording.")]
        public bool configureAudioManager = true;

        /// <summary>
        /// Audio listener for recording audio from an audio listener.
        /// </summary>
        [Tooltip(@"Audio listener for recording audio from an audio listener.")]
        public AudioListener? audioListener;

        /// <summary>
        /// Audio source for recording audio from an audio source.
        /// </summary>
        [Tooltip(@"Audio source for recording audio from an audio source.")]
        public AudioSource? audioSource;

        [Header(@"Recording")]
        /// <summary>
        /// Recording action.
        /// </summary>
        [Tooltip(@"Recording action.")]
        public RecordingAction recordingAction = 0;

        /// <summary>
        /// Event raised when a recording session is completed.
        /// </summary>
        [Tooltip(@"Event raised when a recording session is completed.")]
        public UnityEvent<MediaAsset>? OnRecordingCompleted;
        #endregion


        #region --Client API--
        /// <summary>
        /// Recording path prefix when saving recordings to the app's documents.
        /// </summary>
        [HideInInspector]
        public string mediaPathPrefix = @"recordings";

        /// <summary>
        /// Video bit rate in bits per second.
        /// </summary>
        [HideInInspector]
        public int videoBitRate = 20_000_000;

        /// <summary>
        /// Video keyframe interval in seconds.
        /// </summary>
        [HideInInspector]
        public int keyframeInterval = 2;

        /// <summary>
        /// Audio bit rate in bits per second.
        /// </summary>
        [HideInInspector]
        public int audioBitRate = 64_000;

        /// <summary>
        /// Recording watermark.
        /// </summary>
        public Texture? watermark {
            get => textureSource?.watermark ?? _watermark;
            set {
                _watermark = value;
                if (textureSource != null)
                    textureSource.watermark = value;
            }
        }

        /// <summary>
        /// Watermark normalized display rect when `watermarkMode` is set to `WatermarkMode.Custom`.
        /// </summary>
        public Rect watermarkRect {
            get {
                if (textureSource == null)
                    return _watermarkRect;
                var rect = textureSource!.watermarkRect;
                return new(
                    rect.x / width,
                    rect.y / height,
                    rect.width / width,
                    rect.height / height
                );
            }
            set {
                _watermarkRect = value;
                if (textureSource != null)
                    textureSource.watermarkRect = new(
                        Mathf.RoundToInt(value.x * width),
                        Mathf.RoundToInt(value.y * height),
                        Mathf.RoundToInt(value.width * width),
                        Mathf.RoundToInt(value.height * height)
                    );
            }
        }

        /// <summary>
        /// Recorder status.
        /// </summary>
        public Status status => clock?.paused switch {
            true    => Status.Paused,
            false   => Status.Recording,
            null    => Status.Idle,
        };

        /// <summary>
        /// Start recording.
        /// </summary>
        public async void StartRecording () => await StartRecordingAsync();

        /// <summary>
        /// Start recording.
        /// </summary>
        public async Task StartRecordingAsync () {
            // Check active
            if (!isActiveAndEnabled)
                throw new InvalidOperationException(@"VideoKitRecorder cannot start recording because component is disabled");
            // Check status
            if (status != Status.Idle)
                throw new InvalidOperationException(@"VideoKitRecorder cannot start recording because a recording session is already in progress");
            // Check camera device mode
            if (videoMode == VideoMode.CameraDevice) {
                // Check camera view
                if (cameraView == null)
                    throw new InvalidOperationException(@"VideoKitRecorder cannot start recording because the video mode is set to `VideoMode.CameraDevice` but `cameraView` is null");
                // Check camera preview is running
                if (cameraView.texture == null)
                    throw new InvalidOperationException(@"VideoKitRecorder cannot start recording because the video mode is set to `VideoMode.CameraDevice` but the camera preview is not running");
            }
            // Check audio mode
            if (audioMode.HasFlag(AudioMode.AudioListener) && Application.platform == RuntimePlatform.WebGLPlayer) {
                Debug.LogWarning(@"VideoKitRecorder cannot record audio from AudioListener because WebGL does not support `OnAudioFilterRead`");
                audioMode &= ~AudioMode.AudioListener;
            }
            // Check audio device
            if (audioMode.HasFlag(AudioMode.AudioDevice)) {
                // Check audio manager
                if (audioManager == null)
                    throw new InvalidOperationException(@"VideoKitRecorder cannot start recording because the audio mode includes `AudioMode.AudioDevice` but `audioManager` is null");
                // Configure audio manager
                if (configureAudioManager) {
                    // Set format
                    if (audioMode.HasFlag(AudioMode.AudioListener)) {
                        audioManager.sampleRate = VideoKitAudioManager.SampleRate.MatchUnity;
                        audioManager.channelCount = VideoKitAudioManager.ChannelCount.MatchUnity;
                    }
                    // Start running
                    await audioManager.StartRunningAsync();
                }
            }
            // Check format
            if (format == MediaFormat.MP4 && Application.platform == RuntimePlatform.WebGLPlayer) {
                format = MediaFormat.WEBM;
                Debug.LogWarning(@"VideoKitRecorder will use WEBM format on WebGL because MP4 is not supported");
            }
            // Create recorder
            recorder = await MediaRecorder.Create(
                format,
                width: width,
                height: height,
                frameRate: frameRate,
                sampleRate: sampleRate,
                channelCount: channelCount,
                videoBitRate: videoBitRate,
                keyframeInterval: keyframeInterval,
                compressionQuality: 0.8f,
                audioBitRate: audioBitRate,
                prefix: mediaPathPrefix
            );
            // Create inputs
            clock = new RealtimeClock();
            videoInput = CreateVideoInput(recorder!.width, recorder.height, recorder.Append);
            audioInput = CreateAudioInput();
        }

        /// <summary>
        /// Pause recording.
        /// </summary>
        [Obsolete(@"Deprecated in VideoKit 0.0.20 and will be removed soon after.", false)]
        public void PauseRecording () {
            // Check
            if (status != Status.Recording) {
                Debug.LogError(@"Cannot pause recording because no recording session is in progress");
                return;
            }
            // Stop audio manager
            if (configureAudioManager && audioManager != null)
                audioManager.StopRunning();
            // Dispose inputs
            videoInput?.Dispose();
            audioInput?.Dispose();
            videoInput = null;
            audioInput = null;
            // Pause clock
            clock!.paused = true;
        }

        /// <summary>
        /// Resume recording.
        /// </summary>
        [Obsolete(@"Deprecated in VideoKit 0.0.20 and will be removed soon after.", false)]
        public void ResumeRecording () {
            // Check status
            if (status != Status.Paused) {
                Debug.LogError(@"Cannot resume recording because the recording session is not paused");
                return;
            }
            // Check active
            if (!isActiveAndEnabled) {
                Debug.LogError(@"Cannot resume recording because component is disabled");
                return;
            }
            // Check audio manager
            if (configureAudioManager && audioManager != null)
                audioManager.StartRunning();
            // Unpause clock
            clock!.paused = false;
            // Create inputs
            videoInput = CreateVideoInput(recorder!.width, recorder.height, recorder.Append);
            audioInput = CreateAudioInput();
        }

        /// <summary>
        /// Stop recording.
        /// </summary>
        public async void StopRecording () => await StopRecordingAsync();

        /// <summary>
        /// Stop recording.
        /// </summary>
        public async Task StopRecordingAsync () {
            // Check
            if (status == Status.Idle) {
                Debug.LogWarning(@"Cannot stop recording because no recording session is in progress");
                return;
            }
            // Stop audio manager
            if (configureAudioManager && audioManager != null)
                audioManager.StopRunning();
            // Stop inputs
            audioInput?.Dispose();
            videoInput?.Dispose();
            videoInput = null;
            audioInput = null;
            clock = null;
            // Stop recording
            var asset = await recorder!.FinishWriting();
            // Check that this is not result of disabling // CHECK // Delete asset?
            if (!isActiveAndEnabled)
                return;
            // Post action
            if (recordingAction.HasFlag(RecordingAction.Custom))
                OnRecordingCompleted?.Invoke(asset);
            if (recordingAction.HasFlag(RecordingAction.CameraRoll))
                await asset.SaveToCameraRoll();
            if (recordingAction.HasFlag(RecordingAction.Share))
                await asset.Share();
            if (recordingAction.HasFlag(RecordingAction.Playback) && asset.type == MediaType.Video) {
                #if UNITY_IOS || UNITY_ANDROID
                Handheld.PlayFullScreenMovie($"file://{asset.path}");
                #endif
            }
        }

        /// <summary>
        /// Capture a screenshot with the current video settings.
        /// </summary>
        /// <returns>Screenshot image asset.</returns>
        public async Task<MediaAsset> CaptureScreenshot () {
            var recorder = await MediaRecorder.Create(
                MediaFormat.JPEG,
                width,
                height,
                compressionQuality: 0.8f,
                prefix: mediaPathPrefix
            );
            var tcs = new TaskCompletionSource<MediaAsset>();
            IDisposable? source = null;
            source = CreateVideoInput(
                recorder.width,
                recorder.height,
                async pixelBuffer => {
                    recorder.Append(pixelBuffer);
                    source!.Dispose();
                    var sequenceAsset = await recorder.FinishWriting();
                    var imageAsset = sequenceAsset.assets[0];
                    tcs.SetResult(imageAsset);
                }
            );
            var result = await tcs.Task;
            return result;
        }
        #endregion


        #region --Operations--
        private MediaRecorder? recorder;
        private RealtimeClock? clock;
        private IDisposable? videoInput;
        private IDisposable? audioInput;

        private int width               => resolution switch {
            var _ when videoMode == 0   => 0,
            var _ when videoMode == VideoMode.CameraDevice => cameraView!.texture!.width,
            Resolution._240xAuto        => 240,
            Resolution._320xAuto        => 320,
            Resolution._480xAuto        => 480,
            Resolution._640xAuto        => 640,
            Resolution._720xAuto        => 720,
            Resolution._1080xAuto       => 1080,
            Resolution._1280xAuto       => 1280,
            Resolution._1920xAuto       => 1920,
            Resolution._1440xAuto       => 1440,
            Resolution._2560xAuto       => 2560,
            Resolution._3840xAuto       => 3840,
            Resolution.Screen           => Screen.width >> 1 << 1,
            Resolution.HalfScreen       => Screen.width >> 2 << 1,
            Resolution.Custom           => customResolution.x,
            _                           => 1280,
        };

        private float aspect            => videoMode switch {
            VideoMode.Camera            => (float)Screen.width / Screen.height,
            VideoMode.Screen            => (float)Screen.width / Screen.height,
            VideoMode.Texture           => (float)texture!.width / texture!.height,
            _                           => 0f,
        };

        private int height              => resolution switch {
            var _ when videoMode == 0   => 0,
            var _ when videoMode == VideoMode.CameraDevice => cameraView!.texture!.height,
            Resolution.Custom           => customResolution.y,
            Resolution.Screen           => Screen.height >> 1 << 1,
            Resolution.HalfScreen       => Screen.height >> 2 << 1,
            _                           => Mathf.RoundToInt(width / aspect) >> 1 << 1,
        };

        private float frameRate => videoMode switch {
            var _ when format == MediaFormat.GIF    => _frameRate,
            //VideoMode.CameraDevice                  => cameraManager!.device!.frameRate,
            _                                       => 30,
        };

        private int sampleRate          => audioMode switch {
            AudioMode.AudioDevice       => audioManager?.device?.sampleRate ?? 0,
            AudioMode.AudioListener     => AudioSettings.outputSampleRate,
            AudioMode.AudioSource       => AudioSettings.outputSampleRate,
            _                           => 0,
        };

        private int channelCount        => audioMode switch {
            AudioMode.AudioDevice       => audioManager?.device?.channelCount ?? 0,
            AudioMode.AudioListener     => (int)AudioSettings.speakerMode,
            AudioMode.AudioSource       => (int)AudioSettings.speakerMode,
            _                           => 0,
        };

        private TextureSource? textureSource => videoInput switch {
            CameraSource cameraSource   => cameraSource.textureSource,
            ScreenSource screenSource   => screenSource.textureSource,
            TextureSource textureSource => textureSource,
            _                           => null,
        };

        private void Reset () {
            cameras = Camera.allCameras;
            cameraView = FindFirstObjectByType<VideoKitCameraView>();
            audioManager = FindFirstObjectByType<VideoKitAudioManager>();
            audioListener = FindFirstObjectByType<AudioListener>();
        }

        private async void Awake () {
            if (prepareOnAwake)
                await PrepareEncoder();
        }

        private void OnDestroy () {
            if (status != Status.Idle)
                StopRecording();
        }

        private IDisposable? CreateVideoInput (
            int width,
            int height,
            Action<PixelBuffer> handler
        ) {
            if (!MediaRecorder.CanAppend<PixelBuffer>(format))
                return null;
            if (videoMode == VideoMode.Screen) {
                var source = new ScreenSource(width, height, handler, clock) {
                    frameSkip = frameSkip
                };
                source.textureSource.watermark = watermark;
                source.textureSource.watermarkRect = CreateWatermarkRect(width, height);
                return source;
            } else if (videoMode == VideoMode.Camera) {
                var source = new CameraSource(width, height, handler, clock, cameras) {
                    frameSkip = frameSkip
                };
                source.textureSource.watermark = watermark;
                source.textureSource.watermarkRect = CreateWatermarkRect(width, height);
                return source;
            } else if (videoMode == VideoMode.Texture) {
                var source = new TextureSource(width, height, handler, clock) {
                    texture = texture,
                    frameSkip = frameSkip
                };
                source.watermark = watermark;
                source.watermarkRect = CreateWatermarkRect(width, height);
                return source;
            } else if (videoMode == VideoMode.CameraDevice) // No support for watermarking yet
                return new CameraViewSource(cameraView!, handler, clock) {
                    frameSkip = frameSkip
                };
            else
                return null;
        }

        private IDisposable? CreateAudioInput () => audioMode switch {
            var _ when !MediaRecorder.CanAppend<AudioBuffer>(format) => null,
            AudioMode.AudioDevice   => new AudioManagerSource(recorder!, clock, audioManager!),
            AudioMode.AudioListener => new AudioComponentSource(recorder!, clock, audioListener!),
            AudioMode.AudioSource   => new AudioComponentSource(recorder!, clock, audioSource!),
            _                       => null,
        };
        #endregion


        #region --Utility--

        private RectInt CreateWatermarkRect (int width, int height) {
            // Check none
            if (watermarkMode == WatermarkMode.None)
                return default;
            // Check custom
            if (watermarkMode == WatermarkMode.Custom)
                return new(
                    Mathf.RoundToInt(watermarkRect.x * width),
                    Mathf.RoundToInt(watermarkRect.y * height),
                    Mathf.RoundToInt(watermarkRect.width * width),
                    Mathf.RoundToInt(watermarkRect.height * height)
                );
            // Construct rect
            var imageSize = new Vector2(width, height);
            var offset = 0.1f;
            var size = 0.3f;
            var normalizedRect = new Dictionary<WatermarkMode, Rect> {
                [WatermarkMode.BottomLeft]  = new(offset, offset, size, size),
                [WatermarkMode.BottomRight] = new(1f - size - offset, offset, size, size),
                [WatermarkMode.UpperLeft]   = new(offset, 1f - size - offset, size, size),
                [WatermarkMode.UpperRight]  = new(1f - size - offset, 1f - size - offset, size, size),
            }[watermarkMode];
            var frameRect = new RectInt(
                Vector2Int.RoundToInt(Vector2.Scale(normalizedRect.position, imageSize)),
                Vector2Int.RoundToInt(Vector2.Scale(normalizedRect.size, imageSize))
            );
            // Return
            return frameRect;
        }

        private static async Task PrepareEncoder () {
            try {
                // Create recorder
                var clock = new FixedClock(30);
                var recorder = await MediaRecorder.Create(
                    MediaFormat.MP4,
                    width: 1280,
                    height: 720,
                    frameRate: 30
                );
                // Commit empty frames
                using var pixelData = new NativeArray<byte>(
                    recorder.width * recorder.height * 4,
                    Allocator.Persistent,
                    NativeArrayOptions.ClearMemory
                );
                var format = PixelBuffer.Format.RGBA8888;
                for (var i = 0; i < 3; ++i) {
                    using var pixelBuffer = new PixelBuffer(
                        recorder.width,
                        recorder.height,
                        format,
                        pixelData,
                        timestamp:
                        clock.timestamp
                    );
                    recorder.Append(pixelBuffer);
                }
                // Finish and delete
                var asset = await recorder.FinishWriting();
                File.Delete(asset.path);
            } catch { }
        }
        #endregion
    }
}