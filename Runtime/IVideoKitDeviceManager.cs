/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit {

    using System.Threading.Tasks;
    using NatML.Devices;

    /// <summary>
    /// VideoKit media device manager.
    /// </summary>
    public interface IVideoKitDeviceManager<T> where T : IMediaDevice {

        /// <summary>
        /// Get or set the media device used for streaming.
        /// If this is `null` the manager will find a suitable default to start streaming.
        /// </summary>
        T device { get; set; }

        /// <summary>
        /// Whether the device manager is streaming.
        /// </summary>
        bool running { get; }

        /// <summary>
        /// Start streamming sample buffers from the current device.
        /// </summary>
        void StartRunning ();

        /// <summary>
        /// Stop streaming sample buffers from the current device.
        /// </summary>
        void StopRunning ();
    }
}