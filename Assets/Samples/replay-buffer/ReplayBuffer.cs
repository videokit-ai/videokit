using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using VideoKit;
using VideoKit.UI;

public class ReplayBuffer : MonoBehaviour {

    [Header(@"Configuration")]
    [Range(0.5f, 5f)] public float capacityInSeconds = 1f;

    [Header(@"Camera Preview")]
    public VideoKitCameraManager cameraManager;
    public VideoKitCameraView cameraView; // used to rotate pixel buffers when recording

    private readonly Queue<Buffer> queue = new();
    private readonly object fence = new(); // used for synchronization

    private void Start() {
        // Listen for new pixel buffers
        cameraManager.OnPixelBuffer += OnPixelBuffer;
    }

    private void OnPixelBuffer(CameraDevice cameraDevice, PixelBuffer pixelBuffer) {
        // Dequeue older pixel buffers
        var maxQueueSize = Mathf.FloorToInt(capacityInSeconds * cameraDevice.frameRate);
        lock (queue)
            while (queue.Count >= maxQueueSize)
                queue.Dequeue().Dispose();
        // Copy to an RGBA32 buffer
        // Ideally we want to copy to a YUV buffer
        // But the `PixelBuffer.CopyTo` method doesn't support copying to YUV
        var bufferSize = pixelBuffer.width * pixelBuffer.height * 4;
        var data = new NativeArray<byte>(bufferSize, Allocator.Persistent);
        var dstBuffer = new PixelBuffer(
            width: pixelBuffer.width,
            height: pixelBuffer.height,
            format: PixelBuffer.Format.RGBA8888,
            data: data,
            timestamp: pixelBuffer.timestamp,
            mirrored: pixelBuffer.verticallyMirrored
        );
        pixelBuffer.CopyTo(dstBuffer);
        // Enqueue
        lock (queue)
            queue.Enqueue(new() { data = data, buffer = dstBuffer });
    }

    /// <summary>
    /// Flush the queue to an MP4 video file.
    /// </summary>
    public async void CaptureReplay() {
        // Get buffers in queue and clear queue
        Debug.Log($"Flushing recording queue");
        Queue<Buffer> queue;
        lock (fence) {
            queue = new(this.queue);
            this.queue.Clear();
        }
        // Check that queue is not empty
        if (queue.Count == 0)
            return;
        // Compute upright recording resolution
        var rotation = cameraView.rotation;
        var portrait =
            rotation == PixelBuffer.Rotation._90 ||
            rotation == PixelBuffer.Rotation._270;
        var firstBuffer = queue.Peek();
        var width = portrait ? firstBuffer.buffer.height : firstBuffer.buffer.width;
        var height = portrait ? firstBuffer.buffer.width : firstBuffer.buffer.height;
        var timebase = firstBuffer.buffer.timestamp;
        // Create recorder
        await Awaitable.BackgroundThreadAsync();
        var recorder = await MediaRecorder.Create(
            format: MediaRecorder.Format.MP4,
            width: width,
            height: height,
            frameRate: 60f, // this doesn't actually matter for MP4
            keyframeInterval: 1
        );
        // Append frames
        using var rgbaData = new NativeArray<byte>(
            length: width * height * 4,
            allocator: Allocator.Persistent
        );
        while (queue.Count > 0) {
            using var buffer = queue.Dequeue();
            var timestamp = buffer.buffer.timestamp - timebase; // relative timestamp
            using var rgbaBuffer = new PixelBuffer(
                width: width,
                height: height,
                format: PixelBuffer.Format.RGBA8888, // recorders currently require RGBA8888 format
                data: rgbaData,
                timestamp: timestamp,
                mirrored: false
            );
            buffer.buffer.CopyTo(rgbaBuffer, rotation: rotation);
            recorder.Append(rgbaBuffer);
        }
        await Awaitable.MainThreadAsync();
        // Finish writing
        var asset = await recorder.FinishWriting();
        Debug.Log($"Recorded replay to path: {asset.path}");
    #if UNITY_ANDROID || UNITY_IOS || UNITY_VISIONOS
        Handheld.PlayFullScreenMovie($"file://{asset.path}");
    #endif
    }

    /// <summary>
    /// The `PixelBuffer` does not own its backing `data` memory.
    /// So we have to keep track of it so we can dispose of it.
    /// </summary>
    private struct Buffer : IDisposable {

        public NativeArray<byte> data;
        public PixelBuffer buffer;

        public void Dispose () {
            buffer.Dispose();
            data.Dispose();
        }
    }
}