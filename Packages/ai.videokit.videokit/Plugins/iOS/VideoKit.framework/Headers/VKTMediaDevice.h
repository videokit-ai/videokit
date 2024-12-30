//
//  VKTMediaDevice.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 5/15/2023.
//  Copyright © 2025 Yusuf Olokoba. All rights reserved.
//

#pragma once

#include <stdbool.h>
#include <stdint.h>
#include <VideoKit/VKTSampleBuffer.h>

#pragma region --Enumerations--
/*!
 @enum VKTMediaDeviceFlags
 
 @abstract Immutable properties of media devices.
 
 @constant VKT_MEDIA_DEVICE_FLAG_INTERNAL
 Media device is internal.

 @constant VKT_MEDIA_DEVICE_FLAG_EXTERNAL
 Media device is external.

 @constant VKT_MEDIA_DEVICE_FLAG_DEFAULT
 Media device is the default device for its media type.

 @constant VKT_AUDIO_DEVICE_FLAG_ECHO_CANCELLATION
 Audio device supports echo cancellation.

 @constant VKT_CAMERA_DEVICE_FLAG_FRONT_FACING
 Camera device is front-facing.

 @constant VKT_CAMERA_DEVICE_FLAG_FLASH
 Camera device supports flash when capturing photos.

 @constant VKT_CAMERA_DEVICE_FLAG_TORCH
 Camera device supports torch.

 @constant VKT_CAMERA_DEVICE_FLAG_DEPTH
 Camera device supports depth streaming.

 @constant VKT_CAMERA_DEVICE_FLAG_EXPOSURE_CONTINUOUS
 Camera device supports continuous auto exposure.

 @constant VKT_CAMERA_DEVICE_FLAG_EXPOSURE_LOCK
 Camera device supports locked auto exposure.

 @constant VKT_CAMERA_DEVICE_FLAG_EXPOSURE_MANUAL
 Camera device supports manual exposure.

 @constant VKT_CAMERA_DEVICE_FLAG_EXPOSURE_POINT
 Camera device supports setting exposure point.

 @constant VKT_CAMERA_DEVICE_FLAG_FOCUS_CONTINUOUS
 Camera device supports continuous auto exposure.

 @constant VKT_CAMERA_DEVICE_FLAG_LOCKED_FOCUS
 Camera device supports locked auto focus.

 @constant VKT_CAMERA_DEVICE_FLAG_FOCUS_POINT
 Camera device supports setting focus point.

 @constant VKT_CAMERA_DEVICE_FLAG_WHITE_BALANCE_CONTINUOUS
 Camera device supports continuous auto white balance.

 @constant VKT_CAMERA_DEVICE_FLAG_WHITE_BALANCE_LOCK
 Camera device supports locked auto white balance.

 @constant VKT_CAMERA_DEVICE_FLAG_VIDEO_STABILIZATION
 Camera device supports video stabilization.
*/
enum VKTMediaDeviceFlags {
    VKT_MEDIA_DEVICE_FLAG_INTERNAL                  = 1 << 0,
    VKT_MEDIA_DEVICE_FLAG_EXTERNAL                  = 1 << 1,
    VKT_MEDIA_DEVICE_FLAG_DEFAULT                   = 1 << 3,
    VKT_AUDIO_DEVICE_FLAG_ECHO_CANCELLATION         = 1 << 2,
    VKT_CAMERA_DEVICE_FLAG_FRONT_FACING             = 1 << 6,
    VKT_CAMERA_DEVICE_FLAG_FLASH                    = 1 << 7,
    VKT_CAMERA_DEVICE_FLAG_TORCH                    = 1 << 8,
    VKT_CAMERA_DEVICE_FLAG_DEPTH                    = 1 << 15,
    VKT_CAMERA_DEVICE_FLAG_EXPOSURE_CONTINUOUS      = 1 << 16,
    VKT_CAMERA_DEVICE_FLAG_EXPOSURE_LOCK            = 1 << 11,
    VKT_CAMERA_DEVICE_FLAG_EXPOSURE_MANUAL          = 1 << 14,
    VKT_CAMERA_DEVICE_FLAG_EXPOSURE_POINT           = 1 << 9,
    VKT_CAMERA_DEVICE_FLAG_FOCUS_CONTINUOUS         = 1 << 17,
    VKT_CAMERA_DEVICE_FLAG_FOCUS_LOCK               = 1 << 12,
    VKT_CAMERA_DEVICE_FLAG_FOCUS_POINT              = 1 << 10,
    VKT_CAMERA_DEVICE_FLAG_WHITE_BALANCE_CONTINUOUS = 1 << 18,
    VKT_CAMERA_DEVICE_FLAG_WHITE_BALANCE_LOCK       = 1 << 13,
    VKT_CAMERA_DEVICE_FLAG_VIDEO_STABILIZATION      = 1 << 19,
};
typedef enum VKTMediaDeviceFlags VKTMediaDeviceFlags;

/*!
 @enum VKTMediaDevicePermissionType
 
 @abstract Media device permission type.
 
 @constant VKT_DEVICE_PERMISSION_TYPE_MICROPHONE
 Request microphone permissions.
 
 @constant VKT_DEVICE_PERMISSION_TYPE_CAMERA
 Request camera permissions.
*/
enum VKTMediaDevicePermissionType {
    VKT_DEVICE_PERMISSION_TYPE_MICROPHONE  = 1,
    VKT_DEVICE_PERMISSION_TYPE_CAMERA      = 2,
};
typedef enum VKTMediaDevicePermissionType VKTMediaDevicePermissionType;

