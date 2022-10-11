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

    [CustomEditor(typeof(VideoKitRecorder)), CanEditMultipleObjects]
    internal sealed class VideoKitRecorderEditor : Editor {

        SerializedProperty formatProperty;
        SerializedProperty destinationProperty;
        SerializedProperty videoModeProperty;
        SerializedProperty audioModeProperty;
        SerializedProperty resolutionProperty;
        SerializedProperty resolutionOptionsProperty;
        SerializedProperty frameRateProperty;
        SerializedProperty camerasProperty;
        SerializedProperty textureProperty;
        SerializedProperty watermarkProperty;
        SerializedProperty audioSourceProperty;
        SerializedProperty onRecordingProperty;
        
        private void OnEnable () {
            formatProperty = serializedObject.FindProperty(@"format");
            destinationProperty = serializedObject.FindProperty(@"destination");
            videoModeProperty = serializedObject.FindProperty(@"videoMode");
            audioModeProperty = serializedObject.FindProperty(@"audioMode");
            resolutionProperty = serializedObject.FindProperty(@"resolution");
            resolutionOptionsProperty = serializedObject.FindProperty(@"resolutionOptions");
            frameRateProperty = serializedObject.FindProperty(@"frameRate");
            camerasProperty = serializedObject.FindProperty(@"cameras");
            textureProperty = serializedObject.FindProperty(@"texture");
            watermarkProperty = serializedObject.FindProperty(@"watermark");
            audioSourceProperty = serializedObject.FindProperty(@"audioSource");
            onRecordingProperty = serializedObject.FindProperty(@"OnRecording");
        }

        public override void OnInspectorGUI () {
            serializedObject.Update();
            // Format
            EditorGUILayout.PropertyField(formatProperty);
            EditorGUILayout.PropertyField(destinationProperty);
            // Video and audio mode
            var format = (Format)Enum.GetValues(typeof(Format)).GetValue(formatProperty.enumValueIndex);
            var videoOptions = format == Format.MP4 || format == Format.GIF;
            var audioOptions = format == Format.MP4 || format == Format.WAV;
            var videoMode = (VideoMode)Enum.GetValues(typeof(VideoMode)).GetValue(videoModeProperty.enumValueIndex);
            var audioMode = (AudioMode)Enum.GetValues(typeof(AudioMode)).GetValue(audioModeProperty.enumValueIndex);
            if (videoOptions)
                EditorGUILayout.PropertyField(videoModeProperty);
            if (audioOptions)
                EditorGUILayout.PropertyField(audioModeProperty);
            // Video config
            if (videoOptions && videoMode != VideoMode.None) {
                EditorGUILayout.PropertyField(resolutionProperty);
                EditorGUILayout.PropertyField(resolutionOptionsProperty);
                EditorGUILayout.PropertyField(frameRateProperty);
                if (videoMode == VideoMode.Camera)
                    EditorGUILayout.PropertyField(camerasProperty);
                else if (videoMode == VideoMode.Texture)
                    EditorGUILayout.PropertyField(textureProperty);
                EditorGUILayout.PropertyField(watermarkProperty);
            }
            // Audio config
            if (audioOptions && audioMode != AudioMode.None) {
                if (audioMode == AudioMode.AudioSource)
                    EditorGUILayout.PropertyField(audioSourceProperty);
            }
            // Events
            EditorGUILayout.PropertyField(onRecordingProperty);
            // Apply
            serializedObject.ApplyModifiedProperties();
        }
    }
}