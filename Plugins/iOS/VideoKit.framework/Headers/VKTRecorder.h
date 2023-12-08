//
//  VKTRecorder.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 5/15/2023.
//  Copyright © 2023 NatML Inc. All rights reserved.
//

#pragma once

#include <stdint.h>
#include <VideoKit/VKTSession.h>

#pragma region --Types--
/*!
 @struct VKTRecorder
 
 @abstract Media recorder.

 @discussion Media recorder.
*/
struct VKTRecorder;
typedef struct VKTRecorder VKTRecorder;

/*!
 @abstract Callback invoked with path to recorded media file.
 
 @param context
 User context provided to recorder.
 
 @param path
 Path to record file. If recording fails for any reason, this path will be `NULL`.
*/
typedef void (*VKTRecordingHandler) (void* context, const char* path);
#pragma endregion


#pragma region --IMediaRecorder--
/*!
 @function VKTRecorderGetFrameSize
 
 @abstract Get the recorder's frame size.
 
 @discussion Get the recorder's frame size.
 
 @param recorder
 Media recorder.
 
 @param outWidth
 Output frame width.
 
 @param outHeight
 Output frame height.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTRecorderGetFrameSize (
    VKTRecorder* recorder,
    int32_t* outWidth,
    int32_t* outHeight
);

/*!
 @function VKTRecorderCommitFrame

 @abstract Commit a video frame to the recording.

 @discussion Commit a video frame to the recording.
 
 @param recorder
 Media recorder.

 @param pixelBuffer
 Pixel buffer containing a single video frame.
 The pixel buffer MUST be laid out in RGBA8888 order (32 bits per pixel).

 @param timestamp
 Frame timestamp. The spacing between consecutive timestamps determines the
 effective framerate of some recorders.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTRecorderCommitFrame (
    VKTRecorder* recorder,
    const uint8_t* pixelBuffer,
    int64_t timestamp
);

/*!
 @function VKTRecorderCommitSamples

 @abstract Commit an audio frame to the recording.

 @discussion Commit an audio frame to the recording.

 @param recorder
 Media recorder.

 @param sampleBuffer
 Sample buffer containing a single audio frame. The sample buffer MUST be in 32-bit PCM
 format, interleaved by channel for channel counts greater than 1.

 @param sampleCount
 Total number of samples in the sample buffer. This should account for multiple channels.

 @param timestamp
 Frame timestamp. The spacing between consecutive timestamps determines the
 effective framerate of some recorders.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTRecorderCommitSamples (
    VKTRecorder* recorder,
    const float* sampleBuffer,
    int32_t sampleCount,
    int64_t timestamp
);

/*!
 @function VKTRecorderFinishWriting

 @abstract Finish writing and invoke the completion handler.

 @discussion Finish writing and invoke the completion handler. The recorder is automatically
 released, along with any resources it owns. The recorder MUST NOT be used once this function
 has been invoked.
 
 The completion handler will be invoked soon after this function is called. If recording fails for any reason,
 the completion handler will receive `NULL` for the recording path.

 @param recorder
 Media recorder.

 @param handler
 Recording completion handler invoked once recording is completed.

 @param context
 Context passed to completion handler. Can be `NULL`.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTRecorderFinishWriting (
    VKTRecorder* recorder,
    VKTRecordingHandler handler,
    void* context
);
#pragma endregion


#pragma region --Constructors--
/*!
 @function VKTRecorderCreateMP4

 @abstract Create an MP4 recorder.

 @discussion Create an MP4 recorder that records with the H.264 AVC codec.

 @param path
 Recording path. This path must be writable on the local file system.

 @param width
 Video width.

 @param height
 Video height.

 @param frameRate
 Video frame rate.

 @param sampleRate
 Audio sample rate. Pass 0 if recording without audio.

 @param channelCount
 Audio channel count. Pass 0 if recording without audio.

 @param videoBitRate
 Video bit rate in bits per second.

 @param keyframeInterval
 Video keyframe interval in seconds.

 @param audioBitRate
 Audio bit rate in bits per second. Ignored if no audio format is provided.

 @param recorder
 Output media recorder.

 @returns Recorder creation status.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTRecorderCreateMP4 (
    const char* path,
    int32_t width,
    int32_t height,
    float frameRate,
    int32_t sampleRate,
    int32_t channelCount,
    int32_t videoBitRate,
    int32_t keyframeInterval,
    int32_t audioBitRate,
    VKTRecorder** recorder
);

/*!
 @function VKTRecorderCreateHEVC
 
 @abstract Create an HEVC recorder.
 
 @discussion Create an MP4 recorder that records with the H.265 HEVC codec.
 
 @param path
 Recording path. This path must be writable on the local file system.

 @param width
 Video width.
 
 @param height
 Video height.
 
 @param frameRate
 Video frame rate.
 
 @param sampleRate
 Audio sample rate. Pass 0 if recording without audio.
 
 @param channelCount
 Audio channel count. Pass 0 if recording without audio.
 
 @param videoBitRate
 Video bit rate in bits per second.

 @param keyframeInterval
 Video keyframe interval in seconds.

 @param audioBitRate
 Audio bit rate in bits per second. Ignored if no audio format is provided.
 
 @param recorder
 Output media recorder.

 @returns Recorder creation status.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTRecorderCreateHEVC (
    const char* path,
    int32_t width,
    int32_t height,
    float frameRate,
    int32_t sampleRate,
    int32_t channelCount,
    int32_t videoBitRate,
    int32_t keyframeInterval,
    int32_t audioBitRate,
    VKTRecorder** recorder
);

/*!
 @function VKTRecorderCreateWEBM

 @abstract Create a WEBM video recorder.

 @discussion Create a WEBM video recorder.

 @param path
 Recording path. This path must be writable on the local file system.

 @param width
 Video width.

 @param height
 Vide. height.

 @param frameRate
 Video frame rate.

 @param sampleRate
 Audio sample rate. Pass 0 if recording without audio.

 @param channelCount
 Audio channel count. Pass 0 if recording without audio.

 @param videoBitRate
 Video bit rate in bits per second.

 @param keyframeInterval
 Video keyframe interval in seconds.

 @param audioBitRate
 Audio bit rate in bits per second. Ignored if no audio format is provided.

 @param recorder
 Output media recorder.

 @returns Recorder creation status.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTRecorderCreateWEBM (
    const char* path,
    int32_t width,
    int32_t height,
    float frameRate,
    int32_t sampleRate,
    int32_t channelCount,
    int32_t videoBitRate,
    int32_t keyframeInterval,
    int32_t audioBitRate,
    VKTRecorder** recorder
);

/*!
 @function VKTRecorderCreateGIF

 @abstract Create a GIF recorder.

 @discussion Create an animated GIF recorder.
 The generated GIF image will loop forever.

 @param path
 Recording path. This path must be writable on the local file system.

 @param width
 Image width.

 @param height
 Image height.

 @param delay
 Per-frame delay in seconds.

 @param recorder
 Output media recorder.

 @returns Recorder creation status.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTRecorderCreateGIF (
    const char* path,
    int32_t width,
    int32_t height,
    float delay,
    VKTRecorder** recorder
);

/*!
 @function VKTRecorderCreateWAV

 @abstract Create a WAV audio recorder.

 @discussion Create a WAV audio recorder.

 @param path
 Recording path. This path must be writable on the local file system.

 @param sampleRate
 Audio sample rate.

 @param channelCount
 Audio channel count.

 @param recorder
 Output media recorder.

 @returns Recorder creation status.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTRecorderCreateWAV (
    const char* path,
    int32_t sampleRate,
    int32_t channelCount,
    VKTRecorder** recorder
);

/*!
 @function VKTRecorderCreateJPEG

 @abstract Create a JPEG image sequence recorder.

 @discussion Create a JPEG image sequence recorder.
 The recorder returns a path separator-delimited list of image frame paths.

 @param width
 Image width.

 @param height
 Image height.

 @param compressionQuality
 Image compression quality in range [0, 1].

 @param recorder
 Output media recorder.

 @returns Recorder creation status.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTRecorderCreateJPEG (
    const char* path,
    int32_t width,
    int32_t height,
    float compressionQuality,
    VKTRecorder** recorder
);
#pragma endregion