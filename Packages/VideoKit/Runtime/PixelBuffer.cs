/* 
*   VideoKit
*   Copyright © 2024 NatML Inc. All Rights Reserved.
*/

#nullable enable

namespace VideoKit {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Function.Types;
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
                pixelBuffer.GetPixelBufferData(out var data);
                pixelBuffer.GetPixelBufferDataSize(out var size);
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
        public readonly Format format => pixelBuffer.GetPixelBufferFormat(out var format).Throw() == Status.Ok ? format : default;

        /// <summary>
        /// Pixel buffer width.
        /// </summary>
        public readonly int width => pixelBuffer.GetPixelBufferWidth(out var width).Throw() == Status.Ok ? width : default;

        /// <summary>
        /// Pixel buffer height.
        /// </summary>
        public readonly int height => pixelBuffer.GetPixelBufferHeight(out var height).Throw() == Status.Ok ? height : default;

        /// <summary>
        /// Pixel buffer row stride in bytes.
        /// This is zero if the pixel data is planar.
        /// </summary>
        public readonly int rowStride => pixelBuffer.GetPixelBufferRowStride(out var stride).Throw() == Status.Ok ? stride : default;

        /// <summary>
        /// Pixel buffer timestamp in nanoseconds.
        /// The timestamp is based on the system media clock.
        /// </summary>
        public readonly long timestamp => pixelBuffer.GetSampleBufferTimestamp(out var timestamp).Throw() == Status.Ok ? timestamp : default;

        /// <summary>
        /// Whether the pixel buffer is vertically mirrored.
        /// </summary>
        public readonly bool verticallyMirrored => pixelBuffer.GetPixelBufferIsVerticallyMirrored(out var mirrored).Throw() == Status.Ok ? mirrored : default;

        /// <summary>
        /// Pixel buffer planes for planar formats.
        /// This is `null` for interleaved formats.
        /// </summary>
        public readonly IReadOnlyList<Plane> planes => new NativePlanes(this);

