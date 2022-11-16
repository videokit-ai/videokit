/* 
*   VideoKit
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit.Editor {

    using System;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Format = VideoKitRecorder.Format;
    using VideoMode = VideoKitRecorder.VideoMode;
    using AudioMode = VideoKitRecorder.AudioMode;
    using OrientationMode = VideoKitRecorder.OrientationMode;

    [CustomEditor(typeof(VideoKitRecorder)), CanEditMultipleObjects]
    internal sealed class VideoKitRecorderEditor : Editor {

        private SerializedProperty formatProperty;
        private SerializedProperty destinationProperty;
        private SerializedProperty videoModeProperty;
        private SerializedProperty audioModeProperty;
        private SerializedProperty resolutionProperty;
        private SerializedProperty orientationProperty;
        private SerializedProperty aspectProperty;
        private SerializedProperty camerasProperty;
        private SerializedProperty textureProperty;
        private SerializedProperty onRecordingCompletedProperty;
        private SerializedProperty onRecordingFailedProperty;
        private readonly Format[] VideoFormats = new [] { Format.MP4, Format.HEVC, Format.WEBM, Format.GIF, Format.JPEG };
        private readonly Format[] AudioFormats = new [] { Format.MP4, Format.HEVC, Format.WEBM, Format.WAV };
        
        private void OnEnable () {
            formatProperty = serializedObject.FindProperty(@"format");
            destinationProperty = serializedObject.FindProperty(@"destination");
            videoModeProperty = serializedObject.FindProperty(@"videoMode");
            audioModeProperty = serializedObject.FindProperty(@"audioMode");
            resolutionProperty = serializedObject.FindProperty(@"resolution");
            orientationProperty = serializedObject.FindProperty(@"orientation");
            aspectProperty = serializedObject.FindProperty(@"aspect");
            camerasProperty = serializedObject.FindProperty(@"cameras");
            textureProperty = serializedObject.FindProperty(@"texture");
            onRecordingCompletedProperty = serializedObject.FindProperty(@"OnRecordingCompleted");
            onRecordingFailedProperty = serializedObject.FindProperty(@"OnRecordingFailed");
        }

        public override void OnInspectorGUI () {
            serializedObject.Update();
            // Format
            EditorGUILayout.PropertyField(formatProperty);
            EditorGUILayout.PropertyField(destinationProperty);
            // Video and audio mode
            var format = (Format)Enum.GetValues(typeof(Format)).GetValue(formatProperty.enumValueIndex);
            var videoOptions = Array.IndexOf(VideoFormats, format) > -1;
            var audioOptions = Array.IndexOf(AudioFormats, format) > -1;
            var videoMode = (VideoMode)Enum.GetValues(typeof(VideoMode)).GetValue(videoModeProperty.enumValueIndex);
            var audioMode = (AudioMode)audioModeProperty.enumValueFlag;
            if (videoOptions)
                EditorGUILayout.PropertyField(videoModeProperty);
            if (audioOptions)
                EditorGUILayout.PropertyField(audioModeProperty);
            // Video config
            var orientation = (OrientationMode)Enum.GetValues(typeof(OrientationMode)).GetValue(orientationProperty.enumValueIndex);
            if (videoOptions && videoMode != VideoMode.None) {
                EditorGUILayout.PropertyField(resolutionProperty);
                EditorGUILayout.PropertyField(orientationProperty);
                if (orientation == OrientationMode.MatchScreen)
                    EditorGUILayout.PropertyField(aspectProperty);
                if (videoMode == VideoMode.Camera)
                    EditorGUILayout.PropertyField(camerasProperty);
                else if (videoMode == VideoMode.Texture)
                    EditorGUILayout.PropertyField(textureProperty);
            }
            // Events
            EditorGUILayout.PropertyField(onRecordingCompletedProperty);
            EditorGUILayout.PropertyField(onRecordingFailedProperty);
            // Apply
            serializedObject.ApplyModifiedProperties();
        }
    }
}