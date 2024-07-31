/* 
*   VideoKit
*   Copyright © 2024 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Editor {

    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using static VideoKitRecorder;
    using MediaFormat = MediaRecorder.Format;
    using Resolution = VideoKitRecorder.Resolution;

    [CustomEditor(typeof(VideoKitRecorder)), CanEditMultipleObjects]
    internal sealed class VideoKitRecorderEditor : Editor {

        private readonly Dictionary<string, SerializedProperty> properties = new();

        private void OnEnable () {
            var propertyNames = new [] {
                @"format", @"recordingAction", @"prepareOnAwake", @"videoMode", @"audioMode", @"resolution", @"customResolution",
                @"cameras", @"texture", @"cameraManager", @"_frameRate", @"frameSkip", @"watermarkMode", @"_watermark",
                @"_watermarkRect", @"audioManager", @"configureAudioManager", @"audioListener", @"audioSource",
                @"OnRecordingCompleted",
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
            EditorGUILayout.PropertyField(properties[@"prepareOnAwake"]);
            // Video mode
            var format = (MediaFormat)Enum.GetValues(typeof(MediaFormat)).GetValue(properties[@"format"].enumValueIndex);            
            if (MediaRecorder.CanAppend<PixelBuffer>(format)) {
                var videoMode = (VideoMode)Enum.GetValues(typeof(VideoMode)).GetValue(properties[@"videoMode"].enumValueIndex);
                EditorGUILayout.PropertyField(properties[@"videoMode"]);
                if (videoMode != VideoMode.None) {
                    // Video resolution
                    var resolution = (Resolution)Enum.GetValues(typeof(Resolution)).GetValue(properties[@"resolution"].enumValueIndex);
                    if (videoMode == VideoMode.CameraDevice) {
                        EditorGUILayout.LabelField(@"Resolution", "Configure in camera manager.");
                    } else {
                        EditorGUILayout.PropertyField(properties[@"resolution"]);
                        if (resolution == Resolution.Custom)
                            EditorGUILayout.PropertyField(properties[@"customResolution"]);
                    }
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
                    // Frame duration
                    if (format == MediaFormat.GIF)
                        EditorGUILayout.PropertyField(properties[@"_frameRate"]);
                    // Frame skip
                    if (videoMode != VideoMode.CameraDevice)
                        EditorGUILayout.PropertyField(properties[@"frameSkip"]);
                    // Watermark
                    var watermarkMode = (WatermarkMode)Enum.GetValues(typeof(WatermarkMode)).GetValue(properties[@"watermarkMode"].enumValueIndex);
                    EditorGUILayout.PropertyField(properties[@"watermarkMode"]);
                    if (watermarkMode != WatermarkMode.None)
                        EditorGUILayout.PropertyField(properties[@"_watermark"]);
                    if (watermarkMode == WatermarkMode.Custom)
                        EditorGUILayout.PropertyField(properties[@"_watermarkRect"]);
                }
            }
            // Audio mode            
            if (MediaRecorder.CanAppend<AudioBuffer>(format)) {
                var audioMode = (AudioMode)properties[@"audioMode"].enumValueFlag;
                EditorGUILayout.PropertyField(properties[@"audioMode"]);
                if (audioMode.HasFlag(AudioMode.AudioDevice)) {
                    EditorGUILayout.PropertyField(properties[@"audioManager"]);
                    EditorGUILayout.PropertyField(properties[@"configureAudioManager"]);
                }
                if (audioMode.HasFlag(AudioMode.AudioListener))
                    EditorGUILayout.PropertyField(properties[@"audioListener"]);
                if (audioMode.HasFlag(AudioMode.AudioSource))
                    EditorGUILayout.PropertyField(properties[@"audioSource"]);
                //if (audioMode == AudioMode.ExperimentalAudioListenerAndDeviceMixer)
                //    EditorGUILayout.PropertyField(properties[@"audioDeviceGain"]);
            }
            // Events
            var recordingAction = (RecordingAction)properties[@"recordingAction"].enumValueFlag;
            EditorGUILayout.PropertyField(properties[@"recordingAction"]);
            if (recordingAction.HasFlag(RecordingAction.Custom))
                EditorGUILayout.PropertyField(properties[@"OnRecordingCompleted"]);
            // Apply
            serializedObject.ApplyModifiedProperties();
        }
    }
}