        /// <summary>
        /// Pixel buffer metadata.
        /// This is `null` if the pixel buffer has no metadata.
        /// </summary>
        public readonly IReadOnlyDictionary<string, object>? metadata => new NativeMetadataDictionary(this);
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
            texture.width,
            texture.height,
            ToImageFormat(texture.format),
            texture.GetRawTextureData<byte>(),
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
        ) {
            // Copy
            pixelData = (byte*)UnsafeUtility.Malloc(data.Length, 16, Allocator.Persistent);
            fixed (byte* srcData = data)
                UnsafeUtility.MemCpy(pixelData, srcData, data.Length);
            // Create
            VideoKit.CreatePixelBuffer(
                width,
                height,
                format,
                pixelData,
                rowStride > 0 ? rowStride : GetDefaultStride(format, width),
                timestamp,
                mirrored,
                out pixelBuffer
            ).Throw();
        }

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
        ) : this(width, height, format, (byte*)data.GetUnsafePtr(), rowStride, timestamp, mirrored) { }

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
        ) : this(VideoKit.CreatePixelBuffer(
            width,
            height,
            format,
            data,
            rowStride > 0 ? rowStride : GetDefaultStride(format, width),
            timestamp,
            mirrored,
            out var pixelBuffer
        ).Throw() == Status.Ok ? pixelBuffer : default) { }

        /// <summary>
        /// Dispose the pixel buffer and release resources.
        /// </summary>
        public void Dispose () {
            pixelBuffer.ReleaseSampleBuffer();
            UnsafeUtility.Free(pixelData, Allocator.Persistent);
        }
        #endregion


        #region --Processing--
        /// <summary>
        /// Create a pixel buffer from a region of this pixel buffer.
        /// NOTE: This is only supported for pixel buffers with `RGBA8888` and `BGRA8888` formats.
        /// </summary>
        /// <param name="x">Left coordinate.</param>
        /// <param name="y">Bottom coordinate.</param>
        /// <param name="width">Region width.</param>
        /// <param name="height">Region height.</param>
        /// <returns>Region pixel buffer.</returns>
        public unsafe PixelBuffer Region (int x, int y, int width, int height) {
            // Check
            if (format != Format.RGBA8888 && format != Format.BGRA8888)
                throw new InvalidOperationException(@"PixelBuffer.Region currently only supports pixel buffers with `RGBA8888` pixel format");
            // Return
            return new PixelBuffer(
                width,
                height,
                format,
                (byte*)data.GetUnsafePtr() + (y * rowStride) + (x * 4),
                rowStride,
                timestamp: timestamp,
                mirrored: verticallyMirrored
            );
        }

        /// <summary>
        /// Copy pixel data into a destination pixel buffer.
        /// </summary>
        /// <param name="destination">Destination pixel buffer.</param>
        /// <param name="rotation">Rotation to apply when copying pixel data.</param>
        public void CopyTo (
            PixelBuffer destination,
            Rotation rotation = Rotation._0
        ) => pixelBuffer.CopyToPixelBuffer(destination, rotation).Throw();

        /// <summary>
        /// Create a Function image for making predictions.
        /// NOTE: The pixel buffer format MUST be `RGBA8888`.
        /// </summary>
        public unsafe Image ToImage () {
            // Check format
            if (format != Format.RGBA8888)
                throw new InvalidOperationException($"Cannot convert pixel buffer to image because pixel buffer format is not `RGBA8888`: {format}");
            // Create
            return new Image((byte*)data.GetUnsafeReadOnlyPtr(), width, height, 4);
        }
        #endregion


        #region --Operations--
        private readonly IntPtr pixelBuffer;
        private readonly byte* pixelData;

        internal PixelBuffer (IntPtr pixelBuffer) {
            this.pixelBuffer = pixelBuffer;
            this.pixelData = null;
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

        private readonly struct NativeMetadataDictionary : IReadOnlyDictionary<string, object> {

            private readonly IntPtr metadata;

            public NativeMetadataDictionary (IntPtr pixelBuffer) => pixelBuffer.GetPixelBufferMetadata(out metadata);

            object IReadOnlyDictionary<string, object>.this[string key] {
                get {
                    if ((this as IReadOnlyDictionary<string, object>).TryGetValue(key, out var value))
                        return value;
                    else
                        throw new KeyNotFoundException();
                }
            }

            int IReadOnlyCollection<KeyValuePair<string, object>>.Count => metadata.GetMetadataCount(out var count) ==  Status.Ok ? count : default;

            bool IReadOnlyDictionary<string, object>.ContainsKey (string key) => metadata.MetadataContainsKey(key) == Status.Ok;

            unsafe IEnumerable<string> IReadOnlyDictionary<string, object>.Keys {
                get {
                    if (metadata.GetMetadataCount(out var count) != Status.Ok)
                        return Enumerable.Empty<string>();
                    var keyPtrs = new IntPtr[count];
                    metadata.GetMetadataKeys(keyPtrs);
                    return keyPtrs.Select(ptr => Marshal.PtrToStringUTF8(ptr));
                }
            }

            IEnumerable<object> IReadOnlyDictionary<string, object>.Values {
                get {
                    var dict = this as IReadOnlyDictionary<string, object>;
                    foreach (var key in dict.Keys)
                        if (dict.TryGetValue(key, out var value))
                            yield return value;
                }
            }

            bool IReadOnlyDictionary<string, object>.TryGetValue(string key, out object value) {
                value = null!;
                var count = 0;
                var status = metadata.GetMetadataFloatValue(key, null, ref count);
                if (status == Status.Ok) {
                    var result = new float[count];
                    metadata.GetMetadataFloatValue(key, result, ref count).Throw();
                    value = result;
                    return true;
                }
                return false;
            }

            IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator () {
                var dict = this as IReadOnlyDictionary<string, object>;
                return dict.Keys.Zip(dict.Values, KeyValuePair.Create).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator () => (this as IEnumerable<KeyValuePair<string, object>>).GetEnumerator();
        }
        #endregion
    }
}