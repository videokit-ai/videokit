/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.UI {

	using System.Collections;
	using UnityEngine;
	using UnityEngine.UI;
	using UnityEngine.Events;
	using UnityEngine.EventSystems;

    /// <summary>
    /// Lightweight record button with press-and-hold gesture.
    /// </summary>
    [RequireComponent(typeof(Image)), AddComponentMenu(@""), DisallowMultipleComponent]
    public sealed class VideoKitRecordButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

        #region --Inspector--
        [Header(@"Settings")]
        /// <summary>
        /// Recorder to control.
        /// </summary>
        [Tooltip(@"Recorder to control.")]
        public VideoKitRecorder recorder;

        /// <summary>
        /// Maximum duration that button can be pressed.
        /// </summary>
        [Range(5f, 60f), Tooltip(@"Maximum duration that button can be pressed.")]
        public float maxDuration = 10f;

        [Header(@"UI")]
        /// <summary>
        /// Countdown image.
        /// </summary>
        [Tooltip(@"Countdown image.")]
        public Image countdown;

        [Header(@"Events")]
        /// <summary>
        /// Event invoked when recording is started.
        /// </summary>
        [Tooltip(@"Event invoked when recording is started.")]
        public UnityEvent OnStartRecording;

        /// <summary>
        /// Event invoked when recording is stopped.
        /// </summary>
        [Tooltip(@"Event invoked when recording is stopped.")]
        public UnityEvent OnStopRecording;
        #endregion


        #region --Operations--
        private Image button;
        private bool touch;

        private void Reset () => recorder = FindObjectOfType<VideoKitRecorder>();

        private void Awake () => button = GetComponent<Image>();

        private void Start () => Zero();

        private void Zero () {
            button.fillAmount = 1.0f;
            countdown.fillAmount = 0.0f;
        }

        private IEnumerator Countdown () {
            touch = true;
            // Wait for false touch
            yield return new WaitForSeconds(0.2f);
            if (!touch)
                yield break;
            // Start recording
            StartRecording();
            // Animate the countdown
            var startTime = Time.time;
            while (touch) {
                var ratio = (Time.time - startTime) / maxDuration;
                touch = ratio <= 1f;
                countdown.fillAmount = ratio;
                button.fillAmount = 1f - ratio;
                yield return null;
            }
            // Reset
            Zero();
            StopRecording();
        }

        private async void StartRecording () {
            if (recorder != null)
                await recorder.StartRecording();
            OnStartRecording?.Invoke();
        }

        private async void StopRecording () {
            if (recorder != null)
                await recorder.StopRecording();
            OnStopRecording?.Invoke();
        }

        void IPointerDownHandler.OnPointerDown (PointerEventData eventData) => StartCoroutine(Countdown());

        void IPointerUpHandler.OnPointerUp (PointerEventData eventData) => touch = false;
        #endregion
    }
}