/*!
 @enum VKTMediaDevicePermissionStatus
 
 @abstract Media device permission status.
 
 @constant VKT_DEVICE_PERMISSION_STATUS_UNKNOWN
 User has not authorized or denied access to media device.
 
 @constant VKT_DEVICE_PERMISSION_STATUS_DENIED
 User has denied access to media device.

 @constant VKT_DEVICE_PERMISSION_STATUS_AUTHORIZED
 User has authorized access to media device.
*/
enum VKTMediaDevicePermissionStatus {
    VKT_DEVICE_PERMISSION_STATUS_UNKNOWN    = 0,
    VKT_DEVICE_PERMISSION_STATUS_DENIED     = 2,
    VKT_DEVICE_PERMISSION_STATUS_AUTHORIZED = 3,
};
typedef enum VKTMediaDevicePermissionStatus VKTMediaDevicePermissionStatus;
#pragma endregion


#pragma region --Types--
/*!
 @struct VKTMediaDevice
 
 @abstract Media device.

 @discussion Media device.
*/
struct VKTMediaDevice;
typedef struct VKTMediaDevice VKTMediaDevice;

/*!
 @abstract Callback invoked with discovered media devices.
 
 @param context
 User-provided context.
 
 @param devices
 Discovered devices. These MUST be released when they are no longer needed.

 @param count
 Discovered device count.
*/
typedef void (*VKTMediaDeviceDiscoveryHandler) (
    void* context,
    VKTMediaDevice** devices,
    int32_t count
);

/*!
 @abstract Callback invoked when a camera device is disconnected.
 
 @param context
 User-provided context.

 @param device
 Media device that was disconnected.
*/
typedef void (*VKTMediaDeviceDisconnectHandler) (
    void* context,
    VKTMediaDevice* device
);

/*!
 @abstract Callback invoked with status of a media device permission request.
 
 @param context
 User-provided context.
 
 @param status
 Permission status.
*/
typedef void (*VKTMediaDevicePermissionHandler) (
    void* context,
    VKTMediaDevicePermissionStatus status
);
#pragma endregion


#pragma region --Operations--
/*!
 @function VKTMediaDeviceRelease

 @abstract Release a media device.

 @discussion Release a media device.

 @param device
 Media device.
*/
VKT_API VKTStatus VKTMediaDeviceRelease (VKTMediaDevice* device);

/*!
 @function VKTMediaDeviceGetUniqueID

 @abstract Get the media device unique ID.

 @discussion Get the media device unique ID.

 @param device
 Media device.

 @param destination
 Destination UTF-8 string.

 @param size
 Destination buffer size.
*/
VKT_API VKTStatus VKTMediaDeviceGetUniqueID (
    VKTMediaDevice* device,
    char* destination,
    int32_t size
);

/*!
 @function VKTMediaDeviceGetName
 
 @abstract Media device name.
 
 @discussion Media device name.
 
 @param device
 Media device.
 
 @param destination
 Destination UTF-8 string.

 @param size
 Destination buffer size.
*/
VKT_API VKTStatus VKTMediaDeviceGetName (
    VKTMediaDevice* device,
    char* destination,
    int32_t size
);

/*!
 @function VKTMediaDeviceGetFlags
 
 @abstract Get the media device flags.
 
 @discussion Get the media device flags.
 
 @param device
 Media device.

 @param flags
 Device flags.
*/
VKT_API VKTStatus VKTMediaDeviceGetFlags (
    VKTMediaDevice* device,
    VKTMediaDeviceFlags* flags
);

/*!
 @function VKTMediaDeviceIsRunning

 @abstract Check whether the device is running.

 @discussion Check whether the device is running.

 @param device
 Media device.

 @param running
 Whether the device is running.
*/
VKT_API VKTStatus VKTMediaDeviceIsRunning (
    VKTMediaDevice* device,
    bool* running
);

/*!
 @function VKTMediaDeviceStartRunning
 
 @abstract Start running an media device.
 
 @discussion Start running an media device.
 
 @param device
 Media device.

 @param sampleBufferHandler
 Sample buffer delegate to receive sample buffers as the device produces them.

 @param context
 User-provided context to be passed to the sample buffer delegate. Can be `NULL`.
*/
VKT_API VKTStatus VKTMediaDeviceStartRunning (
    VKTMediaDevice* device,
    VKTSampleBufferHandler sampleBufferHandler,
    void* context
);

/*!
 @function VKTMediaDeviceStopRunning
 
 @abstract Stop running device.
 
 @discussion Stop running device.
 
 @param device
 Media device.
*/
VKT_API VKTStatus VKTMediaDeviceStopRunning (VKTMediaDevice* device);

/*!
 @function VKTMediaDeviceSetDisconnectHandler

 @abstract Set the device disconnect handler.

 @discussion Set the device disconnect handler.
 This provided function pointer is invoked when the device is disconnected.

 @param device
 Media device.

 @param disconnectHandler
 Device disconnect handler. Can be `NULL`.

 @param context
 User-provided context. Can be `NULL`.
*/
VKT_API VKTStatus VKTMediaDeviceSetDisconnectHandler (
    VKTMediaDevice* device,
    VKTMediaDeviceDisconnectHandler disconnectHandler,
    void* context
);

/*!
 @function VKTMediaDeviceCheckPermissions
 
 @abstract Check permissions for a given media device type.
 
 @discussion Check permissions for a given media device type.
 
 @param type
 Permission type.

 @param request
 Whether to request the permissions if the user has not been asked.
 
 @param handler
 Permission delegate to receive result of permission request.

 @param context
 User-provided context to be passed to the permission delegate. Can be `NULL`.
*/
VKT_API VKTStatus VKTMediaDeviceCheckPermissions (
    VKTMediaDevicePermissionType type,
    bool request,
    VKTMediaDevicePermissionHandler handler,
    void* context
);
#pragma endregion
