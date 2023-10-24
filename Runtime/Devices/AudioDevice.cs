/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Devices {

    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using AOT;
    using UnityEngine;
    using Internal;
    using Utilities;
    using DeviceFlags = Internal.VideoKit.DeviceFlags;

    /// <summary>
    /// Hardware audio input device.
    /// </summary>
    public sealed class AudioDevice : MediaDevice {

        #region --Properties--
        /// <summary>
        /// Is echo cancellation supported?
        /// </summary>
        public bool echoCancellationSupported => device.Flags().HasFlag(DeviceFlags.EchoCancellation);

        /// <summary>
        /// Enable or disable Adaptive Echo Cancellation (AEC).
        /// </summary>
        public bool echoCancellation {
            get => device.EchoCancellation();
            set => device.SetEchoCancellation(value);
        }

        /// <summary>
        /// Audio sample rate.
        /// </summary>
        public int sampleRate {
            get => device.SampleRate();
            set => device.SetSampleRate(value);
        }

        /// <summary>
        /// Audio channel count.
        /// </summary>
        public int channelCount {
            get => device.ChannelCount();
            set => device.SetChannelCount(value);
        }
        #endregion


        #region --Streaming--
        /// <summary>
        /// Start running.
        /// NOTE: This requires an active VideoKit plan.
        /// </summary>
        /// <param name="handler">Delegate to receive audio buffers.</param>
        public unsafe void StartRunning (Action<AudioBuffer> handler) {
            Action<IntPtr> wrapper = sampleBuffer => handler?.Invoke(new AudioBuffer(this, sampleBuffer));
            streamHandle = GCHandle.Alloc(wrapper, GCHandleType.Normal);
            lifecycleHelper = LifecycleHelper.Create();
            lifecycleHelper.onQuit += StopRunning;
            try {
                device.StartRunning(OnAudioBuffer, (IntPtr)streamHandle).CheckStatus();
            } catch (Exception ex) {
                streamHandle.Free();
                streamHandle = default;
                throw;
            }
        }

        /// <summary>
        /// Stop running.
        /// </summary>
        public override void StopRunning () {
            if (lifecycleHelper)
                lifecycleHelper.Dispose();
            device.StopRunning().CheckStatus();
            if (streamHandle != default)
                streamHandle.Free();
            streamHandle = default;
            lifecycleHelper = default;
        }
        #endregion


        #region --Discovery--
        /// <summary>
        /// Check the current microphone permission status.
        /// </summary>
        /// <param name="request">Request permissions if the user has not yet been asked.</param>
        /// <returns>Current microphone permissions status.</returns>
        public static Task<PermissionStatus> CheckPermissions (bool request = true) => CheckPermissions(VideoKit.PermissionType.Microphone, request);

        /// <summary>
        /// Discover available audio input devices.
        /// </summary>
        /// <param name="configureAudioSession">Configure the application's global audio session for audio device discovery. This is required for discovering audio devices on iOS.</param>
        public static async Task<AudioDevice[]> Discover (bool configureAudioSession = true) {
            // Check session
            await VideoKitSettings.Instance.CheckSession();
            // Configure audio session
            if (configureAudioSession)
                VideoKitExt.ConfigureAudioSession();
            // Discover
            var devices = await DiscoverNative();
            // Return
            return devices;
        }
        #endregion


        #region --Operations--
        private GCHandle streamHandle;
        private LifecycleHelper lifecycleHelper;

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

        public override string ToString () => $"microphone:{uniqueID}";

        private static Task<AudioDevice[]> DiscoverNative () {
            // Discover
            var tcs = new TaskCompletionSource<AudioDevice[]>();
            var handle = GCHandle.Alloc(tcs, GCHandleType.Normal);
            try {
                VideoKit.DiscoverMicrophones(OnDiscoverMicrophones, (IntPtr)handle).CheckStatus();
            } catch (Exception ex) {
                handle.Free();
                tcs.SetException(ex);
            }
            // Return
            return tcs.Task;
        }

        [MonoPInvokeCallback(typeof(VideoKit.DeviceDiscoveryHandler))]
        private static unsafe void OnDiscoverMicrophones (IntPtr context, IntPtr devices, int count) {
            // Check
            if (!VideoKit.IsAppDomainLoaded)
                return;
            // Get tcs
            TaskCompletionSource<AudioDevice[]> tcs;
            try {
                var handle = (GCHandle)context;
                tcs = handle.Target as TaskCompletionSource<AudioDevice[]>;
                handle.Free();
            } catch (Exception ex) {
                Debug.LogException(ex);
                return;
            }
            // Invoke
            var microphones = Enumerable
                .Range(0, count)
                .Select(idx => new AudioDevice(((IntPtr*)devices)[idx]))
                .OrderBy(microphone => microphone.priority)
                .ToArray();
            tcs?.SetResult(microphones);
        }

        [MonoPInvokeCallback(typeof(VideoKit.SampleBufferHandler))]
        private static void OnAudioBuffer (IntPtr context, IntPtr sampleBuffer) {
            // Check
            if (!VideoKit.IsAppDomainLoaded)
                return;
            // Invoke
            try {
                var handle = (GCHandle)context;
                var handler = handle.Target as Action<IntPtr>;
                handler?.Invoke(sampleBuffer);
            }
            catch (Exception ex) {
                Debug.LogException(ex);
            }
        }
        #endregion
    }
}