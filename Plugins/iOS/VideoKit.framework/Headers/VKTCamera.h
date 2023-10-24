//
//  VKTCamera.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 5/15/2023.
//  Copyright © 2023 NatML Inc. All rights reserved.
//

#pragma once

#include <VideoKit/VKTDevice.h>

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
 @enum VKTImageFormat

 @abstract Camera image format.

 @constant VKT_IMAGE_FORMAT_UNKNOWN
 Unknown or invalid format.

 @constant VKT_IMAGE_FORMAT_YCbCr420
 YUV semi-planar format.

 @constant VKT_IMAGE_FORMAT_RGBA8888
 RGBA8888 interleaved format.

 @constant VKT_IMAGE_FORMAT_BGRA8888
 BGRA8888 interleaved format.
 */
enum VKTImageFormat {
    VKT_IMAGE_FORMAT_UNKNOWN     = 0,
    VKT_IMAGE_FORMAT_YCbCr420    = 1,
    VKT_IMAGE_FORMAT_RGBA8888    = 2,
    VKT_IMAGE_FORMAT_BGRA8888    = 3,
};
typedef enum VKTImageFormat VKTImageFormat;

/*!
 @enum VKTImageOrientation
 
 @abstract Camera device frame orientation.
 
 @constant VKT_ORIENTATION_LAVKTSCAPE_LEFT
 Landscape left.
 
 @constant VKT_ORIENTATION_PORTRAIT
 Portrait.
 
 @constant VKT_ORIENTATION_LAVKTSCAPE_RIGHT
 Landscape right.
 
 @constant VKT_ORIENTATION_PORTRAIT_UPSIDE_DOWN
 Portrait upside down.
*/
enum VKTImageOrientation {
    VKT_ORIENTATION_LANDSCAPE_LEFT       = 3,
    VKT_ORIENTATION_PORTRAIT             = 1,
    VKT_ORIENTATION_LANDSCAPE_RIGHT      = 4,
    VKT_ORIENTATION_PORTRAIT_UPSIDE_DOWN = 2
};
typedef enum VKTImageOrientation VKTImageOrientation;

/*!
 @enum VKTMetadata

 @abstract Sample buffer metadata key.

 @constant VKT_METADATA_INTRINSIC_MATRIX
 Camera intrinsic matrix. Value array must have enough capacity for 9 float values.

 @constant VKT_METADATA_EXPOSURE_BIAS
 Camera image exposure bias value in EV.

 @constant VKT_METADATA_EXPOSURE_DURATION
 Camera image exposure duration in seconds.

 @constant VKT_METADATA_FOCAL_LENGTH
 Camera image focal length.

 @constant VKT_METADATA_F_NUMBER
 Camera image aperture F-number.

 @constant VKT_METADATA_BRIGHTNESS
 Camera image ambient brightness.
*/
enum VKTMetadata {
    VKT_METADATA_INTRINSIC_MATRIX   = 1,
    VKT_METADATA_EXPOSURE_BIAS      = 2,
    VKT_METADATA_EXPOSURE_DURATION  = 3,
    VKT_METADATA_FOCAL_LENGTH       = 4,
    VKT_METADATA_F_NUMBER           = 5,
    VKT_METADATA_BRIGHTNESS         = 6,
    VKT_METADATA_ISO                = 7,
};
typedef enum VKTMetadata VKTMetadata;

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
 @typedef VKTCamera
 
 @abstract Camera device.

 @discussion Camera device.
*/
typedef VKTDevice VKTCamera;

/*!
 @typedef VKTCameraImage
 
 @abstract Camera image.

 @discussion Camera image.
*/
typedef VKTSampleBuffer VKTCameraImage;
#pragma endregion


#pragma region --CameraDevice--
/*!
 @function VKTDiscoverCameras

 @abstract Discover available camera devices.

 @discussion Discover available camera devices.

 @param handler
 Device handler. MUST NOT be `NULL`.

 @param context
 Handler context.

 @returns Status.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTDiscoverCameras (
    VKTDeviceDiscoveryHandler handler,
    void* context
);

/*!
 @function VKTCameraGetFieldOfView

 @abstract Camera field of view in degrees.

 @discussion Camera field of view in degrees.

 @param camera
 Camera device.

 @param outWidth
 Output FOV width in degrees.

 @param outHeight
 Output FOV height in degrees.
*/
VKT_BRIDGE VKT_EXPORT void VKT_API VKTCameraGetFieldOfView (
    VKTCamera* camera,
    float* outWidth,
    float* outHeight
);

