/* 
*   VideoKit
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit {

    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Events;
    using NatML.Devices;
    using NatML.Recorders;
    using NatML.Recorders.Clocks;
    using NatML.Recorders.Inputs;
    using NatML.Sharing;

    /// <summary>
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VideoKitRecorder : MonoBehaviour { // INCOMPLETE // DOC

        #region --Enumerations--
        /// <summary>
        /// Recording format.
        /// </summary>
        public enum Format {
            /// <summary>
            /// MP4 video.
            /// This format supports recording both video and audio.
            /// </summary>
            MP4 = 0,
            /// <summary>
            /// Animated GIF image.
            /// This format only supports recording video.
            /// </summary>
            GIF = 1,
            /// <summary>
            /// Waveform audio.
            /// This format only supports recording audio.
            /// </summary>
            WAV = 2,
        }

        /// <summary>
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
        /// </summary>
        public enum VideoMode {
            /// <summary>
            /// Don't record video.
            /// </summary>
            None        = 0,
            /// <summary>
            /// Record video frames from the screen.
            /// </summary>
            Screen      = 1,
            /// <summary>
            /// Record video frames from one or more game cameras.
            /// </summary>
            Camera      = 2,
            /// <summary>
            /// Record video frames from a texture.
            /// </summary>
            Texture     = 3,
        }

        /// <summary>
        /// </summary>
        public enum AudioMode {
            /// <summary>
            /// Don't record audio.
            /// </summary>
            None            = 0,
            /// <summary>
            /// Record audio frames from the microphone.
            /// </summary>
            Microphone      = 1,
            /// <summary>
            /// Record audio frames from an audio source.
            /// </summary>
            AudioSource     = 2,
            /// <summary>
            /// Record audio frames from the scene's audio listener.
            /// </summary>
            AudioListener   = 3,
        }

        /// <summary>
        /// Recording resolution presets.
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
        /// Recording resolution options.
        /// </summary>
        [Flags]
        public enum ResolutionOptions {
            /// <summary>
            /// </summary>
            None                            = 0,
            /// <summary>
            /// </summary>
            MatchScreenOrientation          = 1 << 0, 
            /// <summary>
            /// </summary>
            MatchScreenAspectScaleWidth     = (1 << 1) | ResolutionOptions.MatchScreenOrientation,
            /// <summary>
            /// </summary>
            MatchScreenAspectScaleHeight    = (1 << 2) | ResolutionOptions.MatchScreenOrientation,
        }

        /// <summary>
        /// </summary>
        public enum FrameRate {
            /// <summary>
            /// </summary>
            Realtime        = 0,
            /// <summary>
            /// </summary>
            HalfRealtime    = 1,
            /// <summary>
            /// </summary>
            QuarterRealtime = 2,
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
        /// </summary>
        [Tooltip(@"")]
        public VideoMode videoMode = VideoMode.Screen;

        /// <summary>
        /// </summary>
        [Tooltip(@"")]
        public AudioMode audioMode = AudioMode.AudioSource;

        [Header(@"Video")]
        /// <summary>
        /// </summary>
        [Tooltip(@"")]
        public Resolution resolution = Resolution._1280x720;

        /// <summary>
        /// </summary>
        [Tooltip(@"")]
        public ResolutionOptions resolutionOptions = ResolutionOptions.None;

        /// <summary>
        /// </summary>
        [Tooltip(@"")]
        public FrameRate frameRate = FrameRate.Realtime;

        /// <summary>
        /// </summary>
        [Tooltip(@"")]
        public Camera[] cameras;

        /// <summary>
        /// </summary>
        [Tooltip(@"")]
        public Texture texture;

        /// <summary>
        /// </summary>
        [Tooltip(@"")]
        public Texture watermark;

        [Header(@"Audio")]
        /// <summary>
        /// </summary>
        [Tooltip(@"")]
        public AudioSource audioSource;

        [Header(@"Events")]
        /// <summary>
        /// </summary>
        [Tooltip(@"")]
        public UnityEvent<string> OnRecording;
        #endregion


        #region --Client API--
        /// <summary>
        /// Whether the recorder is currently recording.
        /// </summary>
        public bool recording => videoInput != null || audioInput != null;

        /// <summary>
        /// </summary>
        public async void StartRecording () { // INCOMPLETE
            // Check
            if (recording) {
                Debug.LogWarning(@"Cannot start recording because a recording is already in progress");
                return;
            }
            // Get microphone
            if (audioMode == AudioMode.Microphone)
                microphone ??= await GetDefaultMicrophone();
            // Create recorder
            recorder = CreateRecorder();
        }

        /// <summary>
        /// </summary>
        public void StopRecording () { // INCOMPLETE
            // Check
            if (!recording) {
                Debug.LogWarning(@"Cannot stop recording because no recording is in progress");
                return;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="videoPath"></param>
        /// <param name="relativeTime"></param>
        /// <returns></returns>
        public Texture2D GetThumbnail (string videoPath, float relativeTime = 0f) { // INCOMPLETE
            return default;
        }
        #endregion


        #region --Operations--
        private AudioDevice microphone;
        private IMediaRecorder recorder;
        private IDisposable videoInput;
        private IDisposable audioInput;

        private void Reset () {
            cameras = Camera.allCameras;
        }

        private IMediaRecorder CreateRecorder () {
            var (width, height) = GetVideoFormat(resolution, resolutionOptions);
            var (sampleRate, channelCount) = GetAudioFormat(audioMode, microphone);
            switch (format) {
                case Format.MP4:    return new MP4Recorder(width, height, 30, sampleRate, channelCount);
                case Format.GIF:    return new GIFRecorder(width, height, 0.1f);
                case Format.WAV:    return new WAVRecorder(sampleRate, channelCount);
                default:            return default;
            }
        }

        private static (int width, int height) SizeForResolution (Resolution resolution) => resolution switch {
            Resolution._640x480     => (640, 480),
            Resolution._1280x720    => (1280, 720),
            Resolution._1920x1080   => (1920, 1080),
            Resolution.Screen       => (Screen.width >> 1 << 1, Screen.height >> 1 << 1),
            Resolution.HalfScreen   => (Screen.width >> 2 << 1, Screen.height >> 2 << 1),
            _                       => default
        };

        private static (int width, int height) GetVideoFormat (Resolution resolution, ResolutionOptions options) {
            var swapResolution = options.HasFlag(ResolutionOptions.MatchScreenOrientation) && Screen.height > Screen.width;
            var scaleWidth = options.HasFlag(ResolutionOptions.MatchScreenAspectScaleWidth);
            var scaleHeight = options.HasFlag(ResolutionOptions.MatchScreenAspectScaleHeight);
            var (width, height) = SizeForResolution(resolution);
            (width, height) = swapResolution ? (height, width) : (width, height);
            width = scaleWidth ? (int)((float)Screen.width / Screen.height * height) >> 1 << 1 : width;
            height = scaleHeight ? (int)((float)Screen.height / Screen.width * width) >> 1 << 1 : height;
            return (width, height);
        }

        private static (int sampleRate, int channelCount) GetAudioFormat (AudioMode mode, AudioDevice microphone) => mode switch {
            AudioMode.None          => (0, 0),
            AudioMode.Microphone    => (microphone?.sampleRate ?? 0, microphone?.channelCount ?? 0),
            AudioMode.AudioSource   => (AudioSettings.outputSampleRate, (int)AudioSettings.speakerMode),
            AudioMode.AudioListener => (AudioSettings.outputSampleRate, (int)AudioSettings.speakerMode),
            _                       => (0, 0),
        };

        private static async Task<AudioDevice> GetDefaultMicrophone () {
            // Request microphone permissions
            var permissionStatus = await MediaDeviceQuery.RequestPermissions<AudioDevice>();
            if (permissionStatus != PermissionStatus.Authorized) {
                Debug.LogError(@"VideoKit: User did not grant microphone permissions");
                return null;
            }
            // Discover the front camera
            var query = new MediaDeviceQuery(MediaDeviceCriteria.AudioDevice);
            var device = query.current as AudioDevice;
            // Check
            if (device == null) {
                Debug.LogError(@"VideoKit: Failed to discover any available audio devices");
                return null;
            }
            // Return
            return device;
        }
        #endregion
    }
}