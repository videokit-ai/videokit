/* 
*   VideoKit
*   Copyright © 2026 Yusuf Olokoba. All Rights Reserved.
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
        /// Whether acoustic echo cancellation is supported.
        /// </summary>
        public bool echoCancellationSupported => handle.GetMediaDeviceFlags(out var flags).Throw() == Status.Ok ? flags.HasFlag(MediaDeviceFlags.EchoCancellation) : default;

        /// <summary>
        /// Enable or disable acoustic echo cancellation (AEC).
        /// </summary>
        public bool echoCancellation {
            get => handle.GetAudioDeviceEchoCancellation(out var echoCancellation) == Status.Ok ? echoCancellation : default;
            set => handle.SetAudioDeviceEchoCancellation(value);
        }

        /// <summary>
        /// Audio sample rate.
        /// </summary>
        public int sampleRate {
            get => handle.GetAudioDeviceSampleRate(out var sampleRate).Throw() == Status.Ok ? sampleRate : default;
            set => handle.SetAudioDeviceSampleRate(value).Throw();
        }

        /// <summary>
        /// Audio channel count.
        /// </summary>
        public int channelCount {
            get => handle.GetAudioDeviceChannelCount(out var channelCount).Throw() == Status.Ok ? channelCount : default;
            set => handle.SetAudioDeviceChannelCount(value).Throw();
        }
        #endregion


        #region --Streaming--
        /// <summary>
        /// Start running.
        /// NOTE: This requires an active VideoKit plan.
        /// </summary>
        /// <param name="handler">Delegate to receive audio buffers.</param>
        public unsafe void StartRunning(Action<AudioBuffer> handler) => StartRunning((IntPtr sampleBuffer) => {
            handler(new AudioBuffer(sampleBuffer));
        });
        #endregion


        #region --Discovery--
        /// <summary>
        /// Check the current microphone permission status.
        /// </summary>
        /// <param name="request">Request permissions if the user has not yet been asked.</param>
        /// <returns>Current microphone permissions status.</returns>
        public static Task<PermissionStatus> CheckPermissions(bool request = true) => CheckPermissions(
            VideoKit.PermissionType.Microphone,
            request
        );

        /// <summary>
        /// Discover available audio input devices.
        /// </summary>
        /// <param name="configureAudioSession">Configure the application's global audio session for audio device discovery. This is required for discovering audio devices on iOS.</param>
        public static async Task<AudioDevice[]> Discover(bool configureAudioSession = true) {
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

        private int priority => location switch {
            var _ when defaultForMediaType  => -1000,
            Location.External               => -1,
            Location.Internal               => 0,
            Location.Unknown                => 1,
            _                               => 2
        };

        internal AudioDevice(IntPtr device, bool strong = true) : base(device, strong: strong) { }

        public override string ToString() => $"AudioDevice(uniqueId=\"{uniqueId}\", name=\"{name}\")";

        [MonoPInvokeCallback(typeof(VideoKit.MediaDeviceDiscoveryHandler))]
        private static unsafe void OnDiscoverDevices(IntPtr context, IntPtr devices, int count) {
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
                    .Select(idx => ((IntPtr*)devices)[idx])
                    .Select(device => new AudioDevice(device, strong: true))
                    .OrderBy(device => device.priority)
                    .ToArray();
                tcs?.SetResult(microphones);
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }
        #endregion
    }
}