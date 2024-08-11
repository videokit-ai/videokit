/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Internal {

    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using Function.Types;

    public static class VideoKit {

        public const string Assembly =
        #if (UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR
        @"__Internal";
        #else
        @"VideoKit";
        #endif


        #region --Enumerations--
        [Flags]
        public enum MediaDeviceFlags : int { 
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

        public enum PermissionType : int {
            Microphone  = 1,
            Camera      = 2
        }

        public enum MetadataType : int {
            Unknown   = 0,
            Float     = 1,
            Int       = 2,
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
        public delegate void SampleBufferHandler (IntPtr context, IntPtr sampleBuffer);
        public delegate void MediaAssetHandler (IntPtr context, IntPtr asset);
        public delegate void MediaAssetShareHandler (IntPtr context, IntPtr receiver);
        public delegate void MediaDeviceDiscoveryHandler (IntPtr context, IntPtr devices, int count);
        public delegate void MediaDeviceDisconnectHandler (IntPtr context, IntPtr device);
        public delegate void MediaDevicePermissionResultHandler (IntPtr context, MediaDevice.PermissionStatus result);
        internal delegate void MultiCameraDeviceSystemPressureHandler (IntPtr context, MultiCameraDevice.SystemPressureLevel level);
        #endregion


        #region --VKTSession--
        [DllImport(Assembly, EntryPoint = @"VKTSessionGetIdentifier")]
        public static extern Status GetSessionIdentifier (
            [MarshalAs(UnmanagedType.LPUTF8Str)] StringBuilder dest,
            int size
        );
        [DllImport(Assembly, EntryPoint = @"VKTSessionSetToken")]
        public static extern Status SetSessionToken (
            [MarshalAs(UnmanagedType.LPUTF8Str)] string? token
        );
        #endregion


        #region --VKTClock--
        [DllImport(Assembly, EntryPoint = @"VKTClockGetHighResolutionTimestamp")]
        public static extern Status GetHighResolutionTimestamp (out long timestamp);
        #endregion


        #region --VKTMetadata--
        [DllImport(Assembly, EntryPoint = @"VKTMetadataGetCount")]
        public static extern Status GetMetadataCount (
            this IntPtr metadata,
            out int count
        );
        [DllImport(Assembly, EntryPoint = @"VKTMetadataContainsKey")]
        public static extern Status MetadataContainsKey (
            this IntPtr metadata,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string key
        );
        [DllImport(Assembly, EntryPoint = @"VKTMetadataGetKeys")]
        public static extern Status GetMetadataKeys (
            this IntPtr metadata,
            [Out] IntPtr[] keys
        );
        [DllImport(Assembly, EntryPoint = @"VKTMetadataGetFloatValue")]
        public static extern Status GetMetadataFloatValue (
            this IntPtr metadata,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string key,
            [Out] float[]? value,
            ref int count
        );
        #endregion


        #region --VKTSampleBuffer--
        [DllImport(Assembly, EntryPoint = @"VKTSampleBufferRelease")]
        public static extern Status ReleaseSampleBuffer (this IntPtr sampleBuffer);
        [DllImport(Assembly, EntryPoint = @"VKTSampleBufferGetTimestamp")]
        public static extern Status GetSampleBufferTimestamp (
            this IntPtr audioBuffer,
            out long timestamp
        );
        #endregion


        #region --VKTAudioBuffer--
        [DllImport(Assembly, EntryPoint = @"VKTAudioBufferCreate")]
        public static unsafe extern Status CreateAudioBuffer (
            int sampleRate,
            int channelCount,
            float* data,
            int sampleCount,
            long timestamp,
            out IntPtr audioBuffer
        );
        [DllImport(Assembly, EntryPoint = @"VKTAudioBufferGetData")]
        public static unsafe extern Status GetAudioBufferData (
            this IntPtr audioBuffer,
            out float* data
        );
        [DllImport(Assembly, EntryPoint = @"VKTAudioBufferGetSampleCount")]
        public static extern Status GetAudioBufferSampleCount (
            this IntPtr audioBuffer,
            out int sampleCount
        );
        [DllImport(Assembly, EntryPoint = @"VKTAudioBufferGetSampleRate")]
        public static extern Status GetAudioBufferSampleRate (
            this IntPtr audioBuffer,
            out int sampleRate
        );
        [DllImport(Assembly, EntryPoint = @"VKTAudioBufferGetChannelCount")]
        public static extern Status GetAudioBufferChannelCount (
            this IntPtr audioBuffer,
            out int channelCount
        );
        #endregion


        #region --VKTPixelBuffer--
        [DllImport(Assembly, EntryPoint = @"VKTPixelBufferCreate")]
        public static unsafe extern Status CreatePixelBuffer (
            int width,
            int height,
            PixelBuffer.Format format,
            byte* data,
            int rowStride,
            long timestamp,
            [MarshalAs(UnmanagedType.I1)] bool mirrored,
            out IntPtr pixelBuffer
        );
        [DllImport(Assembly, EntryPoint = @"VKTPixelBufferCreatePlanar")]
        public static extern Status CreatePlanarPixelBuffer (
            int width,
            int height,
            PixelBuffer.Format format,
            int planeCount,
            [In] IntPtr[] planeData,
            [In] int[] planeWidth,
            [In] int[] planeHeight,
            [In] int[] planeRowStride,
            [In] int[] planePixelStride,
            long timestamp,
            [MarshalAs(UnmanagedType.I1)] bool mirrored,
            out IntPtr pixelBuffer
        );
        [DllImport(Assembly, EntryPoint = @"VKTPixelBufferGetData")]
        public static unsafe extern Status GetPixelBufferData (
            this IntPtr pixelBuffer,
            out byte* data
        );
        [DllImport(Assembly, EntryPoint = @"VKTPixelBufferGetDataSize")]
        public static extern Status GetPixelBufferDataSize (
            this IntPtr pixelBuffer,
            out int size
        );
        [DllImport(Assembly, EntryPoint = @"VKTPixelBufferGetFormat")]
        public static extern Status GetPixelBufferFormat (
            this IntPtr pixelBuffer,
            out PixelBuffer.Format format
        );
        [DllImport(Assembly, EntryPoint = @"VKTPixelBufferGetWidth")]
        public static extern Status GetPixelBufferWidth (
            this IntPtr pixelBuffer,
            out int width
        );
        [DllImport(Assembly, EntryPoint = @"VKTPixelBufferGetHeight")]
        public static extern Status GetPixelBufferHeight
        (
            this IntPtr pixelBuffer,
            out int height
        );
        [DllImport(Assembly, EntryPoint = @"VKTPixelBufferGetRowStride")]
        public static extern Status GetPixelBufferRowStride (
            this IntPtr pixelBuffer,
            out int rowStride
        );
        [DllImport(Assembly, EntryPoint = @"VKTPixelBufferIsVerticallyMirrored")]
        public static extern Status GetPixelBufferIsVerticallyMirrored (
            this IntPtr pixelBuffer,
            [MarshalAs(UnmanagedType.I1)] out bool mirrored
        );
        [DllImport(Assembly, EntryPoint = @"VKTPixelBufferGetPlaneCount")]
        public static extern Status GetPixelBufferPlaneCount (
            this IntPtr pixelBuffer,
            out int planeCount
        );
        [DllImport(Assembly, EntryPoint = @"VKTPixelBufferGetPlaneData")]
        public static unsafe extern Status GetPixelBufferPlaneData (
            this IntPtr pixelBuffer,
            int planeIdx,
            out byte* planeData
        );
        [DllImport(Assembly, EntryPoint = @"VKTPixelBufferGetPlaneDataSize")]
        public static extern Status GetPixelBufferPlaneDataSize (
            this IntPtr pixelBuffer,
            int planeIdx,
            out int dataSize
        );
        [DllImport(Assembly, EntryPoint = @"VKTPixelBufferGetPlaneWidth")]
        public static extern Status GetPixelBufferPlaneWidth (
            this IntPtr pixelBuffer,
            int planeIdx,
            out int width
        );
        [DllImport(Assembly, EntryPoint = @"VKTPixelBufferGetPlaneHeight")]
        public static extern Status GetPixelBufferPlaneHeight (
            this IntPtr pixelBuffer,
            int planeIdx,
            out int height
        );
        [DllImport(Assembly, EntryPoint = @"VKTPixelBufferGetPlanePixelStride")]
        public static extern Status GetPixelBufferPlanePixelStride (
            this IntPtr pixelBuffer,
            int planeIdx,
            out int pixelStride
        );
        [DllImport(Assembly, EntryPoint = @"VKTPixelBufferGetPlaneRowStride")]
        public static extern Status GetPixelBufferPlaneRowStride (
            this IntPtr pixelBuffer,
            int planeIdx,
            out int rowStride
        );
        [DllImport(Assembly, EntryPoint = @"VKTPixelBufferGetMetadata")]
        public static extern Status GetPixelBufferMetadata (
            this IntPtr pixelBuffer,
            out IntPtr metadata
        );
        [DllImport(Assembly, EntryPoint = @"VKTPixelBufferCopyTo")]
        public static extern Status CopyToPixelBuffer (
            this IntPtr source,
            IntPtr destination,
            PixelBuffer.Rotation rotation
        );
        #endregion

        
        #region --VKTMediaAsset--
        [DllImport(Assembly, EntryPoint = @"VKTMediaAssetCreate")]
        public static extern Status CreateMediaAsset (
            [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
            MediaAssetHandler handler,
            IntPtr context
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaAssetCreateFromCameraRoll")]
        public static extern Status CreateMediaAssetFromCameraRoll (
            MediaAsset.MediaType type,
            MediaAssetHandler handler,
            IntPtr context
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaAssetRelease")]
        public static extern Status ReleaseMediaAsset (this IntPtr asset);
        [DllImport(Assembly, EntryPoint = @"VKTMediaAssetGetPath")]
        public static extern Status GetMediaAssetPath (
            this IntPtr asset,
            [MarshalAs(UnmanagedType.LPUTF8Str)] StringBuilder path,
            int size
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaAssetGetMediaType")]
        public static extern Status GetMediaAssetMediaType (
            this IntPtr asset,
            out MediaAsset.MediaType type
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaAssetGetWidth")]
        public static extern Status GetMediaAssetWidth (
            this IntPtr asset,
            out int width
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaAssetGetHeight")]
        public static extern Status GetMediaAssetHeight (
            this IntPtr asset,
            out int height
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaAssetGetFrameRate")]
        public static extern Status GetMediaAssetFrameRate (
            this IntPtr asset,
            out float frameRate
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaAssetGetSampleRate")]
        public static extern Status GetMediaAssetSampleRate (
            this IntPtr asset,
            out int sampleRate
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaAssetGetChannelCount")]
        public static extern Status GetMediaAssetChannelCount(
            this IntPtr asset,
            out int channelCount
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaAssetGetDuration")]
        public static extern Status GetMediaAssetDuration(
            this IntPtr asset,
            out float duration
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaAssetGetSubAssetCount")]
        public static extern Status GetMediaAssetSubAssetCount(
            this IntPtr asset,
            out int count
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaAssetGetSubAsset")]
        public static extern Status GetMediaAssetSubAsset(
            this IntPtr asset,
            int index,
            out IntPtr subAsset
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaAssetShare")]
        public static extern Status ShareMediaAsset (
            this IntPtr asset,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string? message,
            MediaAssetShareHandler handler,
            IntPtr context
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaAssetSaveToCameraRoll")]
        public static extern Status SaveMediaAssetToCameraRoll (
            this IntPtr asset,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string? album,
            MediaAssetShareHandler handler,
            IntPtr context
        );
        #endregion


        #region --VKTMediaReader--
        [DllImport(Assembly, EntryPoint = @"VKTMediaReaderCreate")]
        public static extern Status CreateMediaReader (
            this IntPtr asset,
            MediaAsset.MediaType type,
            out IntPtr reader
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaReaderRelease")]
        public static extern Status ReleaseMediaReader (this IntPtr reader);
        [DllImport(Assembly, EntryPoint = @"VKTMediaReaderReadNextSampleBuffer")]
        public static extern Status ReadNextSampleBuffer (
            this IntPtr reader,
            out IntPtr sampleBuffer
        );
        #endregion


        #region --VKTRecorder--
        [DllImport(Assembly, EntryPoint = @"VKTMediaRecorderIsFormatSupported")]
        public static extern Status IsMediaRecorderFormatSupported (MediaRecorder.Format format);
        [DllImport(Assembly, EntryPoint = @"VKTMediaRecorderGetFormat")]
        public static extern Status GetMediaRecorderFormat (
            this IntPtr recorder,
            out MediaRecorder.Format format
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaRecorderGetWidth")]
        public static extern Status GetMediaRecorderWidth (
            this IntPtr recorder,
            out int width
        );

        [DllImport(Assembly, EntryPoint = @"VKTMediaRecorderGetHeight")]
        public static extern Status GetMediaRecorderHeight (
            this IntPtr recorder,
            out int height
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaRecorderGetSampleRate")]
        public static extern Status GetMediaRecorderSampleRate (
            this IntPtr recorder,
            out int sampleRate
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaRecorderGetChannelCount")]
        public static extern Status GetMediaRecorderChannelCount (
            this IntPtr recorder,
            out int channelCount
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaRecorderAppendPixelBuffer")]
        public static extern unsafe Status AppendPixelBuffer (
            this IntPtr recorder,
            IntPtr pixelBuffer
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaRecorderAppendAudioBuffer")]
        public static extern unsafe Status AppendSampleBuffer (
            this IntPtr recorder,
            IntPtr audioBuffer
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaRecorderFinishWriting")]
        public static extern Status FinishWriting (
            this IntPtr recorder,
            MediaAssetHandler handler,
            IntPtr context
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaRecorderCreateMP4")]
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
        [DllImport(Assembly, EntryPoint = @"VKTMediaRecorderCreateHEVC")]
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
        [DllImport(Assembly, EntryPoint = @"VKTMediaRecorderCreateGIF")]
        public static extern Status CreateGIFRecorder (
            [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
            int width,
            int height,
            float delay,
            out IntPtr recorder
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaRecorderCreateWAV")]
        public static extern Status CreateWAVRecorder (
            [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
            int sampleRate,
            int channelCount,
            out IntPtr recorder
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaRecorderCreateWEBM")]
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
        [DllImport(Assembly, EntryPoint = @"VKTMediaRecorderCreateJPEG")]
        public static extern Status CreateJPEGRecorder (
            [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
            int width,
            int height,
            float quality,
            out IntPtr recorder
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaRecorderCreateAV1")]
        public static extern Status CreateAV1Recorder (
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
        [DllImport(Assembly, EntryPoint = @"VKTMediaRecorderCreateProRes4444")]
        public static extern Status CreateProRes4444Recorder (
            [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
            int width,
            int height,
            int sampleRate,
            int channelCount,
            int audioBitRate,
            out IntPtr recorder
        );
        #endregion


        #region --VKTMediaDevice--
        [DllImport(Assembly, EntryPoint = @"VKTMediaDeviceRelease")]
        public static extern Status ReleaseMediaDevice (this IntPtr device);
        [DllImport(Assembly, EntryPoint = @"VKTMediaDeviceGetUniqueID")]
        public static extern Status GetMediaDeviceUniqueID (
            this IntPtr device,
            [MarshalAs(UnmanagedType.LPUTF8Str)] StringBuilder dest,
            int size
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaDeviceGetName")]
        public static extern Status GetMediaDeviceName (
            this IntPtr device,
            [MarshalAs(UnmanagedType.LPUTF8Str)] StringBuilder dest,
            int size
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaDeviceGetFlags")]
        public static extern Status GetMediaDeviceFlags (
            this IntPtr device,
            out MediaDeviceFlags flags
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaDeviceIsRunning")]
        public static extern Status GetMediaDeviceIsRunning (
            this IntPtr device,
            [MarshalAs(UnmanagedType.I1)] out bool running
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaDeviceStartRunning")]
        public static extern Status StartRunning (
            this IntPtr device,
            SampleBufferHandler handler,
            IntPtr context
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaDeviceStopRunning")]
        public static extern Status StopRunning (this IntPtr device);
        [DllImport(Assembly, EntryPoint = @"VKTMediaDeviceSetDisconnectHandler")]
        public static extern Status SetDisconnectHandler (
            this IntPtr device,
            MediaDeviceDisconnectHandler handler,
            IntPtr context
        );
        [DllImport(Assembly, EntryPoint = @"VKTMediaDeviceCheckPermissions")]
        public static extern Status CheckPermissions (
            PermissionType type,
            bool request,
            MediaDevicePermissionResultHandler handler,
            IntPtr context
        );
        #endregion


        #region --VKTAudioDevice--
        [DllImport(Assembly, EntryPoint = @"VKTAudioDeviceDiscoverDevices")]
        public static extern Status DiscoverAudioDevices (
            MediaDeviceDiscoveryHandler handler,
            IntPtr context
        );
        [DllImport(Assembly, EntryPoint = @"VKTAudioDeviceGetEchoCancellation")]
        public static extern Status GetAudioDeviceEchoCancellation (
            this IntPtr audioDevice,
            [MarshalAs(UnmanagedType.I1)] out bool echoCancellation
        );
        [DllImport(Assembly, EntryPoint = @"VKTAudioDeviceSetEchoCancellation")]
        public static extern Status SetAudioDeviceEchoCancellation (
            this IntPtr audioDevice,
            [MarshalAs(UnmanagedType.I1)] bool mode
        );
        [DllImport(Assembly, EntryPoint = @"VKTAudioDeviceGetSampleRate")]
        public static extern Status GetAudioDeviceSampleRate (
            this IntPtr audioDevice,
            out int sampleRate
        );
        [DllImport(Assembly, EntryPoint = @"VKTAudioDeviceSetSampleRate")]
        public static extern Status SetAudioDeviceSampleRate (
            this IntPtr audioDevice,
            int sampleRate
        );
        [DllImport(Assembly, EntryPoint = @"VKTAudioDeviceGetChannelCount")]
        public static extern Status GetAudioDeviceChannelCount (
            this IntPtr audioDevice,
            out int channelCount
        );
        [DllImport(Assembly, EntryPoint = @"VKTAudioDeviceSetChannelCount")]
        public static extern Status SetAudioDeviceChannelCount (
            this IntPtr audioDevice,
            int sampleRate
        );
        #endregion


        #region --VKTCameraDevice--
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceDiscoverDevices")]
        public static extern Status DiscoverCameraDevices (
            MediaDeviceDiscoveryHandler handler,
            IntPtr context
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceGetFieldOfView")]
        public static extern Status GetCameraDeviceFieldOfView (
            this IntPtr camera,
            out float x,
            out float y
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceGetExposureBiasRange")]
        public static extern Status GetCameraDeviceExposureBiasRange (
            this IntPtr camera,
            out float min,
            out float max
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceGetExposureDurationRange")]
        public static extern Status GetCameraDeviceExposureDurationRange (
            this IntPtr camera,
            out float min,
            out float max
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceGetISORange")]
        public static extern Status GetCameraDeviceISORange (
            this IntPtr device,
            out float min,
            out float max
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceGetZoomRange")]
        public static extern Status GetCameraDeviceZoomRange (
            this IntPtr camera,
            out float min,
            out float max
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceGetPreviewResolution")]
        public static extern Status GetCameraDevicePreviewResolution (
            this IntPtr camera,
            out int width,
            out int height
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceSetPreviewResolution")]
        public static extern Status SetCameraDevicePreviewResolution (
            this IntPtr camera,
            int width,
            int height
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceGetPhotoResolution")]
        public static extern Status GetCameraDevicePhotoResolution (
            this IntPtr camera,
            out int width,
            out int height
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceSetPhotoResolution")]
        public static extern Status SetCameraDevicePhotoResolution (
            this IntPtr camera,
            int width,
            int height
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceGetFrameRate")]
        public static extern Status GetCameraDeviceFrameRate (
            this IntPtr camera,
            out float frameRate
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceSetFrameRate")]
        public static extern Status SetCameraDeviceFrameRate (
            this IntPtr camera,
            float frameRate
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceGetExposureMode")]
        public static extern Status GetCameraDeviceExposureMode (
            this IntPtr camera,
            out CameraDevice.ExposureMode mode
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceSetExposureMode")]
        public static extern Status SetCameraDeviceExposureMode (
            this IntPtr camera,
            CameraDevice.ExposureMode mode
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceGetExposureBias")]
        public static extern Status GetCameraDeviceExposureBias (
            this IntPtr camera,
            out float bias
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceSetExposureBias")]
        public static extern Status SetCameraDeviceExposureBias (
            this IntPtr camera,
            float bias
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceGetExposureDuration")]
        public static extern Status GetCameraDeviceExposureDuration (
            this IntPtr camera,
            out float duration
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceGetISO")]
        public static extern Status GetCameraDeviceISO (
            this IntPtr camera,
            out float ISO
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceSetExposureDuration")]
        public static extern Status SetCameraDeviceExposureDuration (
            this IntPtr camera,
            float duration,
            float ISO
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceSetExposurePoint")]
        public static extern Status SetCameraDeviceExposurePoint (
            this IntPtr camera,
            float x,
            float y
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceGetFlashMode")]
        public static extern Status GetCameraDeviceFlashMode (
            this IntPtr camera,
            out CameraDevice.FlashMode mode
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceSetFlashMode")]
        public static extern Status SetCameraDeviceFlashMode (
            this IntPtr camera,
            CameraDevice.FlashMode mode
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceGetFocusMode")]
        public static extern Status GetCameraDeviceFocusMode (
            this IntPtr camera,
            out CameraDevice.FocusMode mode
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceSetFocusMode")]
        public static extern Status SetCameraDeviceFocusMode (
            this IntPtr camera,
            CameraDevice.FocusMode mode
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceSetFocusPoint")]
        public static extern Status SetCameraDeviceFocusPoint (
            this IntPtr camera,
            float x,
            float y
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceGetTorchMode")]
        public static extern Status GetCameraDeviceTorchMode (
            this IntPtr camera,
            out CameraDevice.TorchMode mode
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceSetTorchMode")]
        public static extern Status SetCameraDeviceTorchMode (
            this IntPtr camera,
            CameraDevice.TorchMode mode
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceGetWhiteBalanceMode")]
        public static extern Status GetCameraDeviceWhiteBalanceMode (
            this IntPtr camera,
            out CameraDevice.WhiteBalanceMode mode
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceSetWhiteBalanceMode")]
        public static extern Status SetCameraDeviceWhiteBalanceMode (
            this IntPtr camera,
            CameraDevice.WhiteBalanceMode mode
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceGetVideoStabilizationMode")]
        public static extern Status GetCameraDeviceVideoStabilizationMode (
            this IntPtr camera,
            out CameraDevice.VideoStabilizationMode mode
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceSetVideoStabilizationMode")]
        public static extern Status SetCameraDeviceVideoStabilizationMode (
            this IntPtr camera,
            CameraDevice.VideoStabilizationMode mode
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceGetZoomRatio")]
        public static extern Status GetCameraDeviceZoomRatio (
            this IntPtr camera,
            out float zoom
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceSetZoomRatio")]
        public static extern Status SetCameraDeviceZoomRatio (
            this IntPtr camera,
            float ratio
        );
        [DllImport(Assembly, EntryPoint = @"VKTCameraDeviceCapturePhoto")]
        public static extern Status CapturePhoto (
            this IntPtr camera,
            SampleBufferHandler handler,
            IntPtr context
        );
        #endregion


        #region --VKTStatus--
        [DllImport(Assembly, EntryPoint = @"VKTGetVersion")]
        public static extern IntPtr GetVersion ();
        #endregion


        #region --iOS--
        #if UNITY_IOS && !UNITY_EDITOR
        [DllImport(Assembly, EntryPoint = @"VKTConfigureAudioSession")]
        public static extern void ConfigureAudioSession ();
        #else
        public static void ConfigureAudioSession () { }
        #endif
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

        public static Status Throw (this Status status) {
            switch (status) {
                case Status.Ok:                 return status;
                case Status.InvalidArgument:    throw new ArgumentException();
                case Status.InvalidOperation:   throw new InvalidOperationException();
                case Status.NotImplemented:     throw new NotImplementedException();
                case Status.InvalidSession:     throw new InvalidOperationException(@"VideoKit session token is invalid. Get your VideoKit access key at https://videokit.ai");
                case Status.InvalidPlan:        throw new InvalidOperationException(@"VideoKit plan does not support this operation. Check your plan and upgrade at https://videokit.ai");
                case Status.LimitedPlan:        Debug.LogWarning(@"VideoKit plan only allows for limited functionality. Check your plan and upgrade at https://videokit.ai"); return status;
                default:                        throw new InvalidOperationException();
            }
        }

        public static async Task<Prediction> Throw (this Task<Prediction> task) => (await task).Throw();

        public static Prediction Throw (this Prediction prediction) {
            // Check
            if (!string.IsNullOrEmpty(prediction.error))
                throw new InvalidOperationException(prediction.error);
            // Return
            return prediction;
        }

        public static bool IsOk (this Status status) => status == Status.Ok || status == Status.LimitedPlan;
        #endregion
    }
}