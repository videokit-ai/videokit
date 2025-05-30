//
//  VKTAudioDevice.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 5/15/2023.
//  Copyright © 2025 Yusuf Olokoba. All rights reserved.
//

#pragma once

#include <VideoKit/VKTMediaDevice.h>

#pragma region --Types--
/*!
 @typedef VKTAudioDevice
 
 @abstract Audio input device.

 @discussion Audio input device.
*/
typedef VKTMediaDevice VKTAudioDevice;
#pragma endregion


#pragma region --Operations--
/*!
 @function VKTAudioDeviceDiscoverDevices

 @abstract Discover available audio devices.

 @discussion Discover available audio devices.

 @param handler
 Device handler.

 @param context
 Handler context. Can be `NULL`.
*/
VKT_API VKTStatus VKTAudioDeviceDiscoverDevices (
    VKTMediaDeviceDiscoveryHandler handler,
    void* context
);

/*!
 @function VKTAudioDeviceGetEchoCancellation
 
 @abstract Get the device echo cancellation mode.
 
 @discussion Get the device echo cancellation mode.
 
 @param audioDevice
 Microphone.
 
 @param echoCancellation
 Echo cancellation.
 */
VKT_API VKTStatus VKTAudioDeviceGetEchoCancellation (
    VKTAudioDevice* audioDevice,
    bool* echoCancellation
);

/*!
 @function VKTAudioDeviceSetEchoCancellation
 
 @abstract Enable or disable echo cancellation on the device.
 
 @discussion If the device does not support echo cancellation, this will be a nop.
 
 @param audioDevice
 Microphone.
 
 @param echoCancellation
 Echo cancellation.
 */
VKT_API VKTStatus VKTAudioDeviceSetEchoCancellation (
    VKTAudioDevice* audioDevice,
    bool echoCancellation
);

/*!
 @function VKTAudioDeviceGetSampleRate
 
 @abstract Get the audio device sample rate.
 
 @discussion Get the audio device sample rate.
 
 @param audioDevice
 Microphone.
 
 @param sampleRate
 Sample rate.
 */
VKT_API VKTStatus VKTAudioDeviceGetSampleRate (
    VKTAudioDevice* audioDevice,
    int32_t* sampleRate
);

/*!
 @function VKTAudioDeviceSetSampleRate
 
 @abstract Set the audio device sample rate.
 
 @discussion Set the audio device sample rate.
 
 @param audioDevice
 Microphone.
 
 @param sampleRate
 Sample rate.
 */
VKT_API VKTStatus VKTAudioDeviceSetSampleRate (
    VKTAudioDevice* audioDevice,
    int32_t sampleRate
);

/*!
 @function VKTAudioDeviceGetChannelCount
 
 @abstract Get the audio device channel count.
 
 @discussion Get the audio device channel count.
 
 @param audioDevice
 Microphone.
 
 @param channelCount
 Channel count.
 */
VKT_API VKTStatus VKTAudioDeviceGetChannelCount (
    VKTAudioDevice* audioDevice,
    int32_t* channelCount
);

/*!
 @function VKTAudioDeviceSetChannelCount
 
 @abstract Set the audio device channel count.
 
 @discussion Set the audio device channel count.
 
 @param audioDevice
 Microphone.
 
 @param channelCount
 Channel count.
 */
VKT_API VKTStatus VKTAudioDeviceSetChannelCount (
    VKTAudioDevice* audioDevice,
    int32_t channelCount
);
#pragma endregion
