//
//  VKTCameraDevice.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 5/15/2023.
//  Copyright © 2024 Yusuf Olokoba. All rights reserved.
//

#pragma once

#include <VideoKit/VKTMediaDevice.h>

#pragma region --Enumerations--
/*!
 @enum VKTExposureMode

 @abstract Camera device exposure mode.

 @constant VKT_EXPOSURE_MODE_CONTINUOUS
 Continuous auto exposure.

 @constant VKT_EXPOSURE_MODE_LOCKED
 Locked exposure. Exposure settings will be fixed to their current values.
 Requires `VKT_CAMERA_FLAG_LOCKED_EXPOSURE` device flag.

 @constant VKT_EXPOSURE_MODE_MANUAL
 Manual exposure. User will set exposure duration and sensitivity.
 Requires `VKT_CAMERA_FLAG_MANUAL_EXPOSURE` device flag.
*/
enum VKTExposureMode {
    VKT_EXPOSURE_MODE_CONTINUOUS         = 0,
    VKT_EXPOSURE_MODE_LOCKED             = 1,
    VKT_EXPOSURE_MODE_MANUAL             = 2
};
typedef enum VKTExposureMode VKTExposureMode;

/*!
 @enum VKTFlashMode

 @abstract Camera device photo flash modes.

 @constant VKT_FLASH_MODE_OFF
 The flash will never be fired.

 @constant VKT_FLASH_MODE_ON
 The flash will always be fired.

 @constant VKT_FLASH_MODE_AUTO
 The sensor will determine whether to fire the flash.
*/
enum VKTFlashMode {
    VKT_FLASH_MODE_OFF       = 0,
    VKT_FLASH_MODE_ON        = 1,
    VKT_FLASH_MODE_AUTO      = 2
};
typedef enum VKTFlashMode VKTFlashMode;

/*!
 @enum VKTFocusMode

 @abstract Camera device focus mode.

 @constant VKT_FOCUS_MODE_CONTINUOUS
 Continuous auto focus.

 @constant VKT_FOCUS_MODE_LOCKED
 Locked auto focus. Focus settings will be fixed to their current values.
 Requires `VKT_CAMERA_FLAG_FOCUS_LOCK` device flag.
*/
enum VKTFocusMode {
    VKT_FOCUS_MODE_CONTINUOUS    = 0,
    VKT_FOCUS_MODE_LOCKED        = 1,
};
typedef enum VKTFocusMode VKTFocusMode;

/*!
 @enum VKTTorchMode

 @abstract Camera device torch mode.

 @constant VKT_TORCH_MODE_OFF
 Disabled torch mode.

 @constant VKT_TORCH_MODE_MAXIMUM
 Maximum torch mode.
 Requires `VKT_CAMERA_FLAG_TORCH` device flag.
*/
enum VKTTorchMode {
    VKT_TORCH_MODE_OFF       = 0,
    VKT_TORCH_MODE_MAXIMUM   = 100,
};
typedef enum VKTTorchMode VKTTorchMode;

/*!
 @enum VKTVideoStabilizationMode

 @abstract Camera device video stabilization mode.

 @constant VKT_VIDEO_STABILIZATION_OFF
 Disabled video stabilization.

 @constant VKT_VIDEO_STABILIZATION_STANDARD
 Standard video stabilization
 Requires `VKT_CAMERA_FLAG_VIDEO_STABILIZATION` device flag.
*/
enum VKTVideoStabilizationMode {
    VKT_VIDEO_STABILIZATION_OFF      = 0,
    VKT_VIDEO_STABILIZATION_STANDARD = 1,
};
typedef enum VKTVideoStabilizationMode VKTVideoStabilizationMode;

/*!
 @enum VKTWhiteBalanceMode

 @abstract Camera device white balance mode.

 @constant VKT_WHITE_BALANCE_MODE_CONTINUOUS
 Continuous auto white balance.

 @constant VKT_WHITE_BALANCE_MODE_LOCKED
 Locked auto white balance. White balance settings will be fixed to their current values.
 Requires `VKT_CAMERA_FLAG_WHITE_BALANCE_LOCK` device flag.
*/
enum VKTWhiteBalanceMode {
    VKT_WHITE_BALANCE_MODE_CONTINUOUS    = 0,
    VKT_WHITE_BALANCE_MODE_LOCKED        = 1,
};
typedef enum VKTWhiteBalanceMode VKTWhiteBalanceMode;
#pragma endregion