/*!
 @function VKTCameraGetExposureBiasRange

 @abstract Camera exposure bias range in EV.

 @discussion Camera exposure bias range in EV.

 @param camera
 Camera device.

 @param outMin
 Output minimum exposure bias.

 @param outMax
 Output maximum exposure bias.
*/
VKT_BRIDGE VKT_EXPORT void VKT_API VKTCameraGetExposureBiasRange (
    VKTCamera* camera,
    float* outMin,
    float* outMax
);

/*!
 @function VKTCameraGetExposureDurationRange

 @abstract Camera exposure duration range in seconds.

 @discussion Camera exposure duration range in seconds.

 @param camera
 Camera device.

 @param outMin
 Output minimum exposure duration in seconds.

 @param outMax
 Output maximum exposure duration in seconds.
*/
VKT_BRIDGE VKT_EXPORT void VKT_API VKTCameraGetExposureDurationRange (
    VKTCamera* camera,
    float* outMin,
    float* outMax
);

/*!
 @function VKTCameraGetISORange

 @abstract Camera sensor sensitivity range.

 @discussion Camera sensor sensitivity range.

 @param camera
 Camera device.

 @param outMin
 Output minimum ISO sensitivity value.

 @param outMax
 Output maximum ISO sensitivity value.
*/
VKT_BRIDGE VKT_EXPORT void VKT_API VKTCameraGetISORange (
    VKTCamera* camera,
    float* outMin,
    float* outMax
);

/*!
 @function VKTCameraGetZoomRange

 @abstract Camera optical zoom range.

 @discussion Camera optical zoom range.

 @param camera
 Camera device.

 @param outMin
 Output minimum zoom ratio.

 @param outMax
 Output maximum zoom ratio.
*/
VKT_BRIDGE VKT_EXPORT void VKT_API VKTCameraGetZoomRange (
    VKTCamera* camera, 
    float* outMin, 
    float* outMax
);

/*!
 @function VKTCameraGetPreviewResolution

 @abstract Get the camera preview resolution.

 @discussion Get the camera preview resolution.

 @param camera
 Camera device.

 @param outWidth
 Output width in pixels.

 @param outHeight
 Output height in pixels.
*/
VKT_BRIDGE VKT_EXPORT void VKT_API VKTCameraGetPreviewResolution (
    VKTCamera* camera,
    int32_t* outWidth,
    int32_t* outHeight
);

/*!
 @function VKTCameraSetPreviewResolution

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
VKT_BRIDGE VKT_EXPORT void VKT_API VKTCameraSetPreviewResolution (
    VKTCamera* camera,
    int32_t width,
    int32_t height
);

/*!
 @function VKTCameraGetPhotoResolution

 @abstract Get the camera photo resolution.

 @discussion Get the camera photo resolution.

 @param camera
 Camera device.

 @param outWidth
 Output width in pixels.

 @param outHeight
 Output height in pixels.
*/
VKT_BRIDGE VKT_EXPORT void VKT_API VKTCameraGetPhotoResolution (
    VKTCamera* camera,
    int32_t* outWidth,
    int32_t* outHeight
);

/*!
 @function VKTCameraSetPhotoResolution

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
VKT_BRIDGE VKT_EXPORT void VKT_API VKTCameraSetPhotoResolution (
    VKTCamera* camera,
    int32_t width,
    int32_t height
);

/*!
 @function VKTCameraGetFrameRate

 @abstract Get the camera preview frame rate.

 @discussion Get the camera preview frame rate.

 @param camera
 Camera device.

 @returns Camera preview frame rate.
*/
VKT_BRIDGE VKT_EXPORT int32_t VKT_API VKTCameraGetFrameRate (VKTCamera* camera);

