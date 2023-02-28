/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Events;
    using NatML.Devices;
    using NatML.Recorders;
    using NatML.Recorders.Clocks;
    using NatML.Recorders.Inputs;
    using NatML.Sharing;
    using Inputs;
    using Internal;

    /// <summary>
    /// VideoKit recorder for recording videos.
    /// </summary>
    [Tooltip(@"VideoKit recorder for recording videos.")]
    [HelpURL(@"https://docs.videokit.ai/videokit/api/videokitrecorder")]
    [DisallowMultipleComponent]
    public sealed partial class VideoKitRecorder : MonoBehaviour {

        #region --Enumerations--
        /// <summary>
        /// Recording format.
        /// </summary>
        public enum Format : int {
            /// <summary>
            /// MP4 video with H.264 AVC video codec.
            /// This format supports recording both video and audio.
            /// </summary>
            MP4 = 0,
            /// <summary>
            /// MP4 video with H.265 HEVC video codec.
            /// This format has better compression than `MP4`.
            /// This format supports recording both video and audio.
            /// </summary>
            HEVC = 1,
            /// <summary>
            /// WEBM video.
            /// This format support recording both video and audio.
            /// This is only supported on Android and WebGL.
            /// </summary>
            WEBM = 2,
            /// <summary>
            /// Animated GIF image.
            /// This format only supports recording video.
            /// </summary>
            GIF = 3,
            /// <summary>
            /// JPEG image sequence.
            /// This format only supports recording video.
            /// </summary>
            JPEG = 4,
            /// <summary>
            /// Waveform audio.
            /// This format only supports recording audio.
            /// </summary>
            WAV = 5,
        }

        /// <summary>
        /// Recorded media destination.
        /// </summary>
        [Flags]
        public enum Destination : int {
            /// <summary>
            /// Recorded media is discarded immediately.
            /// </summary>
            None        = 0b0000,
            /// <summary>
            /// Record media to the app's private documents directory.
            /// </summary>
            Documents   = 0b0001,
            /// <summary>
            /// Record images and videos to the camera roll.
            /// </summary>
            CameraRoll  = 0b0010,
            /// <summary>
            /// Prompt the user to share the recorded media with the native sharing UI.
            /// </summary>
            PromptUser  = 0b0100,
            /// <summary>
            /// Playback the video with the platform default media player.
            /// </summary>
            Playback    = Documents | 0b1000,
        }

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
        #endregion


        #region --Inspector--
        [Header(@"Format")]
        /// <summary>
        /// Recording format.
        /// </summary>
        [Tooltip(@"Recording format.")]
        public Format format = Format.MP4;

        /// <summary>
        /// Recording destination.
        /// </summary>
        [Tooltip(@"Recording destination.")]
        public Destination destination = Destination.Documents;

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

        [Header(@"Events")]
        /// <summary>
        /// Event raised when a recording session is completed.
        /// </summary>
        [Tooltip(@"Event raised when a recording session is completed.")]
        public UnityEvent<RecordingSession> OnRecordingCompleted;
        #endregion


        #region --Client API--
        /// <summary>
        /// Recording destination path prefix when saving recordings to the app's documents.
        /// </summary>
        [HideInInspector]
        public string destinationPathPrefix = @"Recordings";

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
        public async void StartRecording () {
            // Check active
            if (!isActiveAndEnabled) {
                Debug.LogError(@"VideoKitRecorder cannot start recording because component is disabled");
                return;
            }
            // Check status
            if (status != Status.Idle) {
                Debug.LogError(@"VideoKitRecorder cannot start recording because a recording session is already in progress");
                return;
            }
            // Check camera device mode
            if (videoMode == VideoMode.CameraDevice) {
                // Check camera manager
                if (!cameraManager) {
                    Debug.LogError(@"VideoKitRecorder cannot start recording because the video mode is set to `VideoMode.CameraDevice` but `cameraManager` is null");
                    return;
                }
                // Check camera preview
                if (!cameraManager.running) {
                    Debug.LogError(@"VideoKitRecorder cannot start recording because the video mode is set to `VideoMode.CameraDevice` but the `cameraManager` is not running");
                    return;
                }
            }
            // Check audio device
            if (audioMode.HasFlag(AudioMode.AudioDevice)) {
                // Check audio manager
                if (!audioManager) {
                    Debug.LogError(@"VideoKitRecorder cannot start recording because the audio mode includes `AudioMode.AudioDevice` but `audioManager` is null");
                    return;
                }
                // Configure audio manager
                if (configureAudioManager) {
                    // Set format
                    if (audioMode.HasFlag(AudioMode.AudioListener)) {
                        audioManager.sampleRate = VideoKitAudioManager.SampleRate.MatchUnity;
                        audioManager.channelCount = VideoKitAudioManager.ChannelCount.MatchUnity;
                    }
                    // Start running
                    audioManager.StartRunning();
                    // Wait until running
                    var startTime = Time.time;
                    while (!audioManager.running && Time.time - startTime < 5f)
                        await Task.Yield();
                    // Check that audio device started
                    if (!audioManager.running) {
                        Debug.LogError(@"VideoKitRecorder cannot start recording because audio manager did not start running");
                        return;
                    }
                }
            }
            // Create recorder
            var (width, height) = CreateVideoFormat();
            var (sampleRate, channelCount) = CreateAudioFormat();
            recorder = CreateRecorder(width, height, sampleRate, channelCount);
            // Create inputs
            clock = new RealtimeClock();
            textureInput = CreateTextureInput();
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
            textureInput = null;
            videoInput = null;
            audioInput = null;
            // Pause clock
            clock.paused = true;
        }

        /// <summary>
        /// Resume recording.
        /// </summary>
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
            if (configureAudioManager && audioManager)
                audioManager.StartRunning();
            // Unpause clock
            clock.paused = false;
            // Create inputs
            textureInput = CreateTextureInput();
            videoInput = CreateVideoInput();
            audioInput = CreateAudioInput();
        }

        /// <summary>
        /// Stop recording.
        /// </summary>
        public async void StopRecording () {
            // Check
            if (status == Status.Idle) {
                Debug.LogWarning(@"Cannot stop recording because no recording session is in progress");
                return;
            }
            // Stop audio manager
            if (configureAudioManager && audioManager)
                audioManager.StopRunning();
            // Stop inputs
            videoInput?.Dispose(); // this implicitly disposes the `textureInput`, perhaps not the best API design
            audioInput?.Dispose();
            textureInput = null;
            videoInput = null;
            audioInput = null;
            clock = null;
            // Stop recording
            string path = null;
            try {
                path = await recorder.FinishWriting();
            } catch (Exception exception) {
                OnRecordingCompleted?.Invoke(new RecordingSession(null, exception, null, false));
                return;
            } finally {
                recorder = null;
            }
            // Check that this is not result of disabling
            if (!isActiveAndEnabled) {
                File.Delete(path);
                return;
            }
            // Create session
            var savedToCameraRoll = destination.HasFlag(Destination.CameraRoll) && await SaveToCameraRoll(path);
            var receiver = destination.HasFlag(Destination.PromptUser) ? await Share(path) : null;                
            var finalPath = SaveToDocuments(path, destinationPathPrefix, destination);
            // Playback
            if (destination.HasFlag(Destination.Playback))
                await PlaybackVideo(finalPath);
            // Invoke handler
            var session = new RecordingSession(finalPath, null, receiver, savedToCameraRoll);
            OnRecordingCompleted?.Invoke(session);
        }
        #endregion


        #region --Operations--
        private IMediaRecorder recorder;
        private RealtimeClock clock;
        private RecorderTextureInput textureInput; // always sends frames to `recorder`
        private IDisposable videoInput; // always sends frames to `textureInput`
        private IDisposable audioInput;
        private const float frameRate = 30;
        private const float frameDuration = 0.1f;

        private void Reset () {
            cameras = Camera.allCameras;
            cameraManager = FindObjectOfType<VideoKitCameraManager>();
            audioManager = FindObjectOfType<VideoKitAudioManager>();
        }

        private async void Awake () {
            if (prepareOnAwake)
                await PrepareEncoder();
        }

        private void OnDestroy () {
            if (status != Status.Idle)
                StopRecording();
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
            VideoKitRecorder.Resolution._320xAuto   => 320,
            VideoKitRecorder.Resolution._480xAuto   => 480,
            VideoKitRecorder.Resolution._640xAuto   => 640,
            VideoKitRecorder.Resolution._1280xAuto  => 1280,
            VideoKitRecorder.Resolution._1920xAuto  => 1920,
            VideoKitRecorder.Resolution._2560xAuto  => 2560,
            VideoKitRecorder.Resolution._3840xAuto  => 3840,
            _                                       => 1280,
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

        private IMediaRecorder CreateRecorder (int width, int height, int sampleRate, int channelCount) => format switch {
            Format.MP4  => new MP4Recorder(width, height, frameRate, sampleRate, channelCount, videoBitRate, keyframeInterval, audioBitRate),
            Format.HEVC => new HEVCRecorder(width, height, frameRate, sampleRate, channelCount, videoBitRate, keyframeInterval, audioBitRate),
            Format.WEBM => new WEBMRecorder(width, height, frameRate, sampleRate, channelCount, videoBitRate, keyframeInterval, audioBitRate),
            Format.GIF  => new GIFRecorder(width, height, frameDuration),
            Format.JPEG => new JPEGRecorder(width, height),
            Format.WAV  => new WAVRecorder(sampleRate, channelCount),
            _           => null,
        };

        private RecorderTextureInput CreateTextureInput () => new RecorderTextureInput(recorder, new [] { // no side effects
            new WatermarkTextureInput(null as TextureInput) { // MUST be `TextureInput`
                watermark = watermarkMode != WatermarkMode.None ? watermark : null,
                rect = CreateWatermarkRect(recorder, watermarkMode, watermarkRect),
            },
        });

        private IDisposable CreateVideoInput () => videoMode switch { // no side effects
            VideoMode.Screen        => new ScreenInput(textureInput, clock) { frameSkip = frameSkip },
            VideoMode.Camera        => new CameraInput(textureInput, clock, cameras) { frameSkip = frameSkip },
            VideoMode.Texture       => new CustomTextureInput(textureInput, clock, texture) { frameSkip = frameSkip },
            VideoMode.CameraDevice  => new CameraDeviceInput(textureInput, clock, cameraManager) { frameSkip = frameSkip },
            _                       => null,
        };

        private IDisposable CreateAudioInput () => audioMode switch { // no side effects
            var x when x.HasFlag(AudioMode.AudioDevice | AudioMode.AudioListener) => new AudioMixerInput(recorder, clock, audioManager, FindObjectOfType<AudioListener>()) { audioDeviceGain = audioDeviceGain },
            AudioMode.AudioListener                         => new AudioInput(recorder, clock, FindObjectOfType<AudioListener>()),
            AudioMode.AudioDevice                           => new AudioDeviceInput(recorder, clock, audioManager),
            _                                               => null,
        };
        #endregion


        #region --Utility--

        private static RectInt CreateWatermarkRect (IMediaRecorder recorder, WatermarkMode mode, RectInt customRect) {
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

        private static string SaveToDocuments (string path, string prefix, Destination destination) {
            // Check platform
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                return path;
            // Check destination
            if (!destination.HasFlag(Destination.Documents)) {
                File.Delete(path);
                return null;
            }
            // Check prefix
            if (string.IsNullOrWhiteSpace(prefix))
                return path;
            // Move file
            var directory = Path.Combine(Path.GetDirectoryName(path), prefix);
            var result = Path.Combine(directory, Path.GetFileName(path));
            Directory.CreateDirectory(directory);
            File.Move(path, result);
            return result;
        }

        private static async Task<string> Share (string path) {
            // Request permissions
            var granted = await SavePayload.RequestPermissions();
            if (!granted) {
                Debug.LogError(@"VideoKit recorder failed to share recording because user did not grant permissions");
                return null;
            }
            // Share
            var payload = new SharePayload();
            payload.AddMedia(path);
            var receiver = await payload.Share();
            return receiver;
        }

        private static async Task<bool> SaveToCameraRoll (string path) {
            // Request permissions
            var granted = await SavePayload.RequestPermissions();
            if (!granted) {
                Debug.LogError(@"VideoKit recorder failed to save recording to camera roll because user did not grant permissions");
                return false;
            }
            // Save
            var payload = new SavePayload();
            payload.AddMedia(path);
            var success = await payload.Save();
            return success;
        }

        private static async Task PlaybackVideo (string path) {
            #if UNITY_EDITOR
            //UnityEditor.EditorUtility.OpenWithDefaultApp(path); // errors on macOS for whatever reason
            #elif UNITY_IOS || UNITY_ANDROID
            Handheld.PlayFullScreenMovie($"file://{path}");
            #endif
        }

        private static async Task PrepareEncoder () {
            // Check platform
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                return;
            // Create recorder
            var width = 1280;
            var height = 720;
            var recorder = new MP4Recorder(width, height, 30);
            // Commit empty frame
            var pixelBuffer = new byte[width * height * 4];
            recorder.CommitFrame(pixelBuffer, 0L);
            // Finis and delete
            var path = await recorder.FinishWriting();
            File.Delete(path);
        }
        #endregion
    }
}