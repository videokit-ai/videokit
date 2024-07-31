//
//  VKTMediaRecorder.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 5/15/2023.
//  Copyright © 2024 NatML Inc. All rights reserved.
//

#pragma once

#include <stdint.h>
#include <VideoKit/VKTStatus.h>
#include <VideoKit/VKTMediaAsset.h>
#include <VideoKit/VKTAudioBuffer.h>
#include <VideoKit/VKTPixelBuffer.h>

#pragma region --Enumerations--
/*!
 @enum VKTMediaFormat

 @abstract VideoKit media format.

 @constant VKT_MEDIA_FORMAT_MP4
 MP4 video with H.264 AVC video codec and AAC audio codec.
 This format supports recording both video and audio frames.

 @constant VKT_MEDIA_FORMAT_HEVC
 MP4 video with H.265 HEVC video codec and AAC audio codec.
 This format has better compression than `VKT_MEDIA_FORMAT_MP4`.
 This format supports recording both video and audio frames.

 @constant VKT_MEDIA_FORMAT_WEBM
 WEBM video with VP8 or VP9 video codec.
 This format support recording both video and audio frames.

 @constant VKT_MEDIA_FORMAT_GIF
 Animated GIF image.
 This format only supports recording video frames.

 @constant VKT_MEDIA_FORMAT_JPEG
 JPEG image sequence.
 This format only supports recording video frames.
 This format is not supported on WebGL.

 @constant VKT_MEDIA_FORMAT_WAV
 Waveform audio.
 This format only supports recording audio.

 @constant VKT_MEDIA_FORMAT_AV1
 MP4 video with AV1 video codec and AAC audio codec.
 This format supports recording both video and audio frames.
 This format is currently experimental and is disabled in release builds of VideoKit.

 @constant VKT_MEDIA_FORMAT_PRO_RES_4444
 Apple ProRes video.
 This format supports recording both video and audio frames.
 This format is currently experimental and is disabled in release builds of VideoKit.
*/
enum VKTMediaFormat {
    VKT_MEDIA_FORMAT_MP4            = 0,
    VKT_MEDIA_FORMAT_HEVC           = 1,
    VKT_MEDIA_FORMAT_WEBM           = 2,
    VKT_MEDIA_FORMAT_GIF            = 3,
    VKT_MEDIA_FORMAT_JPEG           = 4,
    VKT_MEDIA_FORMAT_WAV            = 5,
    VKT_MEDIA_FORMAT_AV1            = 6,
    VKT_MEDIA_FORMAT_PRO_RES_4444   = 7,
};
typedef enum VKTMediaFormat VKTMediaFormat;
#pragma endregion


#pragma region --Types--
/*!
 @struct VKTMediaRecorder
 
 @abstract Media recorder.

 @discussion Media recorder.
*/
struct VKTMediaRecorder;
typedef struct VKTMediaRecorder VKTMediaRecorder;
#pragma endregion


