/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit {

    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;
    using Internal;

    /// <summary>
    /// VideoKit audio manager for streaming audio from audio devices.
    /// </summary>
    [Tooltip(@"VideoKit audio manager for streaming audio from audio devices.")]
    [HelpURL(@"https://videokit.ai/reference/videokitaudiomanager")]
    [DisallowMultipleComponent]
    public sealed class VideoKitAudioManager : MonoBehaviour {

        #region --Enumerations--
        /// <summary>
        /// Audio sample rate.
        /// </summary>
        public enum SampleRate : int {
            /// <summary>
            /// Match Unity's audio DSP sample rate.
            /// </summary>
            MatchUnity = 0,
            /// <summary>
            /// 8KHz.
            /// </summary>
            _8000 = 8000,
            /// <summary>
            /// 16KHz.
            /// </summary>
            _16000 = 16000,
            /// <summary>
            /// 22.05KHz.
            /// </summary>
            _22050 = 22050,
            /// <summary>
            /// 24KHz.
            /// </summary>
            _24000 = 24000,
            /// <summary>
            /// 44.1KHz.
            /// </summary>
            _44100 = 44100,
            /// <summary>
            /// 48KHz.
            /// </summary>
            _48000 = 48000,
        }

        /// <summary>
        /// Audio channel count.
        /// </summary>
        public enum ChannelCount : int {
            /// <summary>
            /// Match Unity's audio DSP channel count.
            /// </summary>
            MatchUnity  = 0,
            /// <summary>
            /// Mono audio.
            /// </summary>
            [InspectorName(@"Mono")]
            _1          = 1,
            /// <summary>
            /// Stereo audio.
            /// </summary>
            [InspectorName(@"Stereo")]
            _2          = 2,
        }
        #endregion


        #region --Inspector--
        [Header(@"Configuration")]
        /// <summary>
        /// Configure the application audio session on awake.
        /// This only applies on iOS.
        /// </summary>
        [Tooltip(@"Configure the application audio session on awake. This only applies on iOS.")]
        public bool configureOnAwake = true;

        [Header(@"Format")]
        /// <summary>
        /// Audio sample rate.
        /// </summary>
        [Tooltip(@"Audio sample rate.")]
        public SampleRate sampleRate = SampleRate._44100;

        /// <summary>
        /// Audio channel count.
        /// </summary>
        [Tooltip(@"Audio channel count.")]
        public ChannelCount channelCount = ChannelCount._1;

        /// <summary>
        /// Request echo cancellation if the device supports it.
        /// </summary>
        [Tooltip(@"Request echo cancellation if the device supports it.")]
        public bool echoCancellation = false;
        #endregion


        #region --Client API--
        /// <summary>
        /// Get or set the audio device used for streaming.
        /// </summary>
        public AudioDevice device {
            get => _device;
            set {
                // Switch mic without disposing output
                // We deliberately skip configuring the mic like we do in `StartRunning`
                if (running) {
                    _device.StopRunning();
                    _device = value;
                    _device?.StartRunning(OnSampleBuffer);
                }
                // Handle trivial case
                else
                    _device = value;
            }
        }

        /// <summary>
        /// Whether the audio device is running.
        /// </summary>
        public bool running => _device?.running ?? false;

        /// <summary>
        /// Event raised when a new audio buffer is available.
        /// </summary>
        public event Action<AudioBuffer> OnAudioBuffer;

        /// <summary>
        /// Start streaming audio.
        /// </summary>
        public async void StartRunning () => await StartRunningAsync();

        /// <summary>
        /// Start streaming audio.
        /// </summary>
        public async Task StartRunningAsync () {
            // Check
            if (!isActiveAndEnabled)
                throw new InvalidOperationException(@"VideoKit: Audio manager failed to start running because component is disabled");
            // Check
            if (running)
                return;
            // Request microphone permissions
            var permissions = await AudioDevice.CheckPermissions(request: true);
            if (permissions != MediaDevice.PermissionStatus.Authorized)
                throw new InvalidOperationException(@"VideoKit: User did not grant microphone permissions");
            // Check device
            var devices = await AudioDevice.Discover(configureAudioSession: false);
            _device ??= devices.FirstOrDefault(); // configure once in `Awake` instead.
            if (_device == null)
                throw new InvalidOperationException(@"VideoKit: Audio manager failed to start running because no audio device is available");
            // Configure microphone
            _device.sampleRate = sampleRate == SampleRate.MatchUnity ? AudioSettings.outputSampleRate : (int)sampleRate;
            _device.channelCount = channelCount == ChannelCount.MatchUnity ? (int)AudioSettings.speakerMode : (int)channelCount;
            _device.echoCancellation = echoCancellation; // devices can say they don't support AEC even when they do
            // Start running
            _device.StartRunning(OnSampleBuffer);
        }

        /// <summary>
        /// Stop streaming audio.
        /// </summary>
        public void StopRunning () {
            // Stop
            if (running)
                _device.StopRunning();
        }
        #endregion


        #region --Operations--
        private AudioDevice _device;

        private void Awake () {
            if (configureOnAwake)
                VideoKit.ConfigureAudioSession();
        }

        private void OnSampleBuffer (AudioBuffer audioBuffer) => OnAudioBuffer?.Invoke(audioBuffer);

        private void OnDestroy () {
            if (running)
                StopRunning();
        }
        #endregion
    }
}