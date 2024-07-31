//
//  VKTAudioBuffer.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 3/05/2024.
//  Copyright © 2024 NatML Inc. All rights reserved.
//

#pragma once

#include <stdint.h>
#include <VideoKit/VKTSampleBuffer.h>

#pragma region --Types--
/*!
 @typedef VKTAudioBuffer
 
 @abstract Audio buffer.

 @discussion Audio buffer containing linear PCM data interleaved by channel.
*/
typedef VKTSampleBuffer VKTAudioBuffer;
#pragma endregion


#pragma region --Operations--
/*!
 @function VKTAudioBufferCreate

 @abstract Create an audio buffer.

 @discussion Create an audio buffer.

 @param sampleRate
 Audio sample rate.

 @param channelCount
 Audio channel count.

 @param data
 Linear PCM audio sample data. Samples must be interleaved by channel.

 @param sampleCount
 Total sample count.

 @param timestamp
 Audio timestamp.

 @param audioBuffer
 Audio buffer.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTAudioBufferCreate (
    int32_t sampleRate,
    int32_t channelCount,
    const float* data,
    int32_t sampleCount,
    int64_t timestamp,
    VKTAudioBuffer** audioBuffer
);

/*!
 @function VKTAudioBufferGetData

 @abstract Get the audio data of an audio buffer.

 @discussion Get the audio data of an audio buffer.

 @param audioBuffer
 Audio buffer.

 @param data
 Audio data
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTAudioBufferGetData (
    VKTAudioBuffer* audioBuffer,
    float** data
);

/*!
 @function VKTAudioBufferGetSampleCount

 @abstract Get the total sample count of an audio buffer.

 @discussion Get the total sample count of an audio buffer.

 @param audioBuffer
 Audio buffer.

 @param sampleCount
 Total sample count.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTAudioBufferGetSampleCount (
    VKTAudioBuffer* audioBuffer,
    int32_t* sampleCount
);

/*!
 @function VKTAudioBufferGetSampleRate

 @abstract Get the sample rate of an audio buffer.

 @discussion Get the sample rate of an audio buffer.

 @param audioBuffer
 Audio buffer.

 @param sampleRate
 Sample rate.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTAudioBufferGetSampleRate (
    VKTAudioBuffer* audioBuffer,
    int32_t* sampleRate
);

/*!
 @function VKTAudioBufferGetChannelCount

 @abstract Get the channel count of an audio buffer.

 @discussion Get the channel count of an audio buffer.

 @param audioBuffer
 Audio buffer.

 @param channelCount
 Channel count.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTAudioBufferGetChannelCount (
    VKTAudioBuffer* audioBuffer,
    int32_t* channelCount
);
#pragma endregion