//
//  VKTMetadata.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 3/13/2024.
//  Copyright © 2024 NatML Inc. All rights reserved.
//

#pragma once

#include <stdint.h>
#include <VideoKit/VKTAPI.h>
#include <VideoKit/VKTStatus.h>

#pragma region --Metadata--
enum VKTMetadataType {
    VKT_METADATA_TYPE_UNKNOWN   = 0,
    VKT_METADATA_TYPE_FLOAT     = 1,
    VKT_METADATA_TYPE_INT       = 2,
};
typedef enum VKTMetadataType VKTMetadataType;
#pragma endregion


#pragma region --Types--
/*!
 @typedef VKTMetadata
 
 @abstract Sample buffer metadata.

 @discussion Sample buffer metadata.
*/
struct VKTMetadata;
typedef struct VKTMetadata VKTMetadata;
#pragma endregion


#pragma region --Operations--
/*!
 @function VKTMetadataGetCount

 @abstract Get the metadata item count.

 @discussion Get the metadata item count.

 @param metadata
 Metadata.

 @param count
 Metadata count.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMetadataGetCount (
    VKTMetadata* metadata,
    int32_t* count
);

/*!
 @function VKTMetadataContainsKey

 @abstract Check whether the metadata dictionary contains a given key.

 @discussion Check whether the metadata dictionary contains a given key.

 @param metadata
 Metadata.

 @param key
 Metadata key.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMetadataContainsKey (
    VKTMetadata* metadata,
    const char* key
);

/*!
 @function VKTMetadataGetKeys

 @abstract Get all keys in the metadata dictionary.

 @discussion Get all keys in the metadata dictionary.

 @param metadata
 Metadata.

 @param keys
 Metadata keys. Size of this buffer must be at least as large as the metadata count.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMetadataGetKeys (
    VKTMetadata* metadata,
    const char** keys
);

VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMetadataGetFloatValue (
    VKTMetadata* metadata,
    const char* key,
    float* value,
    int32_t* count
);
#pragma endregion
