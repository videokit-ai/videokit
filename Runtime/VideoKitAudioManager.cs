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
    public sealed partial class VideoKitAudioManager : MonoBehaviour {

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
        /// Get ot set the audio device used to record.
        /// </summary>
        public AudioDevice audioDevice {
            get => device;
            set {
                var restart = running;
                StopRunning();
                device = value;
                if (restart)
                    StartRunning();
            }
        }

        /// <summary>
        /// Whether the audio device is running.
        /// </summary>
        public bool running => audioDevice?.running ?? false;

        /// <summary>
        /// Start streaming audio.
        /// </summary>
        public async Task StartRunning () {
            // Check
            if (!isActiveAndEnabled) {
                Debug.LogError(@"VideoKit: Audio manager failed to start running because component is disabled");
                return;
            }
            // Check
            if (running)
                return;
            // Get device
            device ??= await GetDefaultAudioDevice();
            if (device == null) {
                Debug.LogError(@"VideoKit: Audio manager failed to start because no audio device is available");
                return;
            }
            // Start running
            device.sampleRate = sampleRate.ToInt();
            device.channelCount = channelCount.ToInt();
            device.echoCancellation = echoCancellation; // devices can say they don't support AEC even when they do
            device.StartRunning(OnAudioBuffer);
        }

        /// <summary>
        /// Stop streaming audio.
        /// </summary>
        public void StopRunning () {
            if (running)
                device.StopRunning();
        }
        #endregion


        #region --Operations--
        private AudioDevice device;

        private void OnAudioBuffer (AudioBuffer audioBuffer) {
            var sampleBuffer = new SampleBuffer(audioBuffer);
            OnSampleBuffer?.Invoke(sampleBuffer);
        }

        private void OnDestroy () {
            if (running)
                StopRunning();
        }

        private static async Task<AudioDevice> GetDefaultAudioDevice () {
            // Request microphone permissions
            var permissionStatus = await MediaDeviceQuery.RequestPermissions<AudioDevice>();
            if (permissionStatus != PermissionStatus.Authorized) {
                Debug.LogError(@"VideoKit: User did not grant microphone permissions");
                return null;
            }
            // Discover the front camera
            var query = new MediaDeviceQuery(MediaDeviceCriteria.AudioDevice);
            var device = query.current;
            // Check
            if (device == null)
                Debug.LogError(@"VideoKit: Failed to discover any available audio device");
            // Return
            return device as AudioDevice;
        }
        #endregion
    }
}