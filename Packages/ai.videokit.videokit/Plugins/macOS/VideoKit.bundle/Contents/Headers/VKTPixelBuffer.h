//
//  VKTPixelBuffer.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 3/05/2024.
//  Copyright © 2024 Yusuf Olokoba. All rights reserved.
//

#pragma once

#include <stdbool.h>
#include <stdint.h>
#include <VideoKit/VKTSampleBuffer.h>

#pragma region --Enumerations--
/*!
 @enum VKTPixelFormat

 @abstract Pixel buffer format.

 @constant VKT_PIXEL_FORMAT_UNKNOWN
 Unknown or invalid format.

 @constant VKT_PIXEL_FORMAT_YCbCr420
 YUV semi-planar format.

 @constant VKT_PIXEL_FORMAT_RGBA8888
 RGBA8888 interleaved format.

 @constant VKT_PIXEL_FORMAT_BGRA8888
 BGRA8888 interleaved format.
 */
enum VKTPixelFormat {
    VKT_PIXEL_FORMAT_UNKNOWN     = 0,
    VKT_PIXEL_FORMAT_YCbCr420    = 1,
    VKT_PIXEL_FORMAT_RGBA8888    = 2,
    VKT_PIXEL_FORMAT_BGRA8888    = 3,
};
typedef enum VKTPixelFormat VKTPixelFormat;

/*!
 @enum VKTPixelRotation

 @abstract Pixel buffer rotation constant.

 @constant VKT_PIXEL_ROTATION_0
 No rotation.

 @constant VKT_PIXEL_BUFFER_ROTATION_90
 Rotate 90 degrees counter-clockwise.

 @constant VKT_PIXEL_BUFFER_ROTATION_180
 Rotate 180 degrees counter-clockwise.

 @constant VKT_PIXEL_BUFFER_ROTATION_270
 Rotate 270 degrees counter-clockwise.
*/
enum VKTPixelRotation {
    VKT_PIXEL_ROTATION_0     = 0,
    VKT_PIXEL_ROTATION_90    = 1,
    VKT_PIXEL_ROTATION_180   = 2,
    VKT_PIXEL_ROTATION_270   = 3,
};
typedef enum VKTPixelRotation VKTPixelRotation;
#pragma endregion


#pragma region --Types--
/*!
 @typedef VKTPixelBuffer
 
 @abstract Pixel buffer.

 @discussion Pixel buffer.
*/
typedef VKTSampleBuffer VKTPixelBuffer;
#pragma endregion


