/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Devices {

    using System;
    using System.Linq;

    /// <summary>
    /// Common media device filters.
    /// </summary>
    public static class MediaDeviceFilters {

        #region --Media Type--
        /// <summary>
        /// Filter for audio devices.
        /// </summary>
        public static readonly Predicate<MediaDevice> AudioDevice = device => device is AudioDevice;

        /// <summary>
        /// Filter for camera devices.
        /// </summary>
        public static readonly Predicate<MediaDevice> CameraDevice = device => device is CameraDevice;
        #endregion


        #region --Device Location and Type--
        /// <summary>
        /// Filter for internal devices.
        /// </summary>
        public static readonly Predicate<MediaDevice> Internal = device => device.location == MediaDevice.Location.Internal;

        /// <summary>
        /// Filter for external devices.
        /// </summary>
        public static readonly Predicate<MediaDevice> External = device => device.location == MediaDevice.Location.External;

        /// <summary>
        /// Filter for default devices for their respective media types.
        /// </summary>
        public static readonly Predicate<MediaDevice> Default = device => device.defaultForMediaType;
        #endregion


        #region --AudioDevice--
        /// <summary>
        /// Filter for audio devices that perform echo cancellation.
        /// </summary>
        public static readonly Predicate<MediaDevice> EchoCancellation = device => device is AudioDevice microphone && microphone.echoCancellationSupported;
        #endregion
        

        #region --CameraDevice--
        /// <summary>
        /// Filter for rear-facing camera devices.
        /// </summary>
        public static readonly Predicate<MediaDevice> RearCamera = device => device is CameraDevice camera && !camera.frontFacing;

        /// <summary>
        /// Filter for front-facing camera devices.
        /// </summary>
        public static readonly Predicate<MediaDevice> FrontCamera = device => device is CameraDevice camera && camera.frontFacing;

        /// <summary>
        /// Filter for camera devices that can stream depth images.
        /// </summary>
        internal static readonly Predicate<MediaDevice> DepthCamera = device => device is CameraDevice camera && camera.depthStreamingSupported;
        
        /// <summary>
        /// Filter for camera devices that have a torch unit.
        /// </summary>
        public static readonly Predicate<MediaDevice> Torch = device => device is CameraDevice camera && camera.torchSupported;
        #endregion


        #region --Utilities--
        /// <summary>
        /// Filter for devices that meet any of the provided criteria.
        /// </summary>
        /// <param name="criteria">Criteria to meet.</param>
        public static Predicate<MediaDevice> Any (params Predicate<MediaDevice>[] criteria) => device => criteria.Any(c => c(device));

        /// <summary>
        /// Filter for devices that meet all of the provided criteria.
        /// </summary>
        /// <param name="criteria">Criteria to meet.</param>
        public static Predicate<MediaDevice> All (params Predicate<MediaDevice>[] criteria) => device => criteria.All(c => c(device));
        #endregion
    }
}