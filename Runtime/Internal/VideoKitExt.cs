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
    using static VideoKit;

    public static class VideoKitExt {

        #region --Delegates--
        public delegate void ReadbackHandler (IntPtr context, IntPtr pixelBuffer);
        #endregion


        #region --GLESTextureInput--
        #if UNITY_ANDROID && !UNITY_EDITOR
        [DllImport(VideoKit.Assembly, EntryPoint = @"VKTGLESTextureInputCreate")]
        public static extern void CreateTexutreInput (
            int width,
            int height,
            ReadbackHandler handler,
            out IntPtr input
        );

        [DllImport(VideoKit.Assembly, EntryPoint = @"VKTGLESTextureInputCommitFrame")]
        public static extern void CommitFrame (
            this IntPtr input,
            IntPtr texture,
            IntPtr context
        );

        [DllImport(VideoKit.Assembly, EntryPoint = @"VKTGLESTextureInputRelease")]
        public static extern void ReleaseTextureInput (this IntPtr input);
        #else

        public static void CreateTexutreInput (
            int width,
            int height,
            ReadbackHandler handler,
            out IntPtr input
        ) => input = IntPtr.Zero;

        public static void CommitFrame (
            this IntPtr input,
            IntPtr texture,
            IntPtr context
        ) { }

        public static void ReleaseTextureInput (this IntPtr input) { }
        #endif
        #endregion


        #region --AudioSession--
        #if UNITY_IOS && !UNITY_EDITOR
        [DllImport(Assembly, EntryPoint = @"VKTConfigureAudioSession")]
        public static extern void ConfigureAudioSession ();
        #else
        public static void ConfigureAudioSession () { }
        #endif
        #endregion


        #region --IO--
        #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport(Assembly, EntryPoint = @"VKTAssetWriteImage")]
        public static extern Status WriteImage (
            byte[] data,
            int size,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder path,
            int pathLen
        );
        #else
        public static Status WriteImage (
            byte[] data,
            int size,
            StringBuilder path,
            int pathLen
        ) => Status.Ok;
        #endif
        #endregion
    }
}