/*!
 @function VKTCameraSetFrameRate

 @abstract Set the camera preview frame rate.

 @discussion Set the camera preview frame rate.

 Note that this method should only be called before the camera preview is started.

 @param camera
 Camera device.

 @param frameRate
 Frame rate to set.
*/
VKT_BRIDGE VKT_EXPORT void VKT_API VKTCameraSetFrameRate (
    VKTCamera* camera,
    int32_t frameRate
);

/*!
 @function VKTCameraGetExposureMode

 @abstract Get the camera exposure mode.

 @discussion Get the camera exposure mode.

 @param camera
 Camera device.

 @returns Exposure mode.
*/
VKT_BRIDGE VKT_EXPORT VKTExposureMode VKT_API VKTCameraGetExposureMode (VKTCamera* camera);

/*!
 @function VKTCameraSetExposureMode

 @abstract Set the camera exposure mode.

 @discussion Set the camera exposure mode.

 @param camera
 Camera device.

 @param mode
 Exposure mode.
 */
VKT_BRIDGE VKT_EXPORT void VKT_API VKTCameraSetExposureMode (
    VKTCamera* camera,
    VKTExposureMode mode
);

/*!
 @function VKTCameraGetExposureBias

 @abstract Get the camera exposure bias.

 @discussion Get the camera exposure bias.

 @param camera
 Camera device.

 @returns Camera exposure bias.
 */
VKT_BRIDGE VKT_EXPORT float VKT_API VKTCameraGetExposureBias (VKTCamera* camera);

/*!
 @function VKTCameraSetExposureBias

 @abstract Set the camera exposure bias.

 @discussion Set the camera exposure bias.

 Note that the value MUST be in the camera exposure range.

 @param camera
 Camera device.

 @param bias
 Exposure bias value to set.
 */
VKT_BRIDGE VKT_EXPORT void VKT_API VKTCameraSetExposureBias (
    VKTCamera* camera,
    float bias
);

/*!
 @function VKTCameraGetExposureDuration

 @abstract Get the camera exposure duration.

 @discussion Get the camera exposure duration.

 @param camera
 Camera device.

 @returns Camera exposure duration in seconds.
 */
VKT_BRIDGE VKT_EXPORT float VKT_API VKTCameraGetExposureDuration (VKTCamera* camera);

/*!
 @function VKTCameraGetISO

 @abstract Get the camera sensitivity.

 @discussion Get the camera sensitivity.

 @param camera
 Camera device.

 @returns Camera sensitivity.
 */
VKT_BRIDGE VKT_EXPORT float VKT_API VKTCameraGetISO (VKTCamera* camera);

/*!
 @function VKTCameraSetExposureDuration

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
VKT_BRIDGE VKT_EXPORT void VKT_API VKTCameraSetExposureDuration (
    VKTCamera* camera,
    float duration,
    float ISO
);

/*!
 @function VKTCameraSetExposurePoint

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
VKT_BRIDGE VKT_EXPORT void VKT_API VKTCameraSetExposurePoint (
    VKTCamera* camera,
    float x,
    float y
);

/*!
 @function VKTCameraGetFlashMode

 @abstract Get the camera photo flash mode.

 @discussion Get the camera photo flash mode.

 @param camera
 Camera device.

 @returns Camera photo flash mode.
 */
VKT_BRIDGE VKT_EXPORT VKTFlashMode VKT_API VKTCameraGetFlashMode (VKTCamera* camera);

/*!
 @function VKTCameraSetFlashMode

 @abstract Set the camera photo flash mode.

 @discussion Set the camera photo flash mode.

 @param camera
 Camera device.

 @param mode
 Flash mode to set.
 */
VKT_BRIDGE VKT_EXPORT void VKT_API VKTCameraSetFlashMode (
    VKTCamera* camera,
    VKTFlashMode mode
);

/*!
 @function VKTCameraGetFocusMode

 @abstract Get the camera focus mode.

 @discussion Get the camera focus mode.

 @param camera
 Camera device.

 @returns Camera focus mode.
 */
VKT_BRIDGE VKT_EXPORT VKTFocusMode VKT_API VKTCameraGetFocusMode (VKTCamera* camera);

/*!
 @function VKTCameraSetFocusMode

 @abstract Set the camera focus mode.

 @discussion Set the camera focus mode.

 @param camera
 Camera device.

 @param mode
 Focus mode.
 */
