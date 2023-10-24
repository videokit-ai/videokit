/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Internal {

    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using UnityEngine;
    using Devices;

    public static class VideoKit {

        public const string Assembly =
        #if (UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR
        @"__Internal";
        #else
        @"VideoKit";
        #endif


        #region --Enumerations--
        public enum AssetType : int {
            Unknown = 0,
            Image = 1,
            Audio = 2,
            Video = 3,
        }

        [Flags]
        public enum DeviceFlags : int { 
            // MediaDevice
            Internal                = 1 << 0,
            External                = 1 << 1,
            Default                 = 1 << 3,
            // AudioDevice
            EchoCancellation        = 1 << 2,
            // CameraDevice
            FrontFacing             = 1 << 6,
            Flash                   = 1 << 7,
            Torch                   = 1 << 8,
            Depth                   = 1 << 15,
            // CameraDevice.Exposure
            ExposureContinuous      = 1 << 16,
            ExposureLock            = 1 << 11,
            ExposureManual          = 1 << 14,
            ExposurePoint           = 1 << 9,
            // CameraDevice.Focus
            FocusContinuous         = 1 << 17,
            FocusLock               = 1 << 12,
            FocusPoint              = 1 << 10,
            // CameraDevice.WhiteBalance
            WhiteBalanceContinuous  = 1 << 18,
            WhiteBalanceLock        = 1 << 13,
            // CameraDevice.VideoStabilization
            VideoStabilization      = 1 << 19,
        }

        public enum Metadata : int {
            IntrinsicMatrix     = 1,
            ExposureBias        = 2,
            ExposureDuration    = 3,
            FocalLength         = 4,
            FNumber             = 5,
            Brightness          = 6,
            ISO                 = 7,
        }

        public enum PermissionType : int {
            Microphone  = 1,
            Camera      = 2
        }
    
        public enum Status : int {
            Ok                  = 0,
            InvalidArgument     = 1,
            InvalidOperation    = 2,
            NotImplemented      = 3,
            InvalidSession      = 101,
            InvalidPlan         = 104,
            LimitedPlan         = 105,
        }
        #endregion


        #region --Delegates--
        public delegate void RecordingHandler (IntPtr context, IntPtr path);
        public delegate void DeviceDiscoveryHandler (IntPtr context, IntPtr devices, int count);
        public delegate void SampleBufferHandler (IntPtr context, IntPtr sampleBuffer);
        public delegate void DeviceDisconnectHandler (IntPtr context);
        public delegate void PermissionResultHandler (IntPtr context, MediaDevice.PermissionStatus result);
        public delegate void AssetLoadHandler (IntPtr context, IntPtr path, AssetType type, int width, int height, float frameRate, int sampleRate, int channelCount, float duration);
        public delegate void AssetShareHandler (IntPtr context, IntPtr receiver);
        #endregion


        #region --VKTSession--
        [DllImport(Assembly, EntryPoint = @"VKTGetBundleIdentifier")]
        public static extern Status BundleIdentifier (
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder dest
        );

        [DllImport(Assembly, EntryPoint = @"VKTGetSessionStatus")]
        public static extern Status SessionStatus ();

        [DllImport(Assembly, EntryPoint = @"VKTSetSessionToken")]
        public static extern Status SetSessionToken (
            [MarshalAs(UnmanagedType.LPUTF8Str)] string? token
        );
        #endregion


        #region --VKTAsset--
        [DllImport(Assembly, EntryPoint = @"VKTAssetLoad")]
        public static extern Status LoadAsset (
            [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
            AssetLoadHandler handler,
            IntPtr context
        );

        [DllImport(Assembly, EntryPoint = @"VKTAssetLoadFromCameraRoll")]
        public static extern Status LoadAssetFromCameraRoll (
            AssetType type,
            AssetLoadHandler handler,
            IntPtr context
        );

        [DllImport(Assembly, EntryPoint = @"VKTAssetShare")]
        public static extern Status ShareAsset (
            [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string message,
            AssetShareHandler handler,
            IntPtr context
        );

        [DllImport(Assembly, EntryPoint = @"VKTAssetSaveToCameraRoll")]
        public static extern Status SaveAssetToCameraRoll (
            [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string album,
            AssetShareHandler handler,
            IntPtr context
        );
        #endregion


        #region --VKTRecorder--
        [DllImport(Assembly, EntryPoint = @"VKTRecorderGetFrameSize")]
        public static extern Status FrameSize (
            this IntPtr recorder,
            out int width,
            out int height
        );

        [DllImport(Assembly, EntryPoint = @"VKTRecorderCommitFrame")]
        public static extern unsafe Status CommitFrame (
            this IntPtr recorder,
            void* pixelBuffer,
            long timestamp
        );

        [DllImport(Assembly, EntryPoint = @"VKTRecorderCommitSamples")]
        public static extern unsafe Status CommitSamples (
            this IntPtr recorder,
            float* sampleBuffer,
            int sampleCount,
            long timestamp
        );

        [DllImport(Assembly, EntryPoint = @"VKTRecorderFinishWriting")]
        public static extern Status FinishWriting (
            this IntPtr recorder,
            RecordingHandler handler,
            IntPtr context
        );

        [DllImport(Assembly, EntryPoint = @"VKTRecorderCreateMP4")]
        public static extern Status CreateMP4Recorder (
            [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
            int width,
            int height,
            float frameRate,
            int sampleRate,
            int channelCount,
            int videoBitrate,
            int keyframeInterval,
            int audioBitRate,
            out IntPtr recorder
        );

        [DllImport(Assembly, EntryPoint = @"VKTRecorderCreateHEVC")]
        public static extern Status CreateHEVCRecorder (
            [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
            int width,
            int height,
            float frameRate,
            int sampleRate,
            int channelCount,
            int videoBitRate,
            int keyframeInterval,
            int audioBitRate,
            out IntPtr recorder
        );

        [DllImport(Assembly, EntryPoint = @"VKTRecorderCreateGIF")]
        public static extern Status CreateGIFRecorder (
            [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
            int width,
            int height,
            float delay,
            out IntPtr recorder
        );

        [DllImport(Assembly, EntryPoint = @"VKTRecorderCreateWAV")]
        public static extern Status CreateWAVRecorder (
            [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
            int sampleRate,
            int channelCount,
            out IntPtr recorder
        );

        [DllImport(Assembly, EntryPoint = @"VKTRecorderCreateWEBM")]
        public static extern Status CreateWEBMRecorder (
            [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
            int width,
            int height,
            float frameRate,
            int sampleRate,
            int channelCount,
            int videoBitRate,
            int keyframeInterval,
            int audioBitRate,
            out IntPtr recorder
        );

        [DllImport(Assembly, EntryPoint = @"VKTRecorderCreateJPEG")]
        public static extern Status CreateJPEGRecorder (
            [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
            int width,
            int height,
            float quality,
            out IntPtr recorder
        );
        #endregion


        #region --VKTDevice--
        [DllImport(Assembly, EntryPoint = @"VKTDeviceRelease")]
        public static extern Status ReleaseDevice (this IntPtr device);

        [DllImport(Assembly, EntryPoint = @"VKTDeviceGetUniqueID")]
        public static extern Status UniqueID (
            this IntPtr device,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder dest
        );

        [DllImport(Assembly, EntryPoint = @"VKTDeviceGetName")]
        public static extern Status Name (
            this IntPtr device,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder dest
        );

        [DllImport(Assembly, EntryPoint = @"VKTDeviceGetFlags")]
        public static extern DeviceFlags Flags (this IntPtr device);

        [DllImport(Assembly, EntryPoint = @"VKTDeviceIsRunning")]
        public static extern bool Running (this IntPtr device);

        [DllImport(Assembly, EntryPoint = @"VKTDeviceStartRunning")]
        public static extern Status StartRunning (
            this IntPtr device,
            SampleBufferHandler callback,
            IntPtr context
        );

        [DllImport(Assembly, EntryPoint = @"VKTDeviceStopRunning")]
        public static extern Status StopRunning (this IntPtr device);

        [DllImport(Assembly, EntryPoint = @"VKTDeviceSetDisconnectHandler")]
        public static extern Status SetDisconnectHandler (
            this IntPtr device,
            DeviceDisconnectHandler handler,
            IntPtr context
        );

        [DllImport(Assembly, EntryPoint = @"VKTDeviceCheckPermissions")]
        public static extern Status CheckPermissions (
            PermissionType type,
            bool request,
            PermissionResultHandler handler,
            IntPtr context
        );
        #endregion


        #region --VKTMicrophone--
        [DllImport(Assembly, EntryPoint = @"VKTDiscoverMicrophones")]
        public static extern Status DiscoverMicrophones (
            DeviceDiscoveryHandler handler,
            IntPtr context
        );

        [DllImport(Assembly, EntryPoint = @"VKTMicrophoneGetEchoCancellation")]
        public static extern bool EchoCancellation (this IntPtr microphone);

        [DllImport(Assembly, EntryPoint = @"VKTMicrophoneSetEchoCancellation")]
        public static extern void SetEchoCancellation (
            this IntPtr microphone,
            bool mode
        );

        [DllImport(Assembly, EntryPoint = @"VKTMicrophoneGetSampleRate")]
        public static extern int SampleRate (this IntPtr microphone);

        [DllImport(Assembly, EntryPoint = @"VKTMicrophoneSetSampleRate")]
        public static extern void SetSampleRate (
            this IntPtr microphone,
            int sampleRate
        );

        [DllImport(Assembly, EntryPoint = @"VKTMicrophoneGetChannelCount")]
        public static extern int ChannelCount (this IntPtr microphone);

        [DllImport(Assembly, EntryPoint = @"VKTMicrophoneSetChannelCount")]
        public static extern void SetChannelCount (
            this IntPtr microphone,
            int sampleRate
        );

        [DllImport(Assembly, EntryPoint = @"VKTAudioBufferGetData")]
        public static unsafe extern float* AudioBufferData (this IntPtr audioBuffer);

        [DllImport(Assembly, EntryPoint = @"VKTAudioBufferGetSampleCount")]
        public static extern int AudioBufferSampleCount (this IntPtr audioBuffer);

        [DllImport(Assembly, EntryPoint = @"VKTAudioBufferGetSampleRate")]
        public static extern int AudioBufferSampleRate (this IntPtr audioBuffer);

        [DllImport(Assembly, EntryPoint = @"VKTAudioBufferGetChannelCount")]
        public static extern int AudioBufferChannelCount (this IntPtr audioBuffer);

        [DllImport(Assembly, EntryPoint = @"VKTAudioBufferGetTimestamp")]
        public static extern long AudioBufferTimestamp (this IntPtr audioBuffer);
        #endregion


        #region --VKTCamera--
        [DllImport(Assembly, EntryPoint = @"VKTDiscoverCameras")]
        public static extern Status DiscoverCameras (
            DeviceDiscoveryHandler handler,
            IntPtr context
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraGetFieldOfView")]
        public static extern void FieldOfView (
            this IntPtr camera,
            out float x,
            out float y
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraGetExposureBiasRange")]
        public static extern void ExposureBiasRange (
            this IntPtr camera,
            out float min,
            out float max
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraGetExposureDurationRange")]
        public static extern void ExposureDurationRange (
            this IntPtr camera,
            out float min,
            out float max
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraGetISORange")]
        public static extern void ISORange (
            this IntPtr device,
            out float min,
            out float max
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraGetZoomRange")]
        public static extern void ZoomRange (
            this IntPtr camera,
            out float min,
            out float max
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraGetPreviewResolution")]
        public static extern void PreviewResolution (
            this IntPtr camera,
            out int width,
            out int height
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraSetPreviewResolution")]
        public static extern void SetPreviewResolution (
            this IntPtr camera,
            int width,
            int height
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraGetPhotoResolution")]
        public static extern void PhotoResolution (
            this IntPtr camera,
            out int width,
            out int height
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraSetPhotoResolution")]
        public static extern void SetPhotoResolution (
            this IntPtr camera,
            int width,
            int height
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraGetFrameRate")]
        public static extern int FrameRate (this IntPtr camera);

        [DllImport(Assembly, EntryPoint = @"VKTCameraSetFrameRate")]
        public static extern void SetFrameRate (
            this IntPtr camera,
            int framerate
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraGetExposureMode")]
        public static extern CameraDevice.ExposureMode ExposureMode (this IntPtr camera);

        [DllImport(Assembly, EntryPoint = @"VKTCameraSetExposureMode")]
        public static extern void SetExposureMode (
            this IntPtr camera,
            CameraDevice.ExposureMode mode
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraGetExposureBias")]
        public static extern float ExposureBias (this IntPtr camera);

        [DllImport(Assembly, EntryPoint = @"VKTCameraSetExposureBias")]
        public static extern void SetExposureBias (
            this IntPtr camera,
            float bias
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraGetExposureDuration")]
        public static extern float ExposureDuration (this IntPtr camera);

        [DllImport(Assembly, EntryPoint = @"VKTCameraGetISO")]
        public static extern float ISO (this IntPtr camera);

        [DllImport(Assembly, EntryPoint = @"VKTCameraSetExposureDuration")]
        public static extern void SetExposureDuration (
            this IntPtr camera,
            float duration,
            float ISO
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraSetExposurePoint")]
        public static extern void SetExposurePoint (
            this IntPtr camera,
            float x,
            float y
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraGetFlashMode")]
        public static extern CameraDevice.FlashMode FlashMode (this IntPtr camera);

        [DllImport(Assembly, EntryPoint = @"VKTCameraSetFlashMode")]
        public static extern void SetFlashMode (
            this IntPtr camera,
            CameraDevice.FlashMode mode
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraGetFocusMode")]
        public static extern CameraDevice.FocusMode FocusMode (this IntPtr camera);

        [DllImport(Assembly, EntryPoint = @"VKTCameraSetFocusMode")]
        public static extern void SetFocusMode (
            this IntPtr camera,
            CameraDevice.FocusMode mode
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraSetFocusPoint")]
        public static extern void SetFocusPoint (
            this IntPtr camera,
            float x,
            float y
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraGetTorchMode")]
        public static extern CameraDevice.TorchMode TorchMode (this IntPtr camera);

        [DllImport(Assembly, EntryPoint = @"VKTCameraSetTorchMode")]
        public static extern void SetTorchMode (
            this IntPtr camera,
            CameraDevice.TorchMode mode
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraGetWhiteBalanceMode")]
        public static extern CameraDevice.WhiteBalanceMode WhiteBalanceMode (this IntPtr camera);

        [DllImport(Assembly, EntryPoint = @"VKTCameraSetWhiteBalanceMode")]
        public static extern void SetWhiteBalanceMode (
            this IntPtr camera,
            CameraDevice.WhiteBalanceMode mode
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraGetVideoStabilizationMode")]
        public static extern CameraDevice.VideoStabilizationMode VideoStabilizationMode (this IntPtr camera);

        [DllImport(Assembly, EntryPoint = @"VKTCameraSetVideoStabilizationMode")]
        public static extern void SetVideoStabilizationMode (
            this IntPtr camera,
            CameraDevice.VideoStabilizationMode mode
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraGetZoomRatio")]
        public static extern float ZoomRatio (this IntPtr camera);

        [DllImport(Assembly, EntryPoint = @"VKTCameraSetZoomRatio")]
        public static extern void SetZoomRatio (
            this IntPtr camera,
            float ratio
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraCapturePhoto")]
        public static extern void CapturePhoto (
            this IntPtr camera,
            SampleBufferHandler handler,
            IntPtr context
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraImageGetData")]
        public static unsafe extern void* CameraImageData (this IntPtr image);

        [DllImport(Assembly, EntryPoint = @"VKTCameraImageGetDataSize")]
        public static extern int CameraImageDataSize (this IntPtr image);

        [DllImport(Assembly, EntryPoint = @"VKTCameraImageGetFormat")]
        public static extern CameraImage.Format CameraImageFormat (this IntPtr image);

        [DllImport(Assembly, EntryPoint = @"VKTCameraImageGetWidth")]
        public static extern int CameraImageWidth (this IntPtr image);

        [DllImport(Assembly, EntryPoint = @"VKTCameraImageGetHeight")]
        public static extern int CameraImageHeight (this IntPtr image);

        [DllImport(Assembly, EntryPoint = @"VKTCameraImageGetRowStride")]
        public static extern int CameraImageRowStride (this IntPtr image);

        [DllImport(Assembly, EntryPoint = @"VKTCameraImageGetTimestamp")]
        public static extern long CameraImageTimestamp (this IntPtr image);

        [DllImport(Assembly, EntryPoint = @"VKTCameraImageGetVerticallyMirrored")]
        public static extern bool CameraImageVerticallyMirrored (this IntPtr image);

        [DllImport(Assembly, EntryPoint = @"VKTCameraImageGetPlaneCount")]
        public static extern int CameraImagePlaneCount (this IntPtr image);

        [DllImport(Assembly, EntryPoint = @"VKTCameraImageGetPlaneData")]
        public static unsafe extern void* CameraImagePlaneData (
            this IntPtr image,
            int planeIdx
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraImageGetPlaneDataSize")]
        public static extern int CameraImagePlaneDataSize (
            this IntPtr image,
            int planeIdx
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraImageGetPlaneWidth")]
        public static extern int CameraImagePlaneWidth (
            this IntPtr image,
            int planeIdx
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraImageGetPlaneHeight")]
        public static extern int CameraImagePlaneHeight (
            this IntPtr image,
            int planeIdx
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraImageGetPlanePixelStride")]
        public static extern int CameraImagePlanePixelStride (
            this IntPtr image,
            int planeIdx
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraImageGetPlaneRowStride")]
        public static extern int CameraImagePlaneRowStride (
            this IntPtr image,
            int planeIdx
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraImageGetMetadata")]
        public static unsafe extern bool CameraImageMetadata (
            this IntPtr image,
            Metadata key,
            float* value,
            int count = 1
        );

        [DllImport(Assembly, EntryPoint = @"VKTCameraImageConvertToRGBA8888")]
        public static unsafe extern void ConvertToRGBA8888 (
            this IntPtr image,
            int orientation,
            bool mirror,
            void* tempBuffer,
            void* dstBuffer,
            out int dstWidth,
            out int dstHeight
        );
        #endregion


        #region --Utility--

        public static bool IsAppDomainLoaded { // thanks @UnityAlex!
            get {
                #if ENABLE_MONO && UNITY_EDITOR
                var domain = mono_domain_get();
                var unloading = mono_domain_is_unloading(domain);
                return !unloading;
                [DllImport(@"__Internal")]
                static extern IntPtr mono_domain_get ();
                [DllImport(@"__Internal")] [return: MarshalAs(UnmanagedType.I4)]
                static extern bool mono_domain_is_unloading (IntPtr domain);
                #else
                return true;
                #endif
            }
        }

        public static void CheckStatus (this Status status) {
            switch (status) {
                case Status.Ok:                 break;
                case Status.InvalidArgument:    throw new ArgumentException();
                case Status.InvalidOperation:   throw new InvalidOperationException();
                case Status.NotImplemented:     throw new NotImplementedException();
                case Status.InvalidSession:     throw new InvalidOperationException(@"VideoKit session token is invalid. Check your NatML access key.");
                case Status.InvalidPlan:        throw new InvalidOperationException(@"VideoKit plan does not support this operation. Check your plan and upgrade at https://hub.natml.ai");
                case Status.LimitedPlan:        Debug.LogWarning(@"VideoKit plan only allows for limited functionality. Check your plan and upgrade at https://hub.natml.ai"); break;
                default:                        throw new InvalidOperationException();
            }
        }
        #endregion
    }
}