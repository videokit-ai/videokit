//
//  VKTAsset.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 7/08/2023.
//  Copyright © 2023 NatML Inc. All rights reserved.
//

#pragma once

#include <stdint.h>
#include <VideoKit/VKTSession.h>

#pragma once

#pragma region --Enumerations--
/*!
 @enum VKTAssetType

 @abstract Media asset type.

 @constant VKT_ASSET_TYPE_UNKNOWN
 Unknown or unsupported asset type.

 @constant VKT_ASSET_TYPE_VIDEO
 Media asset is a video file.

 @constant VKT_ASSET_TYPE_AUDIO
 Media asset is an audio file.
*/
enum VKTAssetType {
    VKT_ASSET_TYPE_UNKNOWN  = 0,
    VKT_ASSET_TYPE_IMAGE    = 1,
    VKT_ASSET_TYPE_AUDIO    = 2,
    VKT_ASSET_TYPE_VIDEO    = 3,
};
typedef enum VKTAssetType VKTAssetType;
#pragma endregion


#pragma region --Types--
/*!
 @abstract Callback invoked with the result of loading an asset.
 
 @param context
 User context provided to the load function.

 @param path
 Asset path.
 
 @param type
 Asset type.

 @param width
 Video width.

 @param height
 Video height.

 @param frameRate
 Video frame rate.

 @param sampleRate
 Audio sample rate.

 @param channelCount
 Audio channel count.

 @param duration
 Asset duration in seconds.
*/
typedef void (*VKTAssetLoadHandler) (
    void* context,
    const char* path,
    VKTAssetType type,
    int32_t width,
    int32_t height,
    float frameRate,
    int32_t sampleRate,
    int32_t channelCount,
    float duration
);

/*!
 @abstract Callback invoked with the result of the sharing action.
 
 @param context
 User context provided to the share function.
 
 @param receiver
 Identifier of receiving application chosen by the user.
*/
typedef void (*VKTAssetShareHandler) (void* context, const char* receiver);
#pragma endregion


#pragma region --Asset--
/*!
 @function VKTAssetLoad
 
 @abstract Load a media asset.
 
 @discussion Load a media asset and return metadata.
 
 @param path
 Path to media asset.

 @param handler
 Completion handler to be invoked with the result of the load action.
 
 @param context
 User context passed to the load handler. Can be `NULL`.

 @returns Load status.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTAssetLoad (
    const char* path,
    VKTAssetLoadHandler handler,
    void* context
);

/*!
 @function VKTAssetLoadFromCameraRoll
 
 @abstract Load a media asset from the camera roll.
 
 @discussion Load a media asset and return metadata.
 
 @param type
 Media asset type.

 @param handler
 Completion handler to be invoked with the result of the load action.
 
 @param context
 User context passed to the load handler. Can be `NULL`.

 @returns Load status.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTAssetLoadFromCameraRoll (
    VKTAssetType type,
    VKTAssetLoadHandler handler,
    void* context
);

/*!
 @function VKTAssetShare
 
 @abstract Share a media asset.
 
 @discussion Share a media asset.
 
 @param path
 Path to media asset.

 @param message
 Message to share along with the media asset. Can be `NULL`.

 @param handler
 Completion handler to be invoked with the result of the sharing action. Can be `NULL`.
 
 @param context
 User context passed to the completion handler. Can be `NULL`.

 @returns Share status.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTAssetShare (
    const char* path,
    const char* message,
    VKTAssetShareHandler handler,
    void* context
);

/*!
 @function VKTAssetSaveToCameraRoll
 
 @abstract Save a media asset to the camera roll.
 
 @discussion Save a media asset to the camera roll.
 
 @param path
 Path to media asset.

 @param album
 Name of album to save asset to. Can be `NULL`.

 @param handler
 Completion handler to be invoked with the result of the save action. Can be `NULL`.
 Note that the `receiver` will receive the constant string "camera_roll" on successfully saving the media asset.
 
 @param context
 User context passed to the completion handler. Can be `NULL`.

 @returns Save status.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTAssetSaveToCameraRoll (
    const char* path,
    const char* album,
    VKTAssetShareHandler handler,
    void* context
);
#pragma endregion