VKT_BRIDGE VKT_EXPORT void VKT_API VKTCameraSetFocusMode (
    VKTCamera* camera,
    VKTFocusMode mode
);

/*!
 @function VKTCameraSetFocusPoint

 @abstract Set the camera focus point.

 @discussion Set the camera focus point of interest.
 The coordinates are specified in viewport space, with each value in range [0., 1.].
 This function should only be used if the camera supports setting the focus point.
 See `VKTCameraFocusPointSupported`.

 @param camera
 Camera device.

 @param x
 Focus point x-coordinate in viewport space.

 @param y
 Focus point y-coordinate in viewport space.
 */
VKT_BRIDGE VKT_EXPORT void VKT_API VKTCameraSetFocusPoint (
    VKTCamera* camera,
    float x,
    float y
);

/*!
 @function VKTCameraGetTorchMode

 @abstract Get the current camera torch mode.

 @discussion Get the current camera torch mode.

 @param camera
 Camera device.

 @returns Current camera torch mode.
 */
VKT_BRIDGE VKT_EXPORT VKTTorchMode VKT_API VKTCameraGetTorchMode (VKTCamera* camera);

/*!
 @function VKTCameraSetTorchMode

 @abstract Set the camera torch mode.

 @discussion Set the camera torch mode.

 @param camera
 Camera device.

 @param mode
 Torch mode.
 */
VKT_BRIDGE VKT_EXPORT void VKT_API VKTCameraSetTorchMode (
    VKTCamera* camera,
    VKTTorchMode mode
);

/*!
 @function VKTCameraGetWhiteBalanceMode

 @abstract Get the camera white balance mode.

 @discussion Get the camera white balance mode.

 @param camera
 Camera device.

 @returns White balance mode.
 */
VKT_BRIDGE VKT_EXPORT VKTWhiteBalanceMode VKT_API VKTCameraGetWhiteBalanceMode (VKTCamera* camera);

/*!
 @function VKTCameraSetWhiteBalanceMode

 @abstract Set the camera white balance mode.

 @discussion Set the camera white balance mode.

 @param camera
 Camera device.

 @param mode
 White balance mode.
 */
VKT_BRIDGE VKT_EXPORT void VKT_API VKTCameraSetWhiteBalanceMode (
    VKTCamera* camera,
    VKTWhiteBalanceMode mode
);

/*!
 @function VKTCameraGetVideoStabilizationMode

 @abstract Get the camera video stabilization mode.

 @discussion Get the camera video stabilization mode.

 @param camera
 Camera device.

 @returns Video stabilization mode.
 */
VKT_BRIDGE VKT_EXPORT VKTVideoStabilizationMode VKT_API VKTCameraGetVideoStabilizationMode (VKTCamera* camera);

/*!
 @function VKTCameraSetVideoStabilizationMode

 @abstract Set the camera video stabilization mode.

 @discussion Set the camera video stabilization mode.

 @param camera
 Camera device.

 @param mode
 Video stabilization mode.
 */
VKT_BRIDGE VKT_EXPORT void VKT_API VKTCameraSetVideoStabilizationMode (
    VKTCamera* camera,
    VKTVideoStabilizationMode mode
);

/*!
 @function VKTCameraGetZoomRatio

 @abstract Get the camera zoom ratio.

 @discussion Get the camera zoom ratio.
 This value will always be within the minimum and maximum zoom values reported by the camera device.

 @param camera
 Camera device.

 @returns Zoom ratio.
 */
VKT_BRIDGE VKT_EXPORT float VKT_API VKTCameraGetZoomRatio (VKTCamera* camera);

/*!
 @function VKTCameraSetZoomRatio

 @abstract Set the camera zoom ratio.

 @discussion Set the camera zoom ratio.
 This value must always be within the minimum and maximum zoom values reported by the camera device.

 @param camera
 Camera device.

 @param ratio
 Zoom ratio.
 */
VKT_BRIDGE VKT_EXPORT void VKT_API VKTCameraSetZoomRatio (
    VKTCamera* camera,
    float ratio
);

