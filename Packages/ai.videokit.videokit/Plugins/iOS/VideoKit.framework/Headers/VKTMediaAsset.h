//
//  VKTMediaAsset.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 7/08/2023.
//  Copyright © 2024 Yusuf Olokoba. All rights reserved.
//

#pragma once

#include <stdint.h>
#include <VideoKit/VKTSession.h>

#pragma region --Enumerations--
/*!
 @enum VKTMediaType

 @abstract Media type.

 @constant VKT_MEDIA_TYPE_UNKNOWN
 Unknown or unsupported media type.

 @constant VKT_MEDIA_TYPE_IMAGE
 Image asset.

 @constant VKT_MEDIA_TYPE_AUDIO
 Audio asset.

 @constant VKT_MEDIA_TYPE_VIDEO
 Video asset.

 @constant VKT_MEDIA_TYPE_TEXT
 Text asset.

 @constant VKT_MEDIA_TYPE_SEQUENCE
 Sequence of media assets.
*/
enum VKTMediaType {
    VKT_MEDIA_TYPE_UNKNOWN  = 0,
    VKT_MEDIA_TYPE_IMAGE    = 1,
    VKT_MEDIA_TYPE_AUDIO    = 2,
    VKT_MEDIA_TYPE_VIDEO    = 3,
    VKT_MEDIA_TYPE_TEXT     = 4,
    VKT_MEDIA_TYPE_SEQUENCE = 5,
};
typedef enum VKTMediaType VKTMediaType;
#pragma endregion


#pragma region --Types--
/*!
 @struct VKTMediaAsset
 
 @abstract Media asset.

 @discussion Media asset.
*/
struct VKTMediaAsset;
typedef struct VKTMediaAsset VKTMediaAsset;

/*!
 @typedef VKTMediaAssetHandler

 @abstract Callback invoked with a media asset.
 
 @param context
 User context provided to the load function.

 @param asset
 Media asset.
*/
typedef void (*VKTMediaAssetHandler) (
    void* context,
    VKTMediaAsset* asset
);

/*!
 @abstract Callback invoked with the result of the sharing action.
 
 @param context
 User context provided to the share function.
 
 @param receiver
 Identifier of receiving application chosen by the user.
*/
typedef void (*VKTMediaAssetShareHandler) (void* context, const char* receiver);
#pragma endregion


