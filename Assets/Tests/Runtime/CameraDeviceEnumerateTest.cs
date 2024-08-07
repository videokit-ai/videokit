/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.Tests {

    using System.Collections.Generic;
    using UnityEngine;
    using Newtonsoft.Json;

    internal sealed class CameraDeviceEnumerateTest : MonoBehaviour {

        private async void Start () {
            await CameraDevice.CheckPermissions();
            var cameraDevices = await CameraDevice.Discover();
            foreach (var device in cameraDevices)
                Debug.Log(JsonConvert.SerializeObject(new Dictionary<string, object> {
                    // Media device
                    ["uniqueID"] = device.uniqueId,
                    ["name"] = device.name,
                    ["location"] = device.location,
                    ["defaultForMediaType"] = device.defaultForMediaType,
                    // Streaming
                    ["previewResolution"] = device.previewResolution,
                    ["photoResolution"] = device.photoResolution,
                    ["frameRate"] = device.frameRate,
                    ["running"] = device.running,                            
                    // Properties
                    ["frontFacing"] = device.frontFacing,
                    ["torchSupported"] = device.torchSupported,
                    ["fieldOfView"] = device.fieldOfView,
                    ["zoomRange"] = device.zoomRange,
                    // Flash
                    ["flashSupported"] = device.flashSupported,
                    ["flashMode"] = device.flashMode,
                    // Exposure
                    ["exposureMode"] = device.exposureMode,
                    ["exposureContinuousSupported"] = device.IsExposureModeSupported(CameraDevice.ExposureMode.Continuous),
                    ["exposureLockSupported"] = device.IsExposureModeSupported(CameraDevice.ExposureMode.Locked),
                    ["exposureManualSupported"] = device.IsExposureModeSupported(CameraDevice.ExposureMode.Manual),
                    ["exposurePointSupported"] = device.exposurePointSupported,
                    ["exposureBiasRange"] = device.exposureBiasRange,
                    ["exposureDurationRange"] = device.exposureDurationRange,
                    ["ISORange"] = device.ISORange,
                    // Focus
                    ["focusMode"] = device.focusMode,
                    ["focusContinuousSupported"] = device.IsFocusModeSupported(CameraDevice.FocusMode.Continuous),
                    ["focusLockSupported"] = device.IsFocusModeSupported(CameraDevice.FocusMode.Locked),
                    ["focusPointSupported"] = device.focusPointSupported,
                    // White balance
                    ["whiteBalanceMode"] = device.whiteBalanceMode,
                    ["whiteBalanceContinuousSupported"] = device.IsWhiteBalanceModeSupported(CameraDevice.WhiteBalanceMode.Continuous),
                    ["whiteBalanceLockSupported"] = device.IsWhiteBalanceModeSupported(CameraDevice.WhiteBalanceMode.Locked),
                    // Video stabilization
                    ["videoStabilizationMode"] = device.videoStabilizationMode,
                    ["videoStabilizationStandardSupported"] = device.IsVideoStabilizationModeSupported(CameraDevice.VideoStabilizationMode.Standard),
                }, Formatting.Indented));
        }
    }
}