/*!
 @function VKTCameraCapturePhoto

 @abstract Capture a still photo.

 @discussion Capture a still photo.

 @param camera
 Camera device.

 @param handler
 Photo handler.

 @param context
 User-provided context.
 */
VKT_BRIDGE VKT_EXPORT void VKT_API VKTCameraCapturePhoto (
    VKTCamera* camera,
    VKTSampleBufferHandler handler,
    void* context
);
#pragma endregion


#pragma region --CameraImage--
/*!
 @function VKTCameraImageGetData

 @abstract Get the image data of a camera image.

 @discussion Get the image data of a camera image.
 If the camera image uses a planar format, this will return `NULL`.

 @param cameraImage
 Camera image.
*/
VKT_BRIDGE VKT_EXPORT void* VKT_API VKTCameraImageGetData (VKTCameraImage* cameraImage);

/*!
 @function VKTCameraImageGetDataSize

 @abstract Get the image data size of a camera image in bytes.

 @discussion Get the image data size of a camera image in bytes.

 @param cameraImage
 Camera image.
*/
VKT_BRIDGE VKT_EXPORT int32_t VKT_API VKTCameraImageGetDataSize (VKTCameraImage* cameraImage);

/*!
 @function VKTCameraImageGetFormat

 @abstract Get the format of a camera image.

 @discussion Get the format of a camera image.

 @param cameraImage
 Camera image.
*/
VKT_BRIDGE VKT_EXPORT VKTImageFormat VKT_API VKTCameraImageGetFormat (VKTCameraImage* cameraImage);

/*!
 @function VKTCameraImageGetWidth

 @abstract Get the width of a camera image.

 @discussion Get the width of a camera image.

 @param cameraImage
 Camera image.
*/
VKT_BRIDGE VKT_EXPORT int32_t VKT_API VKTCameraImageGetWidth (VKTCameraImage* cameraImage);

/*!
 @function VKTCameraImageGetHeight

 @abstract Get the height of a camera image.

 @discussion Get the height of a camera image.

 @param cameraImage
 Camera image.
*/
VKT_BRIDGE VKT_EXPORT int32_t VKT_API VKTCameraImageGetHeight (VKTCameraImage* cameraImage);

/*!
 @function VKTCameraImageGetRowStride

 @abstract Get the row stride of a camera image in bytes.

 @discussion Get the row stride of a camera image in bytes.

 @param cameraImage
 Camera image.

 @returns Row stride in bytes.
*/
VKT_BRIDGE VKT_EXPORT int32_t VKT_API VKTCameraImageGetRowStride (VKTCameraImage* cameraImage);

/*!
 @function VKTCameraImageGetTimestamp

 @abstract Get the timestamp of a camera image.

 @discussion Get the timestamp of a camera image.

 @param cameraImage
 Camera image.

 @returns Image timestamp in nanoseconds.
*/
VKT_BRIDGE VKT_EXPORT int64_t VKT_API VKTCameraImageGetTimestamp (VKTCameraImage* cameraImage);

/*!
 @function VKTCameraImageGetVerticallyMirrored

 @abstract Whether the camera image is vertically mirrored.

 @discussion Whether the camera image is vertically mirrored.

 @param cameraImage
 Camera image.
*/
VKT_BRIDGE VKT_EXPORT bool VKT_API VKTCameraImageGetVerticallyMirrored (VKTCameraImage* cameraImage);

/*!
 @function VKTCameraImageGetPlaneCount

 @abstract Get the plane count of a camera image.

 @discussion Get the plane count of a camera image.
 If the image uses an interleaved format or only has a single plane, this function returns zero.

 @param cameraImage
 Camera image.

 @returns Number of planes in image.
*/
VKT_BRIDGE VKT_EXPORT int32_t VKT_API VKTCameraImageGetPlaneCount (VKTCameraImage* cameraImage);

/*!
 @function VKTCameraImageGetPlaneData

 @abstract Get the plane data for a given plane of a camera image.

 @discussion Get the plane data for a given plane of a camera image.

 @param cameraImage
 Camera image.
 
 @param planeIdx
 Plane index.
*/
VKT_BRIDGE VKT_EXPORT void* VKT_API VKTCameraImageGetPlaneData (
    VKTCameraImage* cameraImage,
    int32_t planeIdx
);

