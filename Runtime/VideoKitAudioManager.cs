/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit {

    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Events;
    using NatML.Devices;
    using Internal;

    /// <summary>
    /// VideoKit audio manager for streaming audio from audio devices.
    /// </summary>
    [Tooltip(@"VideoKit audio manager for streaming audio from audio devices."), DisallowMultipleComponent]
    public sealed partial class VideoKitAudioManager : MonoBehaviour, IVideoKitDeviceManager<AudioDevice> {

        #region --Inspector--
        [Header(@"Configuration")]
        /// <summary>
        /// Audio sample rate.
        /// </summary>
        [Tooltip(@"Audio sample rate.")]
        public SampleRate sampleRate = SampleRate.MatchUnity;

        /// <summary>
        /// Audio channel count.
        /// </summary>
        [Tooltip(@"Audio channel count.")]
        public ChannelCount channelCount = ChannelCount.MatchUnity;

        /// <summary>
        /// Request echo cancellation if the device supports it.
        /// </summary>
        [Tooltip(@"Request echo cancellation if the device supports it.")]
        public bool echoCancellation = false;

        [Header(@"Events")]
        /// <summary>
        /// Event raised when a new sample buffer is available.
        /// </summary>
        [Tooltip(@"Event raised when a new sample buffer is available.")]
        public UnityEvent<SampleBuffer> OnSampleBuffer;
        #endregion


        #region --Client API--
        /// <summary>
        /// Get or set the audio device used for streaming.
        /// </summary>
        public AudioDevice device {
            get => audioDevice;
            set {
                // Switch mic without disposing output
                // We deliberately skip configuring the mic like we do in `StartRunning`
                if (running) {
                    audioDevice.StopRunning();
                    audioDevice = value;
                    if (audioDevice != null)
                        audioDevice.StartRunning(OnAudioBuffer);
                    else {
                        StopRunning();
                        Debug.LogError(@"VideoKit: Audio manager failed to start running because audio device is null");
                    }
                }
                // Handle trivial case
                else
                    audioDevice = value;
            }
        }

        /// <summary>
        /// Whether the audio device is running.
        /// </summary>
        public bool running => audioDevice?.running ?? false;

        /// <summary>
        /// Start streaming audio.
        /// </summary>
        public async void StartRunning () {
            // Check
            if (!isActiveAndEnabled) {
                Debug.LogError(@"VideoKit: Audio manager failed to start running because component is disabled");
                return;
            }
            // Check
            if (running)
                return;
            // Request microphone permissions
            var permissionStatus = await MediaDeviceQuery.RequestPermissions<AudioDevice>();
            if (permissionStatus != PermissionStatus.Authorized) {
                Debug.LogError(@"VideoKit: User did not grant microphone permissions");
                return;
            }
            // Check device
            if (audioDevice == null) {
                // Get default device
                audioDevice = GetDefaultAudioDevice();
                if (audioDevice == null) {
                    Debug.LogError(@"VideoKit: Audio manager failed to start running because no audio device is available");
                    return;
                }
                // Configure microphone
                audioDevice.sampleRate = sampleRate == SampleRate.MatchUnity ? AudioSettings.outputSampleRate : (int)sampleRate;
                audioDevice.channelCount = channelCount == ChannelCount.MatchUnity ? (int)AudioSettings.speakerMode : (int)channelCount;
                audioDevice.echoCancellation = echoCancellation; // devices can say they don't support AEC even when they do
            }
            // Start running
            audioDevice.StartRunning(OnAudioBuffer);
        }

        /// <summary>
        /// Stop streaming audio.
        /// </summary>
        public void StopRunning () {
            // Stop
            if (running)
                audioDevice.StopRunning();
        }
        #endregion


        #region --Operations--
        private AudioDevice audioDevice;

        private void OnAudioBuffer (AudioBuffer audioBuffer) {
            var sampleBuffer = new SampleBuffer(audioBuffer);
            OnSampleBuffer?.Invoke(sampleBuffer);
        }

        private void OnDestroy () {
            if (running)
                StopRunning();
        }

        private static AudioDevice GetDefaultAudioDevice () {
            // Create query
            MediaDeviceQuery.ConfigureAudioSession = true;
            var query = new MediaDeviceQuery(MediaDeviceCriteria.AudioDevice);
            // Return
            return query.current as AudioDevice;
        }
        #endregion
    }
}