#pragma region --Lifecycle--
/*!
 @function VKTMediaAssetCreate
 
 @abstract Create a media asset.
 
 @discussion Create a media asset.
 
 @param path
 Path to media asset.

 @param handler
 Completion handler to be invoked with the result of the create action.
 
 @param context
 User context passed to the load handler. Can be `NULL`.

 @returns Load status.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaAssetCreate (
    const char* path,
    VKTMediaAssetHandler handler,
    void* context
);

/*!
 @function VKTMediaAssetCreateFromCameraRoll
 
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
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaAssetCreateFromCameraRoll (
    VKTMediaType type,
    VKTMediaAssetHandler handler,
    void* context
);

/*!
 @function VKTMediaAssetRelease

 @abstract Release a media asset.

 @discussion Release a media asset.

 @param asset
 Media asset.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaAssetRelease (VKTMediaAsset* asset);
#pragma endregion


#pragma region --Operations--
/*!
 @function VKTMediaAssetGetPath
 
 @abstract Get the media asset path.
 
 @discussion Get the media asset path.
 
 @param asset
 Media asset.

 @param path
 Media asset path.

 @param size
 Path buffer size.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaAssetGetPath (
    VKTMediaAsset* asset,
    char* path,
    int32_t size
);

/*!
 @function VKTMediaAssetGetMediaType
 
 @abstract Get the media asset media type.
 
 @discussion Get the media asset media type.
 
 @param asset
 Media asset.

 @param type
 Media type.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaAssetGetMediaType (
    VKTMediaAsset* asset,
    VKTMediaType* type
);

/*!
 @function VKTMediaAssetGetWidth
 
 @abstract Get the media asset width in pixels.
 
 @discussion Get the media asset width in pixels.
 This is only valid for image and video assets.
 
 @param asset
 Media asset.

 @param width
 Media width.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaAssetGetWidth (
    VKTMediaAsset* asset,
    int32_t* width
);

/*!
 @function VKTMediaAssetGetHeight
 
 @abstract Get the media asset height in pixels.
 
 @discussion Get the media asset height in pixels.
 This is only valid for image and video assets.
 
 @param asset
 Media asset.

 @param height
 Media height.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaAssetGetHeight (
    VKTMediaAsset* asset,
    int32_t* height
);

/*!
 @function VKTMediaAssetGetFrameRate
 
 @abstract Get the media asset frame rate.
 
 @discussion Get the media asset frame rate.
 This is only valid for video assets.
 
 @param asset
 Media asset.

 @param frameRate
 Video frame rate.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaAssetGetFrameRate (
    VKTMediaAsset* asset,
    float* frameRate
);

/*!
 @function VKTMediaAssetGetSampleRate
 
 @abstract Get the media asset sample rate.
 
 @discussion Get the media asset sample rate.
 This is only valid for audio and video assets.
 
 @param asset
 Media asset.

 @param sampleRate
 Audio sample rate.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaAssetGetSampleRate (
    VKTMediaAsset* asset,
    int32_t* sampleRate
);

/*!
 @function VKTMediaAssetGetChannelCount
 
 @abstract Get the media asset channel count.
 
 @discussion Get the media asset channel count.
 This is only valid for audio and video assets.
 
 @param asset
 Media asset.

 @param channelCount
 Audio channel count.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaAssetGetChannelCount (
    VKTMediaAsset* asset,
    int32_t* channelCount
);

/*!
 @function VKTMediaAssetGetChannelCount
 
 @abstract Get the media asset duration.
 
 @discussion Get the media asset duration.
 This is only valid for audio and video assets.
 
 @param asset
 Media asset.

 @param duration
 Asset duration in seconds.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaAssetGetDuration (
    VKTMediaAsset* asset,
    float* duration
);

/*!
 @function VKTMediaAssetGetSubAssetCount
 
 @abstract Get the media asset sub-asset count.
 
 @discussion Get the media asset sub-asset count.
 This is only valid for sequence assets.
 
 @param asset
 Media asset.

 @param count
 Sub-asset count.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaAssetGetSubAssetCount (
    VKTMediaAsset* asset,
    int32_t* count
);

/*!
 @function VKTMediaAssetGetSubAsset
 
 @abstract Get a media asset sub-asset at a given index.
 
 @discussion Get a media asset sub-asset at a given index.
 This is only valid for sequence assets.
 
 @param asset
 Media asset.

 @param index
 Sub-asset index.

 @param subAsset
 Sub-asset.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaAssetGetSubAsset (
    VKTMediaAsset* asset,
    int32_t index,
    VKTMediaAsset** subAsset
);
#pragma endregion


#pragma region --Sharing--
/*!
 @function VKTMediaAssetShare
 
 @abstract Share a media asset.
 
 @discussion Share a media asset.
 
 @param asset
 Media asset.

 @param message
 Message to share along with the media asset. Can be `NULL`.

 @param handler
 Completion handler to be invoked with the result of the sharing action. Can be `NULL`.
 
 @param context
 User context passed to the completion handler. Can be `NULL`.

 @returns Share status.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaAssetShare (
    VKTMediaAsset* asset,
    const char* message,
    VKTMediaAssetShareHandler handler,
    void* context
);

/*!
 @function VKTMediaAssetSaveToCameraRoll
 
 @abstract Save a media asset to the camera roll.
 
 @discussion Save a media asset to the camera roll.
 
 @param asset
 Media asset.

 @param album
 Name of album to save asset to. Can be `NULL`.

 @param handler
 Completion handler to be invoked with the result of the save action. Can be `NULL`.
 Note that the `receiver` will receive the constant string "camera_roll" on successfully saving the media asset.
 
 @param context
 User context passed to the completion handler. Can be `NULL`.

 @returns Save status.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaAssetSaveToCameraRoll (
    VKTMediaAsset* asset,
    const char* album,
    VKTMediaAssetShareHandler handler,
    void* context
);
#pragma endregion