/*!
 @function VKTCameraImageGetPlaneDataSize

 @abstract Get the plane data size of a given plane of a camera image.

 @discussion Get the plane data size of a given plane of a camera image.

 @param cameraImage
 Camera image.
 
 @param planeIdx
 Plane index.
*/
VKT_BRIDGE VKT_EXPORT int32_t VKT_API VKTCameraImageGetPlaneDataSize (
    VKTCameraImage* cameraImage,
    int32_t planeIdx
);

/*!
 @function VKTCameraImageGetPlaneWidth

 @abstract Get the width of a given plane of a camera image.

 @discussion Get the width of a given plane of a camera image.

 @param cameraImage
 Camera image.
 
 @param planeIdx
 Plane index.
*/
VKT_BRIDGE VKT_EXPORT int32_t VKT_API VKTCameraImageGetPlaneWidth (
    VKTCameraImage* cameraImage,
    int32_t planeIdx
);

/*!
 @function VKTCameraImageGetPlaneHeight

 @abstract Get the height of a given plane of a camera image.

 @discussion Get the height of a given plane of a camera image.

 @param cameraImage
 Camera image.
 
 @param planeIdx
 Plane index.
*/
VKT_BRIDGE VKT_EXPORT int32_t VKT_API VKTCameraImageGetPlaneHeight (
    VKTCameraImage* cameraImage,
    int32_t planeIdx
);

/*!
 @function VKTCameraImageGetPlanePixelStride

 @abstract Get the plane pixel stride for a given plane of a camera image.

 @discussion Get the plane pixel stride for a given plane of a camera image.

 @param cameraImage
 Camera image.
 
 @param planeIdx
 Plane index.

 @returns Plane pixel stride in bytes.
*/
VKT_BRIDGE VKT_EXPORT int32_t VKT_API VKTCameraImageGetPlanePixelStride (
    VKTCameraImage* cameraImage,
    int32_t planeIdx
);

/*!
 @function VKTCameraImageGetPlaneRowStride

 @abstract Get the plane row stride for a given plane of a camera image.

 @discussion Get the plane row stride for a given plane of a camera image.

 @param cameraImage
 Camera image.
 
 @param planeIdx
 Plane index.

 @returns Plane row stride in bytes.
*/
VKT_BRIDGE VKT_EXPORT int32_t VKT_API VKTCameraImageGetPlaneRowStride (
    VKTCameraImage* cameraImage,
    int32_t planeIdx
);

/*!
 @function VKTCameraImageGetMetadata

 @abstract Get the metadata value for a given key in a camera image.

 @discussion Get the metadata value for a given key in a camera image.

 @param cameraImage
 Camera image.
 
 @param key
 Metadata key.

 @param value
 Destination value array.

 @param count
 Destination value array size.

 @returns Whether the metadata key was successfully looked up.
*/
VKT_BRIDGE VKT_EXPORT bool VKT_API VKTCameraImageGetMetadata (
    VKTCameraImage* cameraImage,
    VKTMetadata key,
    float* value,
    int32_t count
);

/*!
 @function VKTCameraImageConvertToRGBA8888
 
 @abstract Convert a camera image to an RGBA8888 pixel buffer.
 
 @discussion Convert a camera image to an RGBA8888 pixel buffer.
 
 @param cameraImage
 Camera image.

 @param orientation
 Desired image orientation.

 @param mirror
 Whether to vertically mirror the pixel buffer.

 @param tempBuffer
 Temporary pixel buffer for intermediate conversions. Pass `NULL` to use short-time allocations.

 @param dstBuffer
 Destination pixel buffer. This must be at least `width * height * 4` bytes large.

 @param dstWidth
 Output pixel buffer width.

 @param dstHeight
 Output pixel buffer height.
*/
VKT_BRIDGE VKT_EXPORT void VKT_API VKTCameraImageConvertToRGBA8888 (
    VKTCameraImage* cameraImage,
    VKTImageOrientation orientation,
    bool mirror,
    uint8_t* tempBuffer,
    uint8_t* dstBuffer,
    int32_t* dstWidth,
    int32_t* dstHeight
);
#pragma endregion
