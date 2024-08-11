/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Sources {

    using System;
    using System.Runtime.CompilerServices;
    using Clocks;
    using Unity.Collections;

    /// <summary>
    /// Media source for generating pixel buffers from a camera device.
    /// </summary>
    internal sealed class CameraManagerSource : IDisposable {

        #region --Client API--
        public CameraManagerSource (
            MediaRecorder recorder,
            IClock? clock,
            VideoKitCameraManager cameraManager
        ) : this(recorder.width, recorder.height, recorder.Append, clock, cameraManager) { }

        public CameraManagerSource (
            int width,
            int height,
            Action<PixelBuffer> handler,
            IClock? clock,
            VideoKitCameraManager cameraManager
        ) {
            this.width = width;
            this.height = height;
            this.handler = handler;
            this.clock = clock;
            this.cameraManager = cameraManager;
            cameraManager.OnPixelBuffer += OnPixelBuffer;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Dispose () {
            cameraManager.OnPixelBuffer -= OnPixelBuffer;
            pixelData.Dispose();
            pixelData = default;
            disposed = true;
        }
        #endregion


        #region --Operations--
        private readonly int width;
        private readonly int height;
        private readonly Action<PixelBuffer> handler;
        private readonly IClock? clock;
        private readonly VideoKitCameraManager cameraManager;
        private NativeArray<byte> pixelData;
        private bool disposed;

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void OnPixelBuffer (PixelBuffer srcBuffer) { // INCOMPLETE // Check width and height
            // Check
            if (disposed)
                return;
            // Check data
            var dataSize = srcBuffer.width * srcBuffer.height * 4;
            if (pixelData.IsCreated && pixelData.Length != dataSize) {
                pixelData.Dispose();
                pixelData = default;
            }
            // Create data
            if (!pixelData.IsCreated)
                pixelData = new NativeArray<byte>(dataSize, Allocator.Persistent);
            // Copy
            var rotation = cameraManager.rotation;
            var (dstWidth, dstHeight) = IsTransposed(rotation) ?
                (srcBuffer.height, srcBuffer.width) :
                (srcBuffer.width, srcBuffer.height);
            using var dstBuffer = new PixelBuffer(
                dstWidth,
                dstHeight,
                PixelBuffer.Format.RGBA8888,
                pixelData,
                timestamp: clock?.timestamp ?? 0L
            );
            srcBuffer.CopyTo(dstBuffer, rotation: rotation);
            // Append
            handler?.Invoke(dstBuffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsTransposed (PixelBuffer.Rotation rotation) =>
            rotation == PixelBuffer.Rotation._90 ||
            rotation == PixelBuffer.Rotation._270;
        #endregion
    }
}