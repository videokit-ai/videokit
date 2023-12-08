/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

#nullable enable

namespace VideoKit {

    using System.Threading.Tasks;
    using UnityEngine;
    using Devices;

    /// <summary>
    /// VideoKit media device manager.
    /// </summary>
    public abstract class VideoKitDeviceManager<T> : MonoBehaviour where T : MediaDevice {

        /// <summary>
        /// Get or set the media device used for streaming.
        /// If this is `null` the manager will find a suitable default to start streaming.
        /// </summary>
        public abstract T? device { get; set; }

        /// <summary>
        /// Whether the device manager is streaming.
        /// </summary>
        public abstract bool running { get; }

        /// <summary>
        /// Start streamming sample buffers from the current device.
        /// </summary>
        public abstract Task StartRunning ();

        /// <summary>
        /// Stop streaming sample buffers from the current device.
        /// </summary>
        public abstract void StopRunning ();
    }
}