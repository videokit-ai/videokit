/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.Internal {

    using UnityEngine;
    using static VideoKitAudioManager;
    using static VideoKitRecorder;

    internal static class VideoKitTypeExtensions {

        public static (int width, int height) FrameSize (this VideoKitRecorder.Resolution resolution, Vector2Int customResolution) => resolution switch {
            VideoKitRecorder.Resolution._320x240    => (320, 240),
            VideoKitRecorder.Resolution._480x320    => (480, 320),
            VideoKitRecorder.Resolution._640x480    => (640, 480),
            VideoKitRecorder.Resolution._1280x720   => (1280, 720),
            VideoKitRecorder.Resolution._1920x1080  => (1920, 1080),
            VideoKitRecorder.Resolution._2K         => (2560, 1440),
            VideoKitRecorder.Resolution._4K         => (3840, 2160),
            VideoKitRecorder.Resolution.Screen      => (Screen.width, Screen.height).EnsureLandscape(),
            VideoKitRecorder.Resolution.HalfScreen  => (Screen.width >> 1, Screen.height >> 1).EnsureLandscape(),
            VideoKitRecorder.Resolution.Custom      => (customResolution.x, customResolution.y),
            _                                       => (1280, 720),
        };

        public static (int width, int height) FrameSize (this VideoKitCameraManager.Resolution resolution) => resolution switch {
            VideoKitCameraManager.Resolution.Lowest     => (176, 144),
            VideoKitCameraManager.Resolution._640x480   => (640, 480),
            VideoKitCameraManager.Resolution._1280x720  => (1280, 720),
            VideoKitCameraManager.Resolution._1920x1080 => (1920, 1080),
            VideoKitCameraManager.Resolution._4K        => (3840, 2160),
            VideoKitCameraManager.Resolution.Highest    => (5120, 2880),
            _                                           => (1280, 720),
        };

        public static (int width, int height) EnsureLandscape (this (int width, int height) res) => (
            Mathf.Max(res.width, res.height),
            Mathf.Min(res.width, res.height)
        );

        public static (int width, int height) EnsureEven (this (int width, int height) res) => (
            res.width >> 1 << 1,
            res.height >> 1 << 1
        );

        public static AspectMode EnsureOrientation (this AspectMode aspect, OrientationMode orientation) => orientation switch {
            OrientationMode.MatchScreen => AspectMode.Resolution,
            _                           => aspect,
        };

        public static (int width, int height) EnsureOrientation (this (int width, int height) res, OrientationMode orientation) => orientation switch {
            OrientationMode.Portrait                                        => (res.height, res.width),
            OrientationMode.MatchScreen when Screen.width < Screen.height   => (res.height, res.width),
            _                                                               => res,
        };

        public static (int width, int height) EnsureAspect (this (int width, int height) res, AspectMode aspect) => aspect switch {
            AspectMode.MatchScreenAdjustWidth   => ((int)((float)Screen.width / Screen.height * res.height), res.height),
            AspectMode.MatchScreenAdjustHeight  => (res.width, (int)((float)Screen.height / Screen.width * res.width)),
            _                                   => res,
        };

        public static int ToInt (this SampleRate sampleRate) => sampleRate switch {
            SampleRate.MatchUnity   => AudioSettings.outputSampleRate,
            _                       => (int)sampleRate,
        };

        public static int ToInt (this ChannelCount channelCount) => channelCount switch {
            ChannelCount.MatchUnity => (int)AudioSettings.speakerMode,
            _                       => (int)channelCount,
        };
    }
}