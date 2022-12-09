/* 
*   VideoKit
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit {

    using System;
    using System.IO;
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
    [Tooltip(@"VideoKit recorder for recording videos."), DisallowMultipleComponent]
    public sealed class VideoKitRecorder : MonoBehaviour {

        #region --Enumerations--
        /// <summary>
        /// Recording format.
        /// </summary>
        public enum Format {
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
        public enum Destination {
            /// <summary>
            /// Recorded media is discarded immediately.
            /// </summary>
            None        = 0,
            /// <summary>
            /// Record media to the app's private documents directory.
            /// </summary>
            Documents   = 1,
            /// <summary>
            /// Record images and videos to the camera roll.
            /// </summary>
            CameraRoll  = 2,
            /// <summary>
            /// Prompt the user to share the recorded media with the native sharing UI.
            /// </summary>
            PromptUser  = 3,
        }

        /// <summary>
        /// Video recording mode.
        /// </summary>
        public enum VideoMode {
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
        public enum AudioMode {
            /// <summary>
            /// Don't record audio.
            /// </summary>
            None            = 0,
            /// <summary>
            /// Record audio frames from the scene's audio listener.
            /// </summary>
            AudioListener   = 1,
            /// <summary>
            /// Record audio frames from an audio device.
            /// </summary>
            AudioDevice     = 2,
        }

        /// <summary>
        /// Video recording resolution presets.
        /// </summary>
        public enum Resolution {
            /// <summary>
            /// SD resolution.
            /// </summary>
            _640x480    = 0,
            /// <summary>
            /// HD resolution.
            /// </summary>
            _1280x720   = 1,
            /// <summary>
            /// Full HD resolution.
            /// </summary>
            _1920x1080  = 2,
            /// <summary>
            /// 2K WQHD resolution.
            /// </summary>
            _2K = 3,
            /// <summary>
            /// 4K UHD resolution.
            /// </summary>
            _4K = 4,
            /// <summary>
            /// Screen resolution.
            /// </summary>
            Screen = 9,
            /// <summary>
            /// Half of the screen resolution.
            /// </summary>
            HalfScreen = 10,
        }

        /// <summary>
        /// Video orientation mode.
        /// </summary>
        public enum OrientationMode {
            /// <summary>
            /// Match the screen orientation.
            /// </summary>
            MatchScreen = 0,
            /// <summary>
            /// Record in landscape.
            /// </summary>
            Landscape = 1,
            /// <summary>
            /// Record in portrait.
            /// </summary>
            Portrait = 2,
        }

        /// <summary>
        /// Video aspect mode.
        /// </summary>
        public enum AspectMode {
            /// <summary>
            /// Use the video resolution aspect ratio.
            /// </summary>
            Resolution = 0,
            /// <summary>
            /// Match the screen aspect ratio by adjusting the video width.
            /// </summary>
            MatchScreenAdjustWidth = 1,
            /// <summary>
            /// Match the screen aspect ratio by adjusting the video height.
            /// </summary>
            MatchScreenAdjustHeight = 2,
        }

        /// <summary>
        /// Recorder status.
        /// </summary>
        public enum Status {
            /// <summary>
            /// No recording session is in progress.
            /// </summary>
            Idle = 0,
            /// <summary>
            /// Recording session is in progress.
            /// </summary>
            Recording = 1,
            /// <summary>
            /// Recording session is in progress but is paused.
            /// </summary>
            Paused = 2,
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
        /// Video recording mode.
        /// </summary>
        [Tooltip(@"Video recording mode.")]
        public VideoMode videoMode = VideoMode.Camera;

        /// <summary>
        /// Audio recording mode.
        /// </summary>
        [Tooltip(@"Audio recording mode.")]
        public AudioMode audioMode = AudioMode.None;

        [Header(@"Video")]
        /// <summary>
        /// Video recording resolution.
        /// </summary>
        [Tooltip(@"Video recording resolution.")]
        public Resolution resolution = Resolution._1280x720;

        /// <summary>
        /// Video orientation mode.
        /// </summary>
        [Tooltip(@"Video orientation mode.")]
        public OrientationMode orientation = OrientationMode.MatchScreen;

        /// <summary>
        /// Video aspect mode.
        /// Use this to ensure that the recording aspect ratio matches the screen aspect ratio.
        /// </summary>
        [Tooltip(@"Video aspect mode. Use this to ensure that the recording aspect ratio matches the screen aspect ratio.")]
        public AspectMode aspect = AspectMode.Resolution;

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

        [Header(@"Events")]
        /// <summary>
        /// Event raised when a recording session is completed.
        /// </summary>
        [Tooltip(@"Event raised when a recording session is completed.")]
        public UnityEvent<string> OnRecordingCompleted;

        /// <summary>
        /// Event raised when a recording session fails.
        /// </summary>
        [Tooltip(@"Event raised when a recording session fails.")]
        public UnityEvent<Exception> OnRecordingFailed;
        #endregion


        #region --Client API--
        /// <summary>
        /// Recording destination path prefix when saving recordings to the app's documents.
        /// </summary>
        [HideInInspector]
        public string destinationPathPrefix = @"VideoKit";

        /// <summary>
        /// Video bit rate in bits per second.
        /// </summary>
        [HideInInspector]
        public int videoBitRate = 10_000_000;

        /// <summary>
        /// Video keyframe interval in seconds.
        /// </summary>
        [HideInInspector]
        public int videoKeyframeInterval = 2;

        /// <summary>
        /// Audio bit rate in bits per second.
        /// </summary>
        [HideInInspector]
        public int audioBitRate = 64_000;

        /// <summary>
        /// Audio device to record from when using `AudioMode.Microphone`.
        /// </summary>
        public AudioDevice audioDevice;

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
            if (audioMode.HasFlag(AudioMode.AudioDevice))
                audioDevice ??= await VideoKitUser.CreateAudioDevice();
            // Start recording
            recorder = CreateRecorder();
            clock = new RealtimeClock();
            videoInput = CreateVideoInput(recorder, clock);
            audioInput = CreateAudioInput(recorder, clock);
        }

        public void PauseRecording () {
            // Check
            if (status != Status.Recording) {
                Debug.LogError(@"Cannot pause recording because no recording session is in progress");
                return;
            }
            // Dispose inputs
            videoInput?.Dispose();
            audioInput?.Dispose();
            videoInput = null;
            audioInput = null;
            // Pause clock
            clock.paused = true;
        }

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
            // Unpause clock
            clock.paused = false;
            // Create inputs
            videoInput = CreateVideoInput(recorder, clock);
            audioInput = CreateAudioInput(recorder, clock);
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
            // Stop inputs
            videoInput?.Dispose();
            audioInput?.Dispose();
            videoInput = null;
            audioInput = null;
            clock = null;
            // Stop recording
            string path = null;
            try {
                path = await recorder.FinishWriting();
            } catch (Exception exception) {
                OnRecordingFailed?.Invoke(exception);
                return;
            } finally {
                recorder = null;
            }
            // Check that this is not result of disabling
            if (!isActiveAndEnabled) {
                File.Delete(path);
                return;
            }
            // Handle post-recording sharing
            if (destination.HasFlag(Destination.CameraRoll))
                await VideoKitUser.SaveToCameraRoll(path);
            if (destination.HasFlag(Destination.PromptUser))
                await VideoKitUser.Share(path);
            // Finalize file
            var finalPath = SaveToDocuments(path, destinationPathPrefix, destination);
            // Invoke handler
            OnRecordingCompleted?.Invoke(finalPath);
        }
        #endregion


        #region --Operations--
        private IMediaRecorder recorder;
        private RealtimeClock clock;
        private IDisposable videoInput;
        private IDisposable audioInput;

        private void Reset () {
            cameras = Camera.allCameras;
            cameraManager = FindObjectOfType<VideoKitCameraManager>();
        }

        private void Update () {
            if (status == Status.Recording && videoInput is TextureInput textureInput)
                textureInput.CommitFrame(texture, clock.timestamp);
        }

        private void OnDisable () {
            if (status != Status.Idle)
                StopRecording();
        }

        private IMediaRecorder CreateRecorder () {
            var (width, height) = CreateVideoFormat(resolution, orientation, aspect);
            var frameRate = 30;
            var (sampleRate, channelCount) = CreateAudioFormat(audioMode, audioDevice);
            switch (format) {
                case Format.MP4:    return new MP4Recorder(width, height, frameRate, sampleRate, channelCount, videoBitRate, videoKeyframeInterval, audioBitRate);
                case Format.HEVC:   return new HEVCRecorder(width, height, frameRate, sampleRate, channelCount, videoBitRate, videoKeyframeInterval, audioBitRate);
                case Format.WEBM:   return new WEBMRecorder(width, height, frameRate, sampleRate, channelCount, videoBitRate, videoKeyframeInterval, audioBitRate);
                case Format.GIF:    return new GIFRecorder(width, height, 0.1f);
                case Format.JPEG:   return new JPEGRecorder(width, height);
                case Format.WAV:    return new WAVRecorder(sampleRate, channelCount);
                default:            return null;
            }
        }

        private IDisposable CreateVideoInput (IMediaRecorder recorder, IClock clock) => videoMode switch {
            VideoMode.Screen        => new ScreenInput(recorder, clock),
            VideoMode.Camera        => new CameraInput(recorder, clock, cameras),
            VideoMode.Texture       => TextureInput.CreateDefault(recorder),
            VideoMode.CameraDevice  => new CameraDeviceInput(recorder, clock, cameraManager),
            _                       => null,
        };

        private IDisposable CreateAudioInput (IMediaRecorder recorder, IClock clock) => audioMode switch {
            AudioMode.AudioListener => new AudioInput(recorder, clock, FindObjectOfType<AudioListener>()),
            AudioMode.AudioDevice   => new AudioDeviceInput(recorder, clock, audioDevice),
            _                       => null,
        };

        private static (int width, int height) CreateVideoFormat (Resolution resolution, OrientationMode orientation, AspectMode aspect) {
            // Check inputs
            aspect = orientation == OrientationMode.MatchScreen ? aspect : AspectMode.Resolution;
            orientation = orientation == OrientationMode.MatchScreen && Screen.width < Screen.height ? OrientationMode.Portrait : orientation;
            // Get size
            var (width, height) = SizeForResolution(resolution);
            var portrait = orientation == OrientationMode.Portrait;
            (width, height) = portrait ? (height, width) : (width, height);
            // Set aspect
            switch (aspect) {
                case AspectMode.MatchScreenAdjustWidth:
                    width = (int)((float)Screen.width / Screen.height * height);
                    break;
                case AspectMode.MatchScreenAdjustHeight:
                    height = (int)((float)Screen.height / Screen.width * width);
                    break;
            }
            // Return
            width = width >> 1 << 1;
            height = height >> 1 << 1;
            return (width, height);
        }

        private static (int sampleRate, int channelCount) CreateAudioFormat (AudioMode mode, AudioDevice audioDevice = null) => mode switch {
            AudioMode.AudioDevice                           => (audioDevice?.sampleRate ?? 0, audioDevice?.channelCount ?? 0),
            _ when mode.HasFlag(AudioMode.AudioListener)    => (AudioSettings.outputSampleRate, (int)AudioSettings.speakerMode),
            _                                               => (0, 0),
        };

        private static (int width, int height) SizeForResolution (Resolution resolution) => resolution switch {
            Resolution._640x480     => (640, 480),
            Resolution._1280x720    => (1280, 720),
            Resolution._1920x1080   => (1920, 1080),
            Resolution._2K          => (2560, 1440),
            Resolution._4K          => (3840, 2160),
            Resolution.Screen       => (Screen.width >> 1 << 1, Screen.height >> 1 << 1),
            Resolution.HalfScreen   => (Screen.width >> 2 << 1, Screen.height >> 2 << 1),
            _                       => (1280, 720),
        };

        private static string SaveToDocuments (string path, string prefix, Destination destination) {
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
        #endregion
    }
}