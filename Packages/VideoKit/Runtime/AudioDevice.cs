/* 
*   VideoKit
*   Copyright © 2024 NatML Inc. All Rights Reserved.
*/

#nullable enable

namespace VideoKit {

    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using AOT;
    using UnityEngine;
    using Internal;
    using MediaDeviceFlags = Internal.VideoKit.MediaDeviceFlags;
    using Status = Internal.VideoKit.Status;

    /// <summary>
    /// Audio input device.
    /// </summary>
    public sealed class AudioDevice : MediaDevice {

        #region --Properties--
        /// <summary>
        /// Is echo cancellation supported?
        /// </summary>
        public bool echoCancellationSupported => device.GetMediaDeviceFlags(out var flags).Throw() == Status.Ok ? flags.HasFlag(MediaDeviceFlags.EchoCancellation) : default;

        /// <summary>
        /// Enable or disable Adaptive Echo Cancellation (AEC).
        /// </summary>
        public bool echoCancellation {
            get => device.GetAudioDeviceEchoCancellation(out var echoCancellation) == Status.Ok ? echoCancellation : default;
            set => device.SetAudioDeviceEchoCancellation(value);
        }

        /// <summary>
        /// Audio sample rate.
        /// </summary>
        public int sampleRate {
            get => device.GetAudioDeviceSampleRate(out var sampleRate).Throw() == Status.Ok ? sampleRate : default;
            set => device.SetAudioDeviceSampleRate(value).Throw();
        }

        /// <summary>
        /// Audio channel count.
        /// </summary>
        public int channelCount {
            get => device.GetAudioDeviceChannelCount(out var channelCount).Throw() == Status.Ok ? channelCount : default;
            set => device.SetAudioDeviceChannelCount(value).Throw();
        }
        #endregion


        #region --Streaming--
        /// <summary>
        /// Start running.
        /// NOTE: This requires an active VideoKit plan.
        /// </summary>
        /// <param name="handler">Delegate to receive audio buffers.</param>
        public unsafe void StartRunning (Action<AudioBuffer> handler) => StartRunning((IntPtr sampleBuffer) => {
            handler(new AudioBuffer(sampleBuffer));
        });
        #endregion


        #region --Discovery--
        /// <summary>
        /// Check the current microphone permission status.
        /// </summary>
        /// <param name="request">Request permissions if the user has not yet been asked.</param>
        /// <returns>Current microphone permissions status.</returns>
        public static Task<PermissionStatus> CheckPermissions (bool request = true) => CheckPermissions(
            VideoKit.PermissionType.Microphone,
            request
        );

        /// <summary>
        /// Discover available audio input devices.
        /// </summary>
        /// <param name="configureAudioSession">Configure the application's global audio session for audio device discovery. This is required for discovering audio devices on iOS.</param>
        public static async Task<AudioDevice[]> Discover (bool configureAudioSession = true) {
            // Check session
            await VideoKitClient.Instance!.CheckSession();
            // Configure audio session
            if (configureAudioSession)
                VideoKit.ConfigureAudioSession();
            // Discover
            var tcs = new TaskCompletionSource<AudioDevice[]>();
            var handle = GCHandle.Alloc(tcs, GCHandleType.Normal);
            try {
                VideoKit.DiscoverAudioDevices(OnDiscoverDevices, (IntPtr)handle).Throw();
                return await tcs.Task;
            } catch {
                handle.Free();
                throw;
            }
        }
        #endregion


        #region --Operations--

        private int priority { // #24
            get {
                var order = 0;
                if (!defaultForMediaType)
                    order += 1;
                if (location == Location.External)
                    order += 10;
                if (location == Location.Unknown)
                    order += 100;
                return order;
            }
        }

        internal AudioDevice (IntPtr device) : base(device) { }

        public override string ToString () => $"AudioDevice(uniqueId=\"{uniqueId}\", name=\"{name}\")";
        #endregion


        #region --Callbacks--

        [MonoPInvokeCallback(typeof(VideoKit.MediaDeviceDiscoveryHandler))]
        private static unsafe void OnDiscoverDevices (IntPtr context, IntPtr devices, int count) {
            try {
                // Check
                if (!VideoKit.IsAppDomainLoaded)
                    return;
                // Get tcs
                var handle = (GCHandle)context;
                var tcs = handle.Target as TaskCompletionSource<AudioDevice[]>;
                handle.Free();
                // Complete task
                var microphones = Enumerable
                    .Range(0, count)
                    .Select(idx => new AudioDevice(((IntPtr*)devices)[idx]))
                    .OrderBy(microphone => microphone.priority)
                    .ToArray();
                tcs?.SetResult(microphones);
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }
        #endregion
    }
}