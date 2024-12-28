/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using UnityEngine;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Newtonsoft.Json;
    using Internal;
    using Status = Internal.VideoKit.Status;

    /// <summary>
    /// Pixel buffer.
    /// The format of the pixel buffer varies by platform and must be taken into consideration when using the pixel data.
    /// The pixel buffer may contain a metadata dictionary.
    /// NOTE: The pixel buffer NEVER owns its own memory.
    /// </summary>
    public unsafe readonly struct PixelBuffer : IDisposable {

        #region --Enumerations--
        /// <summary>
        /// Pixel buffer format.
        /// </summary>
        public enum Format : int { // CHECK // VideoKit.h
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

        public enum Rotation : int { // CHECK // VideoKit.h
            /// <summary>
            /// No rotation.
            /// </summary>
            _0 = 0,
            /// <summary>
            /// Rotate 90 degrees counter-clockwise.
            /// </summary>
            _90 = 1,
            /// <summary>
            /// Rotate 180 degrees.
            /// </summary>
            _180 = 2,
            /// <summary>
            /// Rotate 270 degrees counter-clockwise.
            /// </summary>
            _270 = 3
        }
        #endregion


        #region --Types--
        /// <summary>
        /// Pixel buffer plane for planar formats.
        /// </summary>
        public readonly struct Plane {

            #region --Properties--
            /// <summary>
            /// Pixel data.
            /// </summary>
            public unsafe readonly NativeArray<byte> data {
                get {
                    pixelBuffer.GetPixelBufferPlaneData(index, out var data).Throw();
                    pixelBuffer.GetPixelBufferPlaneDataSize(index, out var size).Throw();
                    var nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(
                        data,
                        size,
                        Allocator.None
                    );
                    #if ENABLE_UNITY_COLLECTIONS_CHECKS
                    NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, AtomicSafetyHandle.Create());
                    #endif
                    return nativeArray;
                }
            }

            /// <summary>
            /// Plane width.
            /// </summary>
            public readonly int width => pixelBuffer.GetPixelBufferPlaneWidth(index, out var width).Throw() == Status.Ok ? width : default;

            /// <summary>
            /// Plane height.
            /// </summary>
            public readonly int height => pixelBuffer.GetPixelBufferPlaneHeight(index, out var height).Throw() == Status.Ok ? height : default;

            /// <summary>
            /// Row stride in bytes.
            /// </summary>
            public readonly int rowStride => pixelBuffer.GetPixelBufferPlaneRowStride(index, out var stride).Throw() == Status.Ok ? stride : default;
            
            /// <summary>
            /// Pixel stride in bytes.
            /// </summary>
            public readonly int pixelStride => pixelBuffer.GetPixelBufferPlanePixelStride(index, out var stride).Throw() == Status.Ok ? stride : default;
            #endregion


            #region --Operations--
            private readonly IntPtr pixelBuffer;
            private readonly int index;

            internal Plane (IntPtr pixelBuffer, int index) {
                this.pixelBuffer = pixelBuffer;
                this.index = index;
            }
            #endregion
        }
        #endregion


        #region --Properties--
        /// <summary>
        /// Pixel data.
        /// Some planar pixel buffers might not have contiguous data.
        /// In this case, the data is uninitialized.
        /// </summary>
        public unsafe readonly NativeArray<byte> data {
            get {
                if (dataBuffer.IsCreated)
                    return dataBuffer;
                pixelBuffer.GetPixelBufferData(out var data);
                pixelBuffer.GetPixelBufferDataSize(out var size);
                if (data == default)
                    return default;
                var nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(
                    data,
                    size,
                    Allocator.None
                );
                #if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, AtomicSafetyHandle.Create());
                #endif
                return nativeArray;
            }
        }

        /// <summary>
        /// Pixel buffer format.
        /// </summary>
        public readonly Format format => pixelBuffer.GetPixelBufferFormat(out var format) == Status.Ok ? format : default;

        /// <summary>
        /// Pixel buffer width.
        /// </summary>
        public readonly int width => pixelBuffer.GetPixelBufferWidth(out var width) == Status.Ok ? width : default;

        /// <summary>
        /// Pixel buffer height.
        /// </summary>
        public readonly int height => pixelBuffer.GetPixelBufferHeight(out var height) == Status.Ok ? height : default;

        /// <summary>
        /// Pixel buffer row stride in bytes.
        /// This is zero if the pixel data is planar.
        /// </summary>
        public readonly int rowStride => pixelBuffer.GetPixelBufferRowStride(out var stride) == Status.Ok ? stride : default;

        /// <summary>
        /// Pixel buffer timestamp in nanoseconds.
        /// The timestamp is based on the system media clock.
        /// </summary>
        public readonly long timestamp => pixelBuffer.GetSampleBufferTimestamp(out var timestamp) == Status.Ok ? timestamp : default;

        /// <summary>
        /// Whether the pixel buffer is vertically mirrored.
        /// </summary>
        public readonly bool verticallyMirrored => pixelBuffer.GetPixelBufferIsVerticallyMirrored(out var mirrored) == Status.Ok ? mirrored : default;

        /// <summary>
        /// Pixel buffer planes for planar formats.
        /// This is `null` for interleaved formats.
        /// </summary>
        public readonly IReadOnlyList<Plane>? planes {
            get {
                var planes = new NativePlanes(this) as IReadOnlyList<Plane>;
                return planes.Count > 0 ? planes : null;
            }
        }

        /// <summary>
        /// Pixel buffer metadata.
        /// A new dictionary is returned every time this property is accessed.
        /// This is `null` if the pixel buffer has no metadata.
        /// </summary>
        public readonly Dictionary<string, object>? metadata {
            get {
                var sb = new StringBuilder(8192);
                if (pixelBuffer.CopyPixelBufferMetadata(sb, sb.Capacity) != Status.Ok)
                    return null;
                var metadata = JsonConvert.DeserializeObject<Dictionary<string, object>>(sb.ToString());
                return metadata;
            }
        }
        #endregion


        #region --Lifecycle--
        /// <summary>
        /// Create a pixel buffer from a texture.
        /// </summary>
        /// <param name="texture">Input texture.</param>
        /// <param name="timestamp">Pixel buffer timestamp.</param>
        /// <param name="metadata">Pixel buffer metadata.</param>
        /// <returns>Created pixel buffer.</returns>
        public PixelBuffer (Texture2D texture, long timestamp = 0L) : this(
            width: texture.width,
            height: texture.height,
            format: ToImageFormat(texture.format),
            data: texture.GetRawTextureData<byte>(),
            timestamp: timestamp,
            mirrored: false
        ) { }

        /// <summary>
        /// Create an interleaved pixel buffer from pixel data in native memory.
        /// NOTE: This overload makes a copy of the input buffer, so prefer using the other overloads instead.
        /// </summary>
        /// <param name="width">Pixel buffer width.</param>
        /// <param name="height">Pixel buffer height.</param>
        /// <param name="format">Pixel buffer format. This MUST be `RGBA8888`.</param>
        /// <param name="data">Pixel data.</param>
        /// <param name="rowStride">Pixel buffer row stride.</param>
        /// <param name="timestamp">Pixel buffer timestamp.</param>
        /// <param name="mirrored">Whether the pixel buffer is vertically mirrored.</param>
        /// <returns>Created pixel buffer.</returns>
        public PixelBuffer (
            int width,
            int height,
            Format format,
            byte[] data,
            int rowStride = 0,
            long timestamp = 0L,
            bool mirrored = false
        ) : this(
            width: width,
            height: height,
            format: format,
            rowStride: rowStride,
            timestamp: timestamp,
            mirrored: mirrored
        ) => dataBuffer.CopyFrom(data);

        /// <summary>
        /// Create an interleaved pixel buffer from pixel data.
        /// </summary>
        /// <param name="width">Pixel buffer width.</param>
        /// <param name="height">Pixel buffer height.</param>
        /// <param name="format">Pixel buffer format. This MUST be `RGBA8888`.</param>
        /// <param name="data">Pixel data.</param>
        /// <param name="rowStride">Pixel buffer row stride.</param>
        /// <param name="timestamp">Pixel buffer timestamp.</param>
        /// <param name="mirrored">Whether the pixel buffer is vertically mirrored.</param>
        /// <returns>Created pixel buffer.</returns>
        public unsafe PixelBuffer (
            int width,
            int height,
            Format format,
            NativeArray<byte> data,
            int rowStride = 0,
            long timestamp = 0L,
            bool mirrored = false
        ) : this(
            width: width,
            height: height,
            format: format,
            data: (byte*)data.GetUnsafePtr(),
            rowStride: rowStride,
            timestamp: timestamp,
            mirrored: mirrored
        ) { }

        /// <summary>
        /// Create an interleaved pixel buffer from pixel data in native memory.
        /// NOTE: Do not use this overload unless you know what you are doing!
        /// </summary>
        /// <param name="width">Pixel buffer width.</param>
        /// <param name="height">Pixel buffer height.</param>
        /// <param name="format">Pixel buffer format. This MUST be `RGBA8888`.</param>
        /// <param name="data">Pixel data.</param>
        /// <param name="rowStride">Pixel buffer row stride.</param>
        /// <param name="timestamp">Pixel bufffer timestamp.</param>
        /// <param name="mirrored">Whether the pixel buffer is vertically mirrored.</param>
        /// <returns>Created pixel buffer.</returns>
        public unsafe PixelBuffer (
            int width,
            int height,
            Format format,
            byte* data,
            int rowStride = 0,
            long timestamp = 0L,
            bool mirrored = false
        ) {
            VideoKit.CreatePixelBuffer(
                width,
                height,
                format,
                data,
                rowStride > 0 ? rowStride : GetDefaultStride(format, width),
                timestamp,
                mirrored,
                out pixelBuffer
            ).Throw();
            dataBuffer = default;
        }

        /// <summary>
        /// Dispose the pixel buffer and release resources.
        /// </summary>
        public void Dispose () {
            pixelBuffer.ReleaseSampleBuffer();
            dataBuffer.Dispose();
        }
        #endregion


        #region --Processing--
         /// <summary>
        /// Copy pixel data into a destination pixel buffer.
        /// </summary>
        /// <param name="destination">Destination pixel buffer.</param>
        /// <param name="rotation">Rotation to apply when copying pixel data.</param>
        public void CopyTo (
            PixelBuffer destination,
            Rotation rotation = Rotation._0
        ) => pixelBuffer.CopyToPixelBuffer(destination, rotation).Throw();
        #endregion


        #region --Operations--
        private readonly IntPtr pixelBuffer;
        private readonly NativeArray<byte> dataBuffer;

        internal PixelBuffer (IntPtr pixelBuffer) {
            this.pixelBuffer = pixelBuffer;
            this.dataBuffer = default;
        }

        internal PixelBuffer ( // might wanna make this public
            int width,
            int height,
            Format format,
            int rowStride = 0,
            long timestamp = 0L,
            bool mirrored = false
        ) {
            rowStride = rowStride > 0 ? rowStride : GetDefaultStride(format, width);
            dataBuffer = new NativeArray<byte>(rowStride * height, Allocator.Persistent);
            VideoKit.CreatePixelBuffer(
                width,
                height,
                format,
                (byte*)dataBuffer.GetUnsafePtr(),
                rowStride,
                timestamp,
                mirrored,
                out pixelBuffer
            ).Throw();
        }

        public static implicit operator IntPtr (PixelBuffer pixelBuffer) => pixelBuffer.pixelBuffer;
        #endregion


        #region --Utilities--

        private static Format ToImageFormat (TextureFormat format) => format switch {
            TextureFormat.RGBA32    => Format.RGBA8888,
            TextureFormat.BGRA32    => Format.BGRA8888,
            _                       => throw new ArgumentException($"Cannot create pixel buffer from texture with format: {format}"),
        };

        private static int GetDefaultStride (Format format, int width) => format switch {
            Format.RGBA8888 => width * 4,
            Format.BGRA8888 => width * 4,
            _               => throw new ArgumentException($"Cannot infer default stride for format: {format}"),
        };

        private readonly struct NativePlanes : IReadOnlyList<Plane> {

            private readonly IntPtr pixelBuffer;

            public NativePlanes (IntPtr pixelBuffer) => this.pixelBuffer = pixelBuffer;

            int IReadOnlyCollection<Plane>.Count => pixelBuffer.GetPixelBufferPlaneCount(out var count) == Status.Ok ? count : default;

            Plane IReadOnlyList<Plane>.this [int index] => new(pixelBuffer, index);
    
            IEnumerator<Plane> IEnumerable<Plane>.GetEnumerator () {
                pixelBuffer.GetPixelBufferPlaneCount(out var count);
                for (var idx = 0; idx < count; ++idx)
                    yield return new(pixelBuffer, idx);
            }

            IEnumerator IEnumerable.GetEnumerator () => (this as IEnumerable<Plane>).GetEnumerator();
        }
        #endregion
    }
}