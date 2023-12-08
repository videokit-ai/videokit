/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Events;
    using Assets;
    using Devices;
    using Internal;
    using Recorders;
    using Recorders.Clocks;
    using Recorders.Inputs;

    /// <summary>
    /// VideoKit recorder for recording videos.
    /// </summary>
    [Tooltip(@"VideoKit recorder for recording videos.")]
    [HelpURL(@"https://docs.videokit.ai/videokit/api/videokitrecorder")]
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
        [Flags]
        public enum AudioMode : int {
            /// <summary>
            /// Don't record audio.
            /// </summary>
            None            = 0b00,
            /// <summary>
            /// Record audio frames from the scene's audio listener.
            /// </summary>
            AudioListener   = 0b01,
            /// <summary>
            /// Record audio frames from an audio device.
            /// </summary>
            AudioDevice     = 0b10,
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
            /// HVGA resolution.
            /// </summary>
            _480xAuto   = 6,
            /// <summary>
            /// SD resolution.
            /// </summary>
            _640xAuto   = 0,
            /// <summary>
            /// Potrait HD resolution.
            /// </summary>
            _720xAuto   = 7,
            /// <summary>
            /// Portrait Full HD resolution.
            /// </summary>
            _1080xAuto  = 12,
            /// <summary>
            /// HD resolution.
            /// </summary>
            _1280xAuto  = 1,
            /// <summary>
            /// Full HD resolution.
            /// </summary>
            _1920xAuto  = 2,
            /// <summary>
            /// 2K WQHD resolution.
            /// </summary>
            _2560xAuto  = 3,
            /// <summary>
            /// 4K UHD resolution.
            /// </summary>
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
            /// Delete the media asset immediately.
            /// </summary>
            Delete      = 1 << 4,
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
        /// </summary>
        [Tooltip(@"Video recording resolution.")]
        public Resolution resolution = Resolution._1280xAuto;

        /// <summary>
        /// Video recording custom resolution.
        /// This is only used when `resolution` is set to `Resolution.Custom`.
        /// </summary>
        [Tooltip(@"Video recording custom resolution.")]
        public Vector2Int customResolution = new Vector2Int(1280, 720);

        /// <summary>
        /// Game cameras to record.
        /// </summary>
        [Tooltip(@"Game cameras to record.")]
        public Camera[] cameras;

        /// <summary>
        /// Recording texture for recording video frames from a texture.
        /// </summary>
        [Tooltip(@"Recording texture for recording video frames from a texture.")]
        public Texture texture;

        /// <summary>
        /// Camera manager for recording video frames from a camera device.
        /// </summary>
        [Tooltip(@"Camera manager for recording video frames from a camera device.")]
        public VideoKitCameraManager cameraManager;

        /// <summary>
        /// Frame rate for animated GIF images.
        /// This only applies when recording GIF images.
        /// </summary>
        [Tooltip(@"Frame rate for animated GIF images."), Range(5f, 30f)]
        public float frameRate = 10f;

        /// <summary>
        /// Number of successive camera frames to skip while recording.
        /// </summary>
        [Tooltip(@"Number of successive camera frames to skip while recording."), Range(0, 5)]
        public int frameSkip = 0;

        [Header(@"Watermark")]
        /// <summary>
        /// Recording watermark mode for adding a watermark to videos.
        /// </summary>
        [Tooltip(@"Recording watermark mode for adding a watermark to videos.")]
        public WatermarkMode watermarkMode = WatermarkMode.None;

        /// <summary>
        /// Recording watermark.
        /// </summary>
        [Tooltip(@"Recording watermark.")]
        public Texture watermark;

        /// <summary>
        /// Watermark display rect when `watermarkMode` is set to `WatermarkMode.Custom`.
        /// </summary>
        [Tooltip(@"Watermark display rect when `watermarkMode` is set to `WatermarkMode.Custom`")]
        public RectInt watermarkRect;

        [Header(@"Audio")]
        /// <summary>
        /// Audio recording mode.
        /// </summary>
        [Tooltip(@"Audio recording mode.")]
        public AudioMode audioMode = AudioMode.None;

        /// <summary>
        /// Audio manager for recording audio sample buffers from an audio device.
        /// </summary>
        [Tooltip(@"Audio manager for recording audio sample buffers from an audio device.")]
        public VideoKitAudioManager audioManager;

        /// <summary>
        /// Whether the recorder can configure the audio manager for recording.
        /// Unless you intend to override the audio manager configuration, leave this `true`.
        /// </summary>
        [Tooltip(@"Whether the recorder can configure the audio manager for recording.")]
        public bool configureAudioManager = true;

        /// <summary>
        /// Audio device gain when recording both game and microphone audio.
        /// </summary>
        [Tooltip(@"Audio device gain when recording both game and microphone audio."), Range(1f, 5f)]
        public float audioDeviceGain = 2f;

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
        public UnityEvent<MediaAsset> OnRecordingCompleted;
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
        public int videoBitRate = 10_000_000;

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
        public async Task StartRecording () {
            // Check active
            if (!isActiveAndEnabled)
                throw new InvalidOperationException(@"VideoKitRecorder cannot start recording because component is disabled");
            // Check status
            if (status != Status.Idle)
                throw new InvalidOperationException(@"VideoKitRecorder cannot start recording because a recording session is already in progress");
            // Check camera device mode
            if (videoMode == VideoMode.CameraDevice) {
                // Check camera manager
                if (!cameraManager)
                    throw new InvalidOperationException(@"VideoKitRecorder cannot start recording because the video mode is set to `VideoMode.CameraDevice` but `cameraManager` is null");
                // Check camera preview
                if (!cameraManager.running)
                    throw new InvalidOperationException(@"VideoKitRecorder cannot start recording because the video mode is set to `VideoMode.CameraDevice` but the `cameraManager` is not running");
            }
            // Check audio mode
            if (audioMode.HasFlag(AudioMode.AudioListener) && Application.platform == RuntimePlatform.WebGLPlayer) {
                Debug.LogWarning(@"VideoKitRecorder cannot record audio from AudioListener because WebGL does not support `OnAudioFilterRead`");
                audioMode &= ~AudioMode.AudioListener;
            }
            // Check audio device
            if (audioMode.HasFlag(AudioMode.AudioDevice)) {
                // Check audio manager
                if (!audioManager)
                    throw new InvalidOperationException(@"VideoKitRecorder cannot start recording because the audio mode includes `AudioMode.AudioDevice` but `audioManager` is null");
                // Configure audio manager
                if (configureAudioManager) {
                    // Set format
                    if (audioMode.HasFlag(AudioMode.AudioListener)) {
                        audioManager.sampleRate = VideoKitAudioManager.SampleRate.MatchUnity;
                        audioManager.channelCount = VideoKitAudioManager.ChannelCount.MatchUnity;
                    }
                    // Start running
                    await audioManager.StartRunning();
                }
                // Check audio stream
                if (!audioManager.running)
                    throw new InvalidOperationException(@"VideoKitRecorder cannot start recording because the audio mode includes to `AudioMode.AudioDevice` but the `audioManager` is not running");
            }
            // Check format
            if (format == MediaFormat.MP4 && Application.platform == RuntimePlatform.WebGLPlayer) {
                format = MediaFormat.WEBM;
                Debug.LogWarning(@"VideoKitRecorder will use WEBM format on WebGL because MP4 is not supported");
            }
            // Create recorder
            var (width, height) = CreateVideoFormat();
            var (sampleRate, channelCount) = CreateAudioFormat();
            recorder = await MediaRecorder.Create(
                format,
                width: width,
                height: height,
                frameRate: format == MediaFormat.GIF ? frameRate : 30,
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
            videoInput = CreateVideoInput();
            audioInput = CreateAudioInput();
        }

        /// <summary>
        /// Pause recording.
        /// </summary>
        public void PauseRecording () {
            // Check
            if (status != Status.Recording) {
                Debug.LogError(@"Cannot pause recording because no recording session is in progress");
                return;
            }
            // Stop audio manager
            if (configureAudioManager && audioManager)
                audioManager.StopRunning();
            // Dispose inputs
            videoInput?.Dispose(); // this implicitly disposes the `textureInput`, perhaps not the best API design
            audioInput?.Dispose();
            videoInput = null;
            audioInput = null;
            // Pause clock
            clock.paused = true;
        }

        /// <summary>
        /// Resume recording.
        /// </summary>
        public async void ResumeRecording () {
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
            if (configureAudioManager && audioManager)
                await audioManager.StartRunning();
            // Unpause clock
            clock.paused = false;
            // Create inputs
            videoInput = CreateVideoInput();
            audioInput = CreateAudioInput();
        }

        /// <summary>
        /// Stop recording.
        /// </summary>
        public async Task StopRecording () {
            // Check
            if (status == Status.Idle) {
                Debug.LogWarning(@"Cannot stop recording because no recording session is in progress");
                return;
            }
            // Stop audio manager
            if (configureAudioManager && audioManager)
                audioManager.StopRunning();
            // Stop inputs
            audioInput?.Dispose();
            videoInput?.Dispose();
            videoInput = null;
            audioInput = null;
            clock = null;
            // Stop recording
            var recordingPath = await recorder.FinishWriting();
            var mediaAsset = await MediaAsset.FromFile(recordingPath);
            // Check that this is not result of disabling
            if (!isActiveAndEnabled) {
                mediaAsset.Delete();
                return;
            }
            // Post action
            if (recordingAction.HasFlag(RecordingAction.Custom))
                OnRecordingCompleted?.Invoke(mediaAsset);
            if (recordingAction.HasFlag(RecordingAction.CameraRoll))
                await mediaAsset.SaveToCameraRoll();
            if (recordingAction.HasFlag(RecordingAction.Share))
                await mediaAsset.Share();
            if (recordingAction.HasFlag(RecordingAction.Playback) && mediaAsset is VideoAsset videoAsset)
                videoAsset.Playback();
            if (recordingAction.HasFlag(RecordingAction.Delete))
                mediaAsset.Delete();                
        }
        #endregion


        #region --Operations--
        private MediaRecorder recorder;
        private RealtimeClock clock;
        private IDisposable videoInput;
        private IDisposable audioInput;

        private void Reset () {
            cameras = Camera.allCameras;
            cameraManager = FindObjectOfType<VideoKitCameraManager>();
            audioManager = FindObjectOfType<VideoKitAudioManager>();
        }

        private async void Awake () {
            if (prepareOnAwake)
                await PrepareEncoder();
        }

        private async void OnDestroy () {
            if (status != Status.Idle)
                await StopRecording();
        }

        private (int width, int height) CreateVideoFormat () {
            // Custom resolution
            if (resolution == Resolution.Custom)
                return (customResolution.x, customResolution.y);
            // Screen resolution
            if (resolution == Resolution.Screen)
                return (Screen.width >> 1 << 1, Screen.height >> 1 << 1);
            // Half screen resolution
            if (resolution == Resolution.HalfScreen)
                return (Screen.width >> 2 << 1, Screen.height >> 2 << 1);
            // Get video size
            var width = GetVideoWidth();
            var aspect = GetVideoAspect();
            var height = Mathf.RoundToInt(width / aspect) >> 1 << 1;
            return (width, height);
        }

        private int GetVideoWidth () => resolution switch {
            Resolution._240xAuto    => 240,
            Resolution._320xAuto    => 320,
            Resolution._480xAuto    => 480,
            Resolution._640xAuto    => 640,
            Resolution._720xAuto    => 720,
            Resolution._1080xAuto   => 1080,
            Resolution._1280xAuto   => 1280,
            Resolution._1920xAuto   => 1920,
            Resolution._2560xAuto   => 2560,
            Resolution._3840xAuto   => 3840,
            _                       => 1280,
        };

        private float GetVideoAspect () => videoMode switch {
            VideoMode.Camera        => (float)Screen.width / Screen.height,
            VideoMode.Screen        => (float)Screen.width / Screen.height,
            VideoMode.Texture       => (float)texture.width / texture.height,
            VideoMode.CameraDevice  => (float)cameraManager.GetPreviewSize().width / cameraManager.GetPreviewSize().height,
            _                       => 1f,
        };

        private (int sampleRate, int channelCount) CreateAudioFormat () => audioMode switch {
            var mode when mode.HasFlag(AudioMode.AudioListener) => (AudioSettings.outputSampleRate, (int)AudioSettings.speakerMode),
            AudioMode.AudioDevice                               => (audioManager?.device?.sampleRate ?? 0, audioManager?.device?.channelCount ?? 0),
            _                                                   => (0, 0),
        };

        private RecorderTextureInput CreateTextureInput () => new RecorderTextureInput(recorder, new [] { // no side effects
            new WatermarkTextureInput(null as TextureInput) { // MUST be `TextureInput`
                watermark = watermarkMode != WatermarkMode.None ? watermark : null,
                rect = CreateWatermarkRect(recorder, watermarkMode, watermarkRect),
            },
        });

        private IDisposable CreateVideoInput () => videoMode switch { // no side effects
            var x when !format.SupportsVideo() => null,
            VideoMode.Screen        => new ScreenInput(CreateTextureInput(), clock) { frameSkip = frameSkip },
            VideoMode.Camera        => new CameraInput(CreateTextureInput(), clock, cameras) { frameSkip = frameSkip },
            VideoMode.Texture       => new CustomTextureInput(CreateTextureInput(), clock, texture) { frameSkip = frameSkip },
            VideoMode.CameraDevice  => new CameraDeviceInput(CreateTextureInput(), clock, cameraManager) { frameSkip = frameSkip },
            _                       => null,
        };

        private IDisposable CreateAudioInput () => audioMode switch { // no side effects
            var x when !format.SupportsAudio() => null,
            var x when x.HasFlag(AudioMode.AudioDevice | AudioMode.AudioListener) => new AudioMixerInput(recorder, clock, audioManager, FindObjectOfType<AudioListener>()) { audioDeviceGain = audioDeviceGain },
            AudioMode.AudioListener => new AudioInput(recorder, clock, FindObjectOfType<AudioListener>()),
            AudioMode.AudioDevice   => new AudioDeviceInput(recorder, clock, audioManager),
            _                       => null,
        };
        #endregion


        #region --Utility--

        private static RectInt CreateWatermarkRect (MediaRecorder recorder, WatermarkMode mode, RectInt customRect) {
            // Check none
            if (mode == WatermarkMode.None)
                return default;
            // Check custom
            if (mode == WatermarkMode.Custom)
                return customRect;
            // Construct rect
            var (width, height) = recorder.frameSize;
            var NormalizedPositions = new Dictionary<WatermarkMode, Vector2> {
                [WatermarkMode.BottomLeft]  = new Vector2(0.2f, 0.15f),
                [WatermarkMode.BottomRight] = new Vector2(0.8f, 0.15f),
                [WatermarkMode.UpperLeft]   = new Vector2(0.2f, 0.85f),
                [WatermarkMode.UpperRight]  = new Vector2(0.8f, 0.85f),
            };
            var normalizedPosition = NormalizedPositions[mode];
            var position = Vector2Int.RoundToInt(Vector2.Scale(normalizedPosition, new Vector2(width, height)));
            var size = Mathf.RoundToInt(0.15f * Mathf.Max(width, height));
            var rect = new RectInt(position.x - size / 1, position.y - size / 1, size, size);
            return rect;
        }

        private static async Task PrepareEncoder () {
            // Check platform
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                return;
            // Create recorder
            var width = 1280;
            var height = 720;
            var clock = new FixedIntervalClock(30);
            var recorder = await MediaRecorder.Create(MediaFormat.MP4, width: width, height: height, frameRate: 30);
            // Commit empty frames
            var pixelBuffer = new byte[width * height * 4];
            for (var i = 0; i < 3; ++i)
                recorder.CommitFrame(pixelBuffer, clock.timestamp);
            // Finis and delete
            var path = await recorder.FinishWriting();
            File.Delete(path);
        }
        #endregion
    }
}