#pragma region --Operations--
/*!
 @function VKTMediaRecorderIsFormatSupported
 
 @abstract Check whether a recording format is supported on the current device.
 
 @discussion Check whether a recording format is supported on the current device.

 @param format
 Recording format.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaRecorderIsFormatSupported (VKTMediaFormat format);

/*!
 @function VKTMediaRecorderGetFormat
 
 @abstract Get the recorder format.
 
 @discussion Get the recorder format.
 
 @param recorder
 Media recorder.

 @param format
 Output format. MUST NOT be `NULL`.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaRecorderGetFormat (
    VKTMediaRecorder* recorder,
    VKTMediaFormat* format
);

/*!
 @function VKTMediaRecorderGetWidth
 
 @abstract Get the recorder width.
 
 @discussion Get the recorder width.
 
 @param recorder
 Media recorder.
 
 @param width
 Output video width. MUST NOT be `NULL`.

 @returns `VKT_OK` if successful.
 `VKT_ERROR_INVALID_ARGUMENT` if the `recorder` or `width` arguments are `NULL`.
 `VKT_ERROR_INVALID_OPERATION` if the recorder does not support recording video frames.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaRecorderGetWidth (
    VKTMediaRecorder* recorder,
    int32_t* width
);

/*!
 @function VKTMediaRecorderGetHeight
 
 @abstract Get the recorder height.
 
 @discussion Get the recorder height.
 
 @param recorder
 Media recorder.
 
 @param height
 Output video height. MUST NOT be `NULL`.

 @returns `VKT_OK` if successful.
 `VKT_ERROR_INVALID_ARGUMENT` if the `recorder` or `height` arguments are `NULL`.
 `VKT_ERROR_INVALID_OPERATION` if the recorder does not support recording video frames.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaRecorderGetHeight (
    VKTMediaRecorder* recorder,
    int32_t* height
);

/*!
 @function VKTMediaRecorderGetSampleRate
 
 @abstract Get the recorder sample rate.
 
 @discussion Get the recorder sample rate.
 
 @param recorder
 Media recorder.
 
 @param sampleRate
 Output audio sample rate. MUST NOT be `NULL`.

 @returns `VKT_OK` if successful.
 `VKT_ERROR_INVALID_ARGUMENT` if the `recorder` or `sampleRate` arguments are `NULL`.
 `VKT_ERROR_INVALID_OPERATION` if the recorder does not support recording audio frames.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaRecorderGetSampleRate (
    VKTMediaRecorder* recorder,
    int32_t* sampleRate
);

/*!
 @function VKTMediaRecorderGetChannelCount
 
 @abstract Get the recorder channel count.
 
 @discussion Get the recorder channel count.
 
 @param recorder
 Media recorder.
 
 @param channelCount
 Output audio channel count. MUST NOT be `NULL`.

 @returns `VKT_OK` if successful.
 `VKT_ERROR_INVALID_ARGUMENT` if the `recorder` or `channelCount` arguments are `NULL`.
 `VKT_ERROR_INVALID_OPERATION` if the recorder does not support recording audio frames.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaRecorderGetChannelCount (
    VKTMediaRecorder* recorder,
    int32_t* channelCount
);

/*!
 @function VKTMediaRecorderAppendPixelBuffer

 @abstract Append a video frame to the recording from a pixel buffer.

 @discussion Append a video frame to the recording from a pixel buffer.
 
 @param recorder
 Media recorder.

 @param pixelBuffer
 Pixel buffer.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaRecorderAppendPixelBuffer (
    VKTMediaRecorder* recorder,
    VKTPixelBuffer* pixelBuffer
);

/*!
 @function VKTMediaRecorderAppendAudioBuffer

 @abstract Append an audio frame to the recording from a sample buffer.

 @discussion Append an audio frame to the recording from a sample buffer.

 @param recorder
 Media recorder.

 @param audioBuffer
 Audio buffer.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaRecorderAppendAudioBuffer (
    VKTMediaRecorder* recorder,
    VKTAudioBuffer* audioBuffer
);

/*!
 @function VKTMediaRecorderFinishWriting

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
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaRecorderFinishWriting (
    VKTMediaRecorder* recorder,
    VKTMediaAssetHandler handler,
    void* context
);
#pragma endregion


#pragma region --Constructors--
/*!
 @function VKTMediaRecorderCreateMP4

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
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaRecorderCreateMP4 (
    const char* path,
    int32_t width,
    int32_t height,
    float frameRate,
    int32_t sampleRate,
    int32_t channelCount,
    int32_t videoBitRate,
    int32_t keyframeInterval,
    int32_t audioBitRate,
    VKTMediaRecorder** recorder
);

/*!
 @function VKTMediaRecorderCreateHEVC
 
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
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaRecorderCreateHEVC (
    const char* path,
    int32_t width,
    int32_t height,
    float frameRate,
    int32_t sampleRate,
    int32_t channelCount,
    int32_t videoBitRate,
    int32_t keyframeInterval,
    int32_t audioBitRate,
    VKTMediaRecorder** recorder
);

/*!
 @function VKTMediaRecorderCreateWEBM

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
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaRecorderCreateWEBM (
    const char* path,
    int32_t width,
    int32_t height,
    float frameRate,
    int32_t sampleRate,
    int32_t channelCount,
    int32_t videoBitRate,
    int32_t keyframeInterval,
    int32_t audioBitRate,
    VKTMediaRecorder** recorder
);

/*!
 @function VKTMediaRecorderCreateGIF

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
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaRecorderCreateGIF (
    const char* path,
    int32_t width,
    int32_t height,
    float delay,
    VKTMediaRecorder** recorder
);

/*!
 @function VKTMediaRecorderCreateWAV

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
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaRecorderCreateWAV (
    const char* path,
    int32_t sampleRate,
    int32_t channelCount,
    VKTMediaRecorder** recorder
);

/*!
 @function VKTMediaRecorderCreateJPEG

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
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaRecorderCreateJPEG (
    const char* path,
    int32_t width,
    int32_t height,
    float compressionQuality,
    VKTMediaRecorder** recorder
);

/*!
 @function VKTMediaRecorderCreateAV1
 
 @abstract Create an AV1 recorder.
 
 @discussion Create an AV1 recorder that records with the AV1 codec.
 
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
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaRecorderCreateAV1 (
    const char* path,
    int32_t width,
    int32_t height,
    float frameRate,
    int32_t sampleRate,
    int32_t channelCount,
    int32_t videoBitRate,
    int32_t keyframeInterval,
    int32_t audioBitRate,
    VKTMediaRecorder** recorder
);

/*!
 @function VKTMediaRecorderCreateProRes4444
 
 @abstract Create an Apple ProRes4444 recorder.
 
 @discussion Create an MOV recorder that records with the Apple ProRes4444 codec.
 
 @param path
 Recording path. This path must be writable on the local file system.

 @param width
 Video width.
 
 @param height
 Video height.

 @param sampleRate
 Audio sample rate. Pass 0 if recording without audio.
 
 @param channelCount
 Audio channel count. Pass 0 if recording without audio.

 @param audioBitRate
 Audio bit rate in bits per second. Ignored if no audio format is provided.
 
 @param recorder
 Output media recorder.

 @returns Recorder creation status.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaRecorderCreateProRes4444 (
    const char* path,
    int32_t width,
    int32_t height,
    int32_t sampleRate,
    int32_t channelCount,
    int32_t audioBitRate,
    VKTMediaRecorder** recorder
);
#pragma endregion