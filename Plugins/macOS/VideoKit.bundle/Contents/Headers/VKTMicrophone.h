//
//  VKTMicrophone.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 5/15/2023.
//  Copyright © 2023 NatML Inc. All rights reserved.
//

#pragma once

#include <VideoKit/VKTDevice.h>

#pragma region --Types--
/*!
 @typedef VKTMicrophone
 
 @abstract Audio input device.

 @discussion Audio input device.
*/
typedef VKTDevice VKTMicrophone;

/*!
 @typedef VKTAudioBuffer
 
 @abstract Audio buffer.

 @discussion Audio buffer
*/
typedef VKTSampleBuffer VKTAudioBuffer;
#pragma endregion


#pragma region --AudioDevice--
/*!
 @function VKTDiscoverMicrophones

 @abstract Discover available audio input devices.

 @discussion Discover available audio input devices.

 @param handler
 Device handler. MUST NOT be `NULL`.

 @param context
 Handler context.

 @returns Status.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTDiscoverMicrophones (
    VKTDeviceDiscoveryHandler handler,
    void* context
);

/*!
 @function VKTMicrophoneGetEchoCancellation
 
 @abstract Get the device echo cancellation mode.
 
 @discussion Get the device echo cancellation mode.
 
 @param microphone
 Microphone.
 
 @returns True if the device performs adaptive echo cancellation.
 */
VKT_BRIDGE VKT_EXPORT bool VKT_API VKTMicrophoneGetEchoCancellation (VKTMicrophone* microphone);

/*!
 @function VKTMicrophoneSetEchoCancellation
 
 @abstract Enable or disable echo cancellation on the device.
 
 @discussion If the device does not support echo cancellation, this will be a nop.
 
 @param microphone
 Microphone.
 
 @param echoCancellation
 Echo cancellation.
 */
VKT_BRIDGE VKT_EXPORT void VKT_API VKTMicrophoneSetEchoCancellation (
    VKTMicrophone* microphone,
    bool echoCancellation
);

/*!
 @function VKTMicrophoneGetSampleRate
 
 @abstract Audio device sample rate.
 
 @discussion Audio device sample rate.
 
 @param microphone
 Microphone.
 
 @returns Current sample rate.
 */
VKT_BRIDGE VKT_EXPORT int32_t VKT_API VKTMicrophoneGetSampleRate (VKTMicrophone* microphone);

/*!
 @function VKTMicrophoneSetSampleRate
 
 @abstract Set the audio device sample rate.
 
 @discussion Set the audio device sample rate.
 
 @param microphone
 Microphone.
 
 @param sampleRate
 Sample rate to set.
 */
VKT_BRIDGE VKT_EXPORT void VKT_API VKTMicrophoneSetSampleRate (
    VKTMicrophone* microphone,
    int32_t sampleRate
);

/*!
 @function VKTMicrophoneGetChannelCount
 
 @abstract Audio device channel count.
 
 @discussion Audio device channel count.
 
 @param microphone
 Microphone.
 
 @returns Current channel count.
 */
VKT_BRIDGE VKT_EXPORT int32_t VKT_API VKTMicrophoneGetChannelCount (VKTMicrophone* microphone);

/*!
 @function VKTMicrophoneSetChannelCount
 
 @abstract Set the audio device channel count.
 
 @discussion Set the audio device channel count.
 
 @param microphone
 Microphone.
 
 @param channelCount
 Channel count to set.
 */
VKT_BRIDGE VKT_EXPORT void VKT_API VKTMicrophoneSetChannelCount (
    VKTMicrophone* microphone,
    int32_t channelCount
);
#pragma endregion


#pragma region --AudioBuffer--
/*!
 @function VKTAudioBufferGetData

 @abstract Get the audio data of an audio buffer.

 @discussion Get the audio data of an audio buffer.

 @param audioBuffer
 Audio buffer.
*/
VKT_BRIDGE VKT_EXPORT float* VKT_API VKTAudioBufferGetData (VKTAudioBuffer* audioBuffer);

/*!
 @function VKTAudioBufferGetSampleCount

 @abstract Get the total sample count of an audio buffer.

 @discussion Get the total sample count of an audio buffer.

 @param audioBuffer
 Audio buffer.
*/
VKT_BRIDGE VKT_EXPORT int32_t VKT_API VKTAudioBufferGetSampleCount (VKTAudioBuffer* audioBuffer);

/*!
 @function VKTAudioBufferGetSampleRate

 @abstract Get the sample rate of an audio buffer.

 @discussion Get the sample rate of an audio buffer.

 @param audioBuffer
 Audio buffer.
*/
VKT_BRIDGE VKT_EXPORT int32_t VKT_API VKTAudioBufferGetSampleRate (VKTAudioBuffer* audioBuffer);

/*!
 @function VKTAudioBufferGetChannelCount

 @abstract Get the channel count of an audio buffer.

 @discussion Get the channel count of an audio buffer.

 @param audioBuffer
 Audio buffer.
*/
VKT_BRIDGE VKT_EXPORT int32_t VKT_API VKTAudioBufferGetChannelCount (VKTAudioBuffer* audioBuffer);

/*!
 @function VKTAudioBufferGetTimestamp

 @abstract Get the timestamp of an audio buffer.

 @discussion Get the timestamp of an audio buffer.

 @param audioBuffer
 Audio buffer.
*/
VKT_BRIDGE VKT_EXPORT int64_t VKT_API VKTAudioBufferGetTimestamp (VKTAudioBuffer* audioBuffer);
#pragma endregion
