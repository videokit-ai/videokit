/* 
*   VideoKit
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit {

    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Events;
    using NatML.Devices;
    using NatML.Recorders;
    using NatML.Recorders.Clocks;
    using NatML.Recorders.Inputs;
    using NatML.Sharing;
    using Inputs;

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
            None        = 0,
            /// <summary>
            /// Record video frames from one or more game cameras.
            /// </summary>
            Camera      = 1,
            /// <summary>
            /// Record video frames from the screen.
            /// </summary>
            Screen      = 2,
            /// <summary>
            /// Record video frames from a texture.
            /// </summary>
            Texture     = 3,
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
            AudioListener   = 1 << 0,
            /// <summary>
            /// Record audio frames from the microphone.
            /// </summary>
            Microphone      = 1 << 1,
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
            /// Screen resolution.
            /// </summary>
            Screen = 3,
            /// <summary>
            /// Half of the screen resolution.
            /// </summary>
            HalfScreen = 4,
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
        /// Texture to record.
        /// </summary>
        [Tooltip(@"Texture to record.")]
        public Texture texture;

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
        /// Audio device to record from when using `AudioMode.Microphone`.
        /// </summary>
        public AudioDevice audioDevice;

        /// <summary>
        /// Whether a recording is currently in progress.
        /// </summary>
        public bool recording => clock != null;

        /// <summary>
        /// Start recording.
        /// </summary>
        public async void StartRecording () {
            // Check
            if (recording) {
                Debug.LogError(@"Cannot start recording because a recording is already in progress");
                return;
            }
            // Start recording
            audioDevice = audioMode.HasFlag(AudioMode.Microphone) ? (audioDevice ?? await CreateAudioDevice()) : audioDevice;
            recorder = CreateRecorder();
            clock = new RealtimeClock();
            videoInput = CreateVideoInput(recorder, clock);
            audioInput = CreateAudioInput(recorder, clock);
        }

        /// <summary>
        /// Stop recording.
        /// </summary>
        public async void StopRecording () {
            // Check
            if (!recording) {
                Debug.LogWarning(@"Cannot stop recording because no recording is in progress");
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
            // Handle post-recording
            var finalPath = destination.HasFlag(Destination.Documents) ? SaveToDocuments(path) : null;
            if (destination.HasFlag(Destination.CameraRoll))
                await SaveToCameraRoll(path);
            if (destination.HasFlag(Destination.PromptUser))
                await PromptUser(path);
            // Delete recording
            File.Delete(path);
            // Invoke handler
            OnRecordingCompleted?.Invoke(finalPath);
        }
        #endregion


        #region --Operations--
        private IMediaRecorder recorder;
        private IClock clock;
        private IDisposable videoInput;
        private IDisposable audioInput;

        private void Reset () => cameras = Camera.allCameras;

        private void Update () {
            if (videoInput is TextureInput textureInput)
                textureInput.CommitFrame(texture, clock.timestamp);
        }

        private void OnDisable () {
            if (recording)
                StopRecording();
        }

        private IMediaRecorder CreateRecorder () {
            var (width, height) = CreateVideoFormat(resolution, orientation, aspect);
            var frameRate = 30;
            var (sampleRate, channelCount) = CreateAudioFormat(audioMode, audioDevice);
            return CreateRecorder(format, width, height, frameRate, sampleRate, channelCount);
        }

        private IDisposable CreateVideoInput (IMediaRecorder recorder, IClock clock) => videoMode switch {
            VideoMode.Screen    => new ScreenInput(recorder, clock),
            VideoMode.Camera    => new CameraInput(recorder, clock, cameras),
            VideoMode.Texture   => TextureInput.CreateDefault(recorder),
            _                   => null,
        };

        private IDisposable CreateAudioInput (IMediaRecorder recorder, IClock clock) => audioMode switch {
            AudioMode.AudioListener => new AudioInput(recorder, clock, FindObjectOfType<AudioListener>()),
            AudioMode.Microphone    => new MicrophoneInput(recorder, clock, audioDevice),
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

        private static (int sampleRate, int channelCount) CreateAudioFormat (AudioMode mode, AudioDevice microphone = null) => mode switch {
            AudioMode.Microphone                            => (microphone?.sampleRate ?? 0, microphone?.channelCount ?? 0),
            _ when mode.HasFlag(AudioMode.AudioListener)    => (AudioSettings.outputSampleRate, (int)AudioSettings.speakerMode),
            _                                               => (0, 0),
        };

        private static IMediaRecorder CreateRecorder (Format format, int width, int height, float frameRate, int sampleRate, int channelCount) => format switch {
            Format.MP4  => new MP4Recorder(width, height, frameRate, sampleRate, channelCount),
            Format.HEVC => new HEVCRecorder(width, height, frameRate, sampleRate, channelCount),
            Format.WEBM => new WEBMRecorder(width, height, frameRate, sampleRate, channelCount),
            Format.GIF  => new GIFRecorder(width, height, 0.1f),
            Format.JPEG => new JPEGRecorder(width, height),
            Format.WAV  => new WAVRecorder(sampleRate, channelCount),
            _           => null,
        };

        private static async Task<AudioDevice> CreateAudioDevice () {
            // Request microphone permissions
            var permissionStatus = await MediaDeviceQuery.RequestPermissions<AudioDevice>();
            if (permissionStatus != PermissionStatus.Authorized)
                throw new InvalidOperationException(@"VideoKit: User did not grant microphone permissions");
            // Discover the front camera
            var query = new MediaDeviceQuery(MediaDeviceCriteria.AudioDevice);
            var device = query.current as AudioDevice;
            // Check
            if (device == null)
                throw new InvalidOperationException(@"VideoKit: Failed to discover any available audio devices");
            // Return
            return device;
        }

        private static (int width, int height) SizeForResolution (Resolution resolution) => resolution switch {
            Resolution._640x480     => (640, 480),
            Resolution._1280x720    => (1280, 720),
            Resolution._1920x1080   => (1920, 1080),
            Resolution.Screen       => (Screen.width >> 1 << 1, Screen.height >> 1 << 1),
            Resolution.HalfScreen   => (Screen.width >> 2 << 1, Screen.height >> 2 << 1),
            _                       => (1280, 720),
        };

        private static string SaveToDocuments (string path) {
            var documentsPath = Path.Combine(Path.GetDirectoryName(path), "VideoKit");
            Directory.CreateDirectory(documentsPath);
            var destinationPath = Path.Combine(documentsPath, Path.GetFileName(path));
            File.Copy(path, destinationPath);
            return destinationPath;
        }

        private static async Task SaveToCameraRoll (string path) {
            // Request permissions
            var granted = await SavePayload.RequestPermissions();
            if (!granted) {
                Debug.LogError(@"VideoKit recorder failed to save recording to camera roll because user did not grant permissions");
                return;
            }
            // Save
            var payload = new SavePayload();
            payload.AddMedia(path);
            await payload.Save();
        }

        private static async Task PromptUser (string path) {
            // Request permissions
            var granted = await SavePayload.RequestPermissions();
            if (!granted) {
                Debug.LogError(@"VideoKit recorder failed to share recording because user did not grant permissions");
                return;
            }
            // Share
            var payload = new SharePayload();
            payload.AddMedia(path);
            await payload.Share();
        }
        #endregion
    }
}