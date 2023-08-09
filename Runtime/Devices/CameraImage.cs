/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Devices {

    using System;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Internal;
    using Metadata = Internal.VideoKit.Metadata;

    /// <summary>
    /// Camera image provided by a camera device.
    /// The camera image always contains a pixel buffer along with image metadata.
    /// The format of the pixel buffer varies by platform and must be taken into consideration when using the pixel data.
    /// </summary>
    public readonly partial struct CameraImage {

        #region --Enumerations--
        /// <summary>
        /// Image buffer format.
        /// </summary>
        public enum Format { // CHECK // VideoKit.h
            /// <summary>
            /// Unknown image format.
            /// </summary>
            Unknown = 0,
            /// <summary>
            /// Generic YUV 420 planar format.
            /// </summary>
            YCbCr420 = 1,
            /// <summary>
            /// RGBA8888.
            /// </summary>
            RGBA8888 = 2,
            /// <summary>
            /// BGRA8888.
            /// </summary>
            BGRA8888 = 3,    
        }
        #endregion


        #region --Types--
        /// <summary>
        /// Image plane for planar formats.
        /// </summary>
        public readonly struct Plane {

            /// <summary>
            /// Pixel buffer.
            /// </summary>
            public readonly NativeArray<byte> buffer;

            /// <summary>
            /// Plane width.
            /// </summary>
            public readonly int width;

            /// <summary>
            /// Plane height.
            /// </summary>
            public readonly int height;

            /// <summary>
            /// Row stride in bytes.
            /// </summary>
            public readonly int rowStride;
            
            /// <summary>
            /// Pixel stride in bytes.
            /// </summary>
            public readonly int pixelStride;

            /// <summary>
            /// Create a camera image plane.
            /// </summary>
            public Plane (NativeArray<byte> buffer, int width, int height, int rowStride, int pixelStride) {
                this.buffer = buffer;
                this.width = width;
                this.height = height;
                this.rowStride = rowStride;
                this.pixelStride = pixelStride;
            }
        }
        #endregion


        #region --Properties--
        /// <summary>
        /// Camera device that this image was generated from.
        /// </summary>
        public readonly CameraDevice device;

        /// <summary>
        /// Image pixel buffer.
        /// Some planar images might not have a contiguous pixel buffer.
        /// In this case, the buffer is uninitialized.
        /// </summary>
        public readonly NativeArray<byte> pixelBuffer;

        /// <summary>
        /// Image format.
        /// </summary>
        public readonly Format format;

        /// <summary>
        /// Image width.
        /// </summary>
        public readonly int width;

        /// <summary>
        /// Image height.
        /// </summary>
        public readonly int height;

        /// <summary>
        /// Image row stride in bytes.
        /// This is zero if the image is planar.
        /// </summary>
        public readonly int rowStride;

        /// <summary>
        /// Image timestamp in nanoseconds.
        /// The timestamp is based on the system media clock.
        /// </summary>
        public readonly long timestamp;

        /// <summary>
        /// Whether the image is vertically mirrored.
        /// </summary>
        public readonly bool verticallyMirrored;

        /// <summary>
        /// Image plane for planar formats.
        /// This is `null` for interleaved formats.
        /// </summary>
        public readonly Plane[] planes;

        /// <summary>
        /// Camera intrinsics as a flattened row-major 3x3 matrix.
        /// This is `null` if the camera does not report it.
        /// </summary>
        public readonly float[] intrinsics;

        /// <summary>
        /// Exposure bias value in EV.
        /// This is `null` if the camera does not report it.
        /// </summary>
        public readonly float? exposureBias;

        /// <summary>
        /// Image exposure duration in seconds.
        /// This is `null` if the camera does not report it.
        /// </summary>
        public readonly float? exposureDuration;

        /// <summary>
        /// Sensor sensitivity ISO value.
        /// This is `null` if the camera does not report it.
        /// </summary>
        public readonly float? ISO;

        /// <summary>
        /// Camera focal length in millimeters.
        /// This is `null` if the camera does not report it.
        /// </summary>
        public readonly float? focalLength;

        /// <summary>
        /// Image aperture, in f-number.
        /// This is `null` if the camera does not report it.
        /// </summary>
        public readonly float? fNumber;

        /// <summary>
        /// Ambient brightness.
        /// This is `null` if the camera does not report it.
        /// </summary>
        public readonly float? brightness;

        /// <summary>
        /// Safely clone the camera image.
        /// The clone will never contain a valid pixel buffer or plane data.
        /// </summary>
        public CameraImage Clone () => new CameraImage(
            device,
            default,
            format,
            width,
            height,
            0,
            timestamp,
            verticallyMirrored,
            null,
            intrinsics,
            exposureBias,
            exposureDuration,
            ISO,
            focalLength,
            fNumber,
            brightness
        );
        #endregion


        #region --Operations--
        internal readonly IntPtr nativeImage;

        /// <summary>
        /// Create a camera image.
        /// </summary>
        internal unsafe CameraImage (
            CameraDevice device,
            IntPtr image
        ) {
            this.device = device;
            this.nativeImage = image;
            this.format = image.CameraImageFormat();
            this.width = image.CameraImageWidth();
            this.height = image.CameraImageHeight();
            this.rowStride = image.CameraImageRowStride();
            this.timestamp = image.CameraImageTimestamp();
            this.verticallyMirrored = image.CameraImageVerticallyMirrored();
            // Pixel buffer
            this.pixelBuffer = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(
                image.CameraImageData(),
                image.CameraImageDataSize(),
                Allocator.None
            );
            #if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref pixelBuffer, AtomicSafetyHandle.Create());
            #endif
            // Planes
            var planeCount = image.CameraImagePlaneCount();
            this.planes = planeCount > 0 ? new CameraImage.Plane[planeCount] : null;
            for (var i = 0; i < planes?.Length; ++i) {
                var planeBuffer = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(
                    image.CameraImagePlaneData(i),
                    image.CameraImagePlaneDataSize(i),
                    Allocator.None
                );
                #if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref planeBuffer, AtomicSafetyHandle.Create());
                #endif
                planes[i] = new CameraImage.Plane(
                    planeBuffer,
                    image.CameraImagePlaneWidth(i),
                    image.CameraImagePlaneHeight(i),
                    image.CameraImagePlaneRowStride(i),
                    image.CameraImagePlanePixelStride(i)
                );
            }
            // Intrinsics
            var intrinsics = new float[9];
            fixed (float* dst = intrinsics)
                this.intrinsics = image.CameraImageMetadata(Metadata.IntrinsicMatrix, dst, intrinsics.Length) ? intrinsics : null;
            // Metadata
            var metadata = 0f;            
            this.exposureBias = image.CameraImageMetadata(Metadata.ExposureBias, &metadata) ? (float?)metadata : null;
            this.exposureDuration = image.CameraImageMetadata(Metadata.ExposureDuration, &metadata) ? (float?)metadata : null;
            this.ISO = image.CameraImageMetadata(Metadata.ISO, &metadata) ? (float?)metadata : null;
            this.focalLength = image.CameraImageMetadata(Metadata.FocalLength, &metadata) ? (float?)metadata : null;
            this.fNumber = image.CameraImageMetadata(Metadata.FNumber, &metadata) ? (float?)metadata : null;
            this.brightness = image.CameraImageMetadata(Metadata.Brightness, &metadata) ? (float?)metadata : null;
        }

        /// <summary>
        /// Create a camera image.
        /// </summary>
        private CameraImage (
            CameraDevice device,
            NativeArray<byte> pixelBuffer,
            Format format,
            int width,
            int height,
            int rowStride,
            long timestamp,
            bool mirrored,
            Plane[] planes = null,
            float[] intrinsics = null,
            float? exposureBias = null,
            float? exposureDuration = null,
            float? ISO = null,
            float? focalLength = null,
            float? fNumber = null,
            float? brightness = null,
            IntPtr nativeImage = default
        ) {
            this.device = device;
            this.pixelBuffer = pixelBuffer;
            this.format = format;
            this.width = width;
            this.height = height;
            this.rowStride = rowStride;
            this.timestamp = timestamp;
            this.verticallyMirrored = mirrored;
            this.planes = planes;
            this.intrinsics = intrinsics;
            this.exposureBias = exposureBias;
            this.exposureDuration = exposureDuration;
            this.ISO = ISO;
            this.focalLength = focalLength;
            this.fNumber = fNumber;
            this.brightness = brightness;
            this.nativeImage = nativeImage;
        }
        #endregion
    }
}