#pragma region --Types--
/*!
 @typedef VKTCameraDevice
 
 @abstract Camera device.

 @discussion Camera device.
*/
typedef VKTMediaDevice VKTCameraDevice;
#pragma endregion


#pragma region --CameraDevice--
/*!
 @function VKTCameraDeviceDiscoverDevices

 @abstract Discover available camera devices.

 @discussion Discover available camera devices.

 @param handler
 Device handler. MUST NOT be `NULL`.

 @param context
 Handler context.

 @returns Status.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceDiscoverDevices (
    VKTMediaDeviceDiscoveryHandler handler,
    void* context
);

/*!
 @function VKTCameraDeviceGetFieldOfView

 @abstract Camera field of view in degrees.

 @discussion Camera field of view in degrees.

 @param camera
 Camera device.

 @param width
 Output FOV width in degrees. Can be `NULL`.

 @param height
 Output FOV height in degrees. Can be `NULL`.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceGetFieldOfView (
    VKTCameraDevice* camera,
    float* width,
    float* height
);

/*!
 @function VKTCameraDeviceGetExposureBiasRange

 @abstract Camera exposure bias range in EV.

 @discussion Camera exposure bias range in EV.

 @param camera
 Camera device.

 @param outMin
 Output minimum exposure bias.

 @param outMax
 Output maximum exposure bias.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceGetExposureBiasRange (
    VKTCameraDevice* camera,
    float* outMin,
    float* outMax
);

/*!
 @function VKTCameraDeviceGetExposureDurationRange

 @abstract Camera exposure duration range in seconds.

 @discussion Camera exposure duration range in seconds.

 @param camera
 Camera device.

 @param min
 Minimum exposure duration in seconds. Can be `NULL`.

 @param max
 Maximum exposure duration in seconds. Can be `NULL`.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceGetExposureDurationRange (
    VKTCameraDevice* camera,
    float* min,
    float* max
);

/*!
 @function VKTCameraDeviceGetISORange

 @abstract Camera sensor sensitivity range.

 @discussion Camera sensor sensitivity range.

 @param camera
 Camera device.

 @param min
 Minimum ISO sensitivity value. Can be `NULL`.

 @param max
 Maximum ISO sensitivity value. Can be `NULL`.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceGetISORange (
    VKTCameraDevice* camera,
    float* min,
    float* max
);

/*!
 @function VKTCameraDeviceGetZoomRange

 @abstract Camera optical zoom range.

 @discussion Camera optical zoom range.

 @param camera
 Camera device.

 @param min
 Minimum zoom ratio. Can be `NULL`.

 @param max
 Maximum zoom ratio. Can be `NULL`.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceGetZoomRange (
    VKTCameraDevice* camera, 
    float* min, 
    float* max
);

/*!
 @function VKTCameraDeviceGetPreviewResolution

 @abstract Get the camera preview resolution.

 @discussion Get the camera preview resolution.

 @param camera
 Camera device.

 @param width
 Width in pixels. Can be `NULL`.

 @param height
 Height in pixels. Can be `NULL`.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceGetPreviewResolution (
    VKTCameraDevice* camera,
    int32_t* width,
    int32_t* height
);

/*!
 @function VKTCameraDeviceSetPreviewResolution

 @abstract Set the camera preview resolution.

 @discussion Set the camera preview resolution.

 Most camera devices do not support arbitrary preview resolutions, so the camera will
 set a supported resolution which is closest to the requested resolution that is specified.

 Note that this method should only be called before the camera preview is started.

 @param camera
 Camera device.

 @param width
 Width in pixels.

 @param height
 Height in pixels.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceSetPreviewResolution (
    VKTCameraDevice* camera,
    int32_t width,
    int32_t height
);

/*!
 @function VKTCameraDeviceGetPhotoResolution

 @abstract Get the camera photo resolution.

 @discussion Get the camera photo resolution.

 @param camera
 Camera device.

 @param width
 Output width in pixels. Can be `NULL`.

 @param height
 Output height in pixels. Can be `NULL`.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceGetPhotoResolution (
    VKTCameraDevice* camera,
    int32_t* width,
    int32_t* height
);

/*!
 @function VKTCameraDeviceSetPhotoResolution

 @abstract Set the camera photo resolution.

 @discussion Set the camera photo resolution.

 Most camera devices do not support arbitrary photo resolutions, so the camera will
 set a supported resolution which is closest to the requested resolution that is specified.

 Note that this method should only be called before the camera preview is started.

 @param camera
 Camera device.

 @param width
 Width in pixels.

 @param height
 Height in pixels.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceSetPhotoResolution (
    VKTCameraDevice* camera,
    int32_t width,
    int32_t height
);

/*!
 @function VKTCameraDeviceGetFrameRate

 @abstract Get the camera preview frame rate.

 @discussion Get the camera preview frame rate.

 @param camera
 Camera device.

 @param frameRate
 Preview frame rate.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceGetFrameRate (
    VKTCameraDevice* camera,
    float* frameRate
);

/*!
 @function VKTCameraDeviceSetFrameRate

 @abstract Set the camera preview frame rate.

 @discussion Set the camera preview frame rate.

 Note that this method should only be called before the camera preview is started.

 @param camera
 Camera device.

 @param frameRate
 Frame rate.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceSetFrameRate (
    VKTCameraDevice* camera,
    float frameRate
);

/*!
 @function VKTCameraDeviceGetExposureMode

 @abstract Get the camera exposure mode.

 @discussion Get the camera exposure mode.

 @param camera
 Camera device.

 @param exposureMode
 Exposure mode.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceGetExposureMode (
    VKTCameraDevice* camera,
    VKTExposureMode* exposureMode
);

/*!
 @function VKTCameraDeviceSetExposureMode

 @abstract Set the camera exposure mode.

 @discussion Set the camera exposure mode.

 @param camera
 Camera device.

 @param mode
 Exposure mode.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceSetExposureMode (
    VKTCameraDevice* camera,
    VKTExposureMode mode
);

/*!
 @function VKTCameraDeviceGetExposureBias

 @abstract Get the camera exposure bias.

 @discussion Get the camera exposure bias.

 @param camera
 Camera device.

 @param bias
 Exposure bias.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceGetExposureBias (
    VKTCameraDevice* camera,
    float* bias
);

/*!
 @function VKTCameraDeviceSetExposureBias

 @abstract Set the camera exposure bias.

 @discussion Set the camera exposure bias.

 Note that the value MUST be in the camera exposure range.

 @param camera
 Camera device.

 @param bias
 Exposure bias value to set.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceSetExposureBias (
    VKTCameraDevice* camera,
    float bias
);

/*!
 @function VKTCameraDeviceGetExposureDuration

 @abstract Get the camera exposure duration.

 @discussion Get the camera exposure duration.

 @param camera
 Camera device.

 @param duration
 Exposure duration in seconds.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceGetExposureDuration (
    VKTCameraDevice* camera,
    float* duration
);

/*!
 @function VKTCameraDeviceGetISO

 @abstract Get the camera sensitivity.

 @discussion Get the camera sensitivity.

 @param camera
 Camera device.

 @param ISO
 Camera sensitivity.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceGetISO (
    VKTCameraDevice* camera,
    float* ISO
);

/*!
 @function VKTCameraDeviceSetExposureDuration

 @abstract Set the camera exposure duration.

 @discussion Set the camera exposure duration.
 This method will automatically change the camera's exposure mode to `MANUAL`.

 @param camera
 Camera device.

 @param duration
 Exposure duration in seconds. MUST be in `ExposureDurationRange`.

 @param ISO
 Shutter sensitivity. MUST be in `ISORange`.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceSetExposureDuration (
    VKTCameraDevice* camera,
    float duration,
    float ISO
);

/*!
 @function VKTCameraDeviceSetExposurePoint

 @abstract Set the camera exposure point of interest.

 @discussion Set the camera exposure point of interest.
 The coordinates are specified in viewport space, with each value in range [0., 1.].

 @param camera
 Camera device.

 @param x
 Exposure point x-coordinate in viewport space.

 @param y
 Exposure point y-coordinate in viewport space.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceSetExposurePoint (
    VKTCameraDevice* camera,
    float x,
    float y
);

/*!
 @function VKTCameraDeviceGetFlashMode

 @abstract Get the camera photo flash mode.

 @discussion Get the camera photo flash mode.

 @param camera
 Camera device.

 @param mode
 Photo flash mode.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceGetFlashMode (
    VKTCameraDevice* camera,
    VKTFlashMode* mode
);

/*!
 @function VKTCameraDeviceSetFlashMode

 @abstract Set the camera photo flash mode.

 @discussion Set the camera photo flash mode.

 @param camera
 Camera device.

 @param mode
 Flash mode to set.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceSetFlashMode (
    VKTCameraDevice* camera,
    VKTFlashMode mode
);

/*!
 @function VKTCameraDeviceGetFocusMode

 @abstract Get the camera focus mode.

 @discussion Get the camera focus mode.

 @param camera
 Camera device.

 @param mode
 Focus mode.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceGetFocusMode (
    VKTCameraDevice* camera,
    VKTFocusMode* mode
);

/*!
 @function VKTCameraDeviceSetFocusMode

 @abstract Set the camera focus mode.

 @discussion Set the camera focus mode.

 @param camera
 Camera device.

 @param mode
 Focus mode.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceSetFocusMode (
    VKTCameraDevice* camera,
    VKTFocusMode mode
);

/*!
 @function VKTCameraDeviceSetFocusPoint

 @abstract Set the camera focus point.

 @discussion Set the camera focus point of interest.
 The coordinates are specified in viewport space, with each value in range [0., 1.].
 This function should only be used if the camera supports setting the focus point.
 See `VKTCameraDeviceFocusPointSupported`.

 @param camera
 Camera device.

 @param x
 Focus point x-coordinate in viewport space.

 @param y
 Focus point y-coordinate in viewport space.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceSetFocusPoint (
    VKTCameraDevice* camera,
    float x,
    float y
);

/*!
 @function VKTCameraDeviceGetTorchMode

 @abstract Get the current camera torch mode.

 @discussion Get the current camera torch mode.

 @param camera
 Camera device.

 @param mode
 Torch mode.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceGetTorchMode (
    VKTCameraDevice* camera,
    VKTTorchMode* mode
);

/*!
 @function VKTCameraDeviceSetTorchMode

 @abstract Set the camera torch mode.

 @discussion Set the camera torch mode.

 @param camera
 Camera device.

 @param mode
 Torch mode.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceSetTorchMode (
    VKTCameraDevice* camera,
    VKTTorchMode mode
);

/*!
 @function VKTCameraDeviceGetWhiteBalanceMode

 @abstract Get the camera white balance mode.

 @discussion Get the camera white balance mode.

 @param camera
 Camera device.

 @param mode
 White balance mode.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceGetWhiteBalanceMode (
    VKTCameraDevice* camera,
    VKTWhiteBalanceMode* mode
);

/*!
 @function VKTCameraDeviceSetWhiteBalanceMode

 @abstract Set the camera white balance mode.

 @discussion Set the camera white balance mode.

 @param camera
 Camera device.

 @param mode
 White balance mode.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceSetWhiteBalanceMode (
    VKTCameraDevice* camera,
    VKTWhiteBalanceMode mode
);

/*!
 @function VKTCameraDeviceGetVideoStabilizationMode

 @abstract Get the camera video stabilization mode.

 @discussion Get the camera video stabilization mode.

 @param camera
 Camera device.

 @param mode
 Video stabilization mode.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceGetVideoStabilizationMode (
    VKTCameraDevice* camera,
    VKTVideoStabilizationMode* mode
);

/*!
 @function VKTCameraDeviceSetVideoStabilizationMode

 @abstract Set the camera video stabilization mode.

 @discussion Set the camera video stabilization mode.

 @param camera
 Camera device.

 @param mode
 Video stabilization mode.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceSetVideoStabilizationMode (
    VKTCameraDevice* camera,
    VKTVideoStabilizationMode mode
);

/*!
 @function VKTCameraDeviceGetZoomRatio

 @abstract Get the camera zoom ratio.

 @discussion Get the camera zoom ratio.
 This value will always be within the minimum and maximum zoom values reported by the camera device.

 @param camera
 Camera device.

 @param ratio
 Zoom ratio.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceGetZoomRatio (
    VKTCameraDevice* camera,
    float* ratio
);

/*!
 @function VKTCameraDeviceSetZoomRatio

 @abstract Set the camera zoom ratio.

 @discussion Set the camera zoom ratio.
 This value must always be within the minimum and maximum zoom values reported by the camera device.

 @param camera
 Camera device.

 @param ratio
 Zoom ratio.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceSetZoomRatio (
    VKTCameraDevice* camera,
    float ratio
);

/*!
 @function VKTCameraDeviceCapturePhoto

 @abstract Capture a still photo.

 @discussion Capture a still photo.

 @param camera
 Camera device.

 @param handler
 Photo handler.

 @param context
 User-provided context.
 */
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTCameraDeviceCapturePhoto (
    VKTCameraDevice* camera,
    VKTSampleBufferHandler handler,
    void* context
);
#pragma endregion
