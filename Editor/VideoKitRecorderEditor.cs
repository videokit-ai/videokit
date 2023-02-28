/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.Editor {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using static VideoKitRecorder;
    using Resolution = VideoKitRecorder.Resolution;

    [CustomEditor(typeof(VideoKitRecorder)), CanEditMultipleObjects]
    internal sealed class VideoKitRecorderEditor : Editor {

        private readonly Dictionary<string, SerializedProperty> properties = new();
        private readonly Format[] VideoFormats = new [] { Format.MP4, Format.HEVC, Format.WEBM, Format.GIF, Format.JPEG };
        private readonly Format[] AudioFormats = new [] { Format.MP4, Format.HEVC, Format.WEBM, Format.WAV };
        
        private void OnEnable () {
            var propertyNames = new [] {
                @"format", @"destination", @"prepareOnAwake", @"videoMode", @"audioMode", @"resolution", @"customResolution",
                @"cameras", @"texture", @"cameraManager", @"frameSkip", @"watermarkMode", @"watermark",
                @"watermarkRect", @"audioManager", @"configureAudioManager", @"audioDeviceGain", @"OnRecordingCompleted",
            };
            foreach (var name in propertyNames) {
                var property = serializedObject.FindProperty(name);
                if (property == null)
                    throw new InvalidOperationException($"Cannot find property {name} in `VideoKitRecorder`");
                properties.Add(name, property);
            }
        }

        public override void OnInspectorGUI () {
            serializedObject.Update();
            // Format
            EditorGUILayout.PropertyField(properties[@"format"]);
            EditorGUILayout.PropertyField(properties[@"destination"]);
            EditorGUILayout.PropertyField(properties[@"prepareOnAwake"]);
            // Video mode
            var format = (Format)Enum.GetValues(typeof(Format)).GetValue(properties[@"format"].enumValueIndex);
            var videoOptions = Array.IndexOf(VideoFormats, format) > -1;
            var videoMode = (VideoMode)Enum.GetValues(typeof(VideoMode)).GetValue(properties[@"videoMode"].enumValueIndex);
            if (videoOptions) {
                EditorGUILayout.PropertyField(properties[@"videoMode"]);
                if (videoMode != VideoMode.None) {
                    // Video resolution
                    var resolution = (Resolution)Enum.GetValues(typeof(Resolution)).GetValue(properties[@"resolution"].enumValueIndex);
                    EditorGUILayout.PropertyField(properties[@"resolution"]);
                    if (resolution == Resolution.Custom)
                        EditorGUILayout.PropertyField(properties[@"customResolution"]);
                    // Video input
                    switch (videoMode) {
                        case VideoMode.Camera:
                            EditorGUILayout.PropertyField(properties[@"cameras"]);
                            break;
                        case VideoMode.Texture:
                            EditorGUILayout.PropertyField(properties[@"texture"]);
                            break;
                        case VideoMode.CameraDevice:
                            EditorGUILayout.PropertyField(properties[@"cameraManager"]);
                            break;
                    }
                    // Frame skip
                    EditorGUILayout.PropertyField(properties[@"frameSkip"]);
                    // Watermark
                    var watermarkMode = (WatermarkMode)Enum.GetValues(typeof(WatermarkMode)).GetValue(properties[@"watermarkMode"].enumValueIndex);
                    EditorGUILayout.PropertyField(properties[@"watermarkMode"]);
                    if (watermarkMode != WatermarkMode.None)
                        EditorGUILayout.PropertyField(properties[@"watermark"]);
                    if (watermarkMode == WatermarkMode.Custom)
                        EditorGUILayout.PropertyField(properties[@"watermarkRect"]);
                }
            }
            // Audio mode            
            var audioOptions = Array.IndexOf(AudioFormats, format) > -1;
            var audioMode = (AudioMode)properties[@"audioMode"].enumValueFlag;
            if (audioOptions) {
                EditorGUILayout.PropertyField(properties[@"audioMode"]);
                if (audioMode.HasFlag(AudioMode.AudioDevice)) {
                    EditorGUILayout.PropertyField(properties[@"audioManager"]);
                    EditorGUILayout.PropertyField(properties[@"configureAudioManager"]);
                }
                if (audioMode.HasFlag(AudioMode.AudioListener) && audioMode.HasFlag(AudioMode.AudioDevice))
                    EditorGUILayout.PropertyField(properties[@"audioDeviceGain"]);
            }
            // Events
            EditorGUILayout.PropertyField(properties[@"OnRecordingCompleted"]);
            // Apply
            serializedObject.ApplyModifiedProperties();
        }
    }
}