#pragma region --Lifecycle--
/*!
 @function VKTPixelBufferCreate
 
 @abstract Create a pixel buffer.
 
 @discussion Create a pixel buffer.

 @param width
 Pixel buffer width.

 @param height
 Pixel buffer height.

 @param format
 Pixel buffer format.

 @param data
 Pixel data.

 @param rowStride
 Pixel buffer row stride.

 @param timestamp
 Pixel buffer timestamp in nanoseconds.

 @param mirrored
 Whether the pixel buffer is vertically mirrored.

 @param pixelBuffer
 Output pixel buffer.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTPixelBufferCreate (
    int32_t width,
    int32_t height,
    VKTPixelFormat format,
    const uint8_t* data,
    int32_t rowStride,
    int64_t timestamp,
    bool mirrored,
    VKTPixelBuffer** pixelBuffer
);

/*!
 @function VKTPixelBufferCreatePlanar
 
 @abstract Create a planar pixel buffer.
 
 @discussion Create a planar pixel buffer.
 
 @param width
 Pixel buffer width.

 @param height
 Pixel buffer height.

 @param format
 Pixel buffer format.

 @param planeCount
 Pixel buffer plane count.

 @param planeData
 Pixel buffer plane data. The size of this array must be `planeCount`.

 @param planeWidth
 Pixel buffer plane widths. The size of this array must be `planeCount`.

 @param planeHeight
 Pixel buffer plane heights. The size of this array must be `planeCount`.

 @param planeRowStride
 Pixel buffer plane row strides. The size of this array must be `planeCount`.

 @param planePixelStride
 Pixel buffer plane pixel strides. The size of this array must be `planeCount`.

 @param timestamp
 Pixel buffer timestamp in nanoseconds.

 @param mirrored
 Whether the pixel buffer is vertically mirrored.

 @param pixelBuffer
 Output pixel buffer.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTPixelBufferCreatePlanar (
    int32_t width,
    int32_t height,
    VKTPixelFormat format,
    int32_t planeCount,
    const uint8_t* const * planeData,
    const int32_t* planeWidth,
    const int32_t* planeHeight,
    const int32_t* planeRowStride,
    const int32_t* planePixelStride,
    int64_t timestamp,
    bool mirrored,
    VKTPixelBuffer** pixelBuffer
);
#pragma endregion


#pragma region --Operations--
/*!
 @function VKTPixelBufferGetData

 @abstract Get the image data of a pixel buffer.

 @discussion Get the image data of a pixel buffer
 If the camera image uses a planar format, this will return `NULL`.

 @param pixelBuffer
 Pixel buffer.

 @param data
 Pixel data.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTPixelBufferGetData (
    VKTPixelBuffer* pixelBuffer,
    void** data
);

/*!
 @function VKTPixelBufferGetDataSize

 @abstract Get the image data size of a pixel buffer in bytes.

 @discussion Get the image data size of a pixel buffer in bytes.

 @param pixelBuffer
 Pixel buffer.

 @param size
 Pixel buffer data size.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTPixelBufferGetDataSize (
    VKTPixelBuffer* pixelBuffer,
    int32_t* size
);

/*!
 @function VKTPixelBufferGetFormat

 @abstract Get the format of a pixel buffer.

 @discussion Get the format of a pixel buffer.

 @param pixelBuffer
 Pixel buffer.

 @param format
 Pixel buffer format.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTPixelBufferGetFormat (
    VKTPixelBuffer* pixelBuffer,
    VKTPixelFormat* format
);

/*!
 @function VKTPixelBufferGetWidth

 @abstract Get the width of a pixel buffer.

 @discussion Get the width of a pixel buffer.

 @param pixelBuffer
 Pixel buffer.

 @param width
 Width of the pixel buffer.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTPixelBufferGetWidth (
    VKTPixelBuffer* pixelBuffer,
    int32_t* width
);

/*!
 @function VKTPixelBufferGetHeight

 @abstract Get the height of a pixel buffer.

 @discussion Get the height of a pixel buffer.

 @param pixelBuffer
 Pixel buffer.

 @param height
 Height of the pixel buffer.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTPixelBufferGetHeight (
    VKTPixelBuffer* pixelBuffer,
    int32_t* height
);

/*!
 @function VKTPixelBufferGetRowStride

 @abstract Get the row stride of a pixel buffer in bytes.

 @discussion Get the row stride of a pixel buffer in bytes.

 @param pixelBuffer
 Pixel buffer.

 @param rowStride
 Pixel buffer row stride in bytes.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTPixelBufferGetRowStride (
    VKTPixelBuffer* pixelBuffer,
    int32_t* rowStride
);

/*!
 @function VKTPixelBufferIsVerticallyMirrored

 @abstract Whether the pixel buffer is vertically mirrored.

 @discussion Whether the pixel buffer is vertically mirrored.

 @param pixelBuffer
 Pixel buffer.

 @param isMirrored
 Whether pixel buffer is vertically mirrored.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTPixelBufferIsVerticallyMirrored (
    VKTPixelBuffer* pixelBuffer,
    bool* isMirrored
);

/*!
 @function VKTPixelBufferGetPlaneCount

 @abstract Get the plane count of a pixel buffer.

 @discussion Get the plane count of a pixel buffer. If the buffer uses an interleaved format or only has a single plane, this function returns zero.

 @param pixelBuffer
 Pixel buffer.

 @param planeCount
 Pixel buffer plane count.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTPixelBufferGetPlaneCount (
    VKTPixelBuffer* pixelBuffer,
    int32_t* planeCount
);

/*!
 @function VKTPixelBufferGetPlaneData

 @abstract Get the plane data for a given plane of a pixel buffer.

 @discussion Get the plane data for a given plane of a pixel buffer.

 @param pixelBuffer
 Pixel buffer.

 @param planeIdx
 Plane index.

 @param planeData
 Pixel buffer plane data.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTPixelBufferGetPlaneData (
    VKTPixelBuffer* pixelBuffer,
    int32_t planeIdx,
    void** planeData
);

/*!
 @function VKTPixelBufferGetPlaneDataSize

 @abstract Get the plane data size of a given plane of a pixel buffer.

 @discussion Get the plane data size of a given plane of a pixel buffer.

 @param pixelBuffer
 Pixel buffer.
 
 @param planeIdx
 Plane index.

 @param dataSize
 Pixel buffer plane data size.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTPixelBufferGetPlaneDataSize (
    VKTPixelBuffer* pixelBuffer,
    int32_t planeIdx,
    int32_t* dataSize
);

/*!
 @function VKTPixelBufferGetPlaneWidth

 @abstract Get the width of a given plane of a pixel buffer.

 @discussion Get the width of a given plane of a pixel buffer.

 @param pixelBuffer
 Pixel buffer.
 
 @param planeIdx
 Plane index.

 @param width
 Plane width.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTPixelBufferGetPlaneWidth (
    VKTPixelBuffer* pixelBuffer,
    int32_t planeIdx,
    int32_t* width
);

/*!
 @function VKTPixelBufferGetPlaneHeight

 @abstract Get the height of a given plane of a pixel buffer.

 @discussion Get the height of a given plane of a pixel buffer.

 @param pixelBuffer
 Pixel buffer.
 
 @param planeIdx
 Plane index.

 @param height
 Plane height.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTPixelBufferGetPlaneHeight (
    VKTPixelBuffer* pixelBuffer,
    int32_t planeIdx,
    int32_t* height
);

/*!
 @function VKTPixelBufferGetPlanePixelStride

 @abstract Get the plane pixel stride for a given plane of a pixel buffer.

 @discussion Get the plane pixel stride for a given plane of a pixel buffer.

 @param pixelBuffer
 Pixel buffer.
 
 @param planeIdx
 Plane index.

 @param pixelStride
 Plane pixel stride in bytes.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTPixelBufferGetPlanePixelStride (
    VKTPixelBuffer* pixelBuffer,
    int32_t planeIdx,
    int32_t* pixelStride
);

/*!
 @function VKTPixelBufferGetPlaneRowStride

 @abstract Get the plane row stride for a given plane of a pixel buffer.

 @discussion Get the plane row stride for a given plane of a pixel buffer.

 @param pixelBuffer
 Pixel buffer.
 
 @param planeIdx
 Plane index.

 @param rowStride
 Plane row stride in bytes.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTPixelBufferGetPlaneRowStride (
    VKTPixelBuffer* pixelBuffer,
    int32_t planeIdx,
    int32_t* rowStride
);

/*!
 @function VKTPixelBufferCopyMetadata

 @abstract Copy the pixel buffer metadata.

 @discussion Copy the pixel buffer metadata.
 The metadata dictionary is specified as a JSON-encoded dictionary.

 @param pixelBuffer
 Pixel buffer.
 
 @param metadata
 Pixel buffer metadata as a JSON-encoded UTF-8 string.

 @param size
 Size of metadata buffer.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTPixelBufferCopyMetadata (
    VKTPixelBuffer* pixelBuffer,
    char* metadata,
    int32_t size
);
#pragma endregion


#pragma region --Conversions--
/*!
 @function VKTPixelBufferCopyTo

 @abstract Copy the pixel buffer data to another pixel buffer.

 @discussion Copy the pixel buffer data to another pixel buffer.
 This handles pixel buffer format conversions.

 @param source
 Source pixel buffer.
 
 @param destination
 Destination pixel buffer.

 @param rotation
 Rotation to apply when copying.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTPixelBufferCopyTo (
    VKTPixelBuffer* source,
    VKTPixelBuffer* destination,
    VKTPixelRotation rotation
);
#pragma endregion