//
//  VKTMediaReader.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 3/05/2024.
//  Copyright © 2024 Yusuf Olokoba. All rights reserved.
//

#pragma once

#include <stdint.h>
#include <VideoKit/VKTMediaAsset.h>
#include <VideoKit/VKTSampleBuffer.h>
#include <VideoKit/VKTAudioBuffer.h>
#include <VideoKit/VKTPixelBuffer.h>

#pragma region --Types--
/*!
 @struct VKTMediaReader
 
 @abstract Asset reader for reading pixel and audio data from media sources.

 @discussion Asset reader for reading pixel and audio data from media sources.
*/
struct VKTMediaReader;
typedef struct VKTMediaReader VKTMediaReader;
#pragma endregion


#pragma region --Operations--
/*!
 @function VKTMediaReaderCreate

 @abstract Create an asset reader.

 @discussion Create an asset reader.

 @param asset
 Media asset.

 @param type
 Reader type.

 @param reader
 Asset reader.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaReaderCreate (
    VKTMediaAsset* asset,
    VKTMediaType type,
    VKTMediaReader** reader
);

/*!
 @function VKTMediaReaderRelease

 @abstract Release an asset reader.

 @discussion Release an asset reader.

 @param reader
 Asset reader.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaReaderRelease (VKTMediaReader* reader);

/*!
 @function VKTMediaReaderReadNextSampleBuffer

 @abstract Read the next feature from a feature reader.
 
 @discussion Read the next feature from a feature reader.
 The feature must be released with when it is no longer needed.

 @param reader
 Asset reader.

 @param sampleBuffer
 Sample buffer to read.

 @returns `VKT_OK` if the sample buffer was successfully read.
 `VKT_ERROR_INVALID_OPERATION` when there are no more sample buffers to be read.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTMediaReaderReadNextSampleBuffer (
    VKTMediaReader* reader,
    VKTSampleBuffer** sampleBuffer
);
#pragma endregion