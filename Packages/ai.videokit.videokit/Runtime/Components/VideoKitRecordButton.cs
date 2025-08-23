/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

namespace VideoKit.UI {

	using System.Collections;
	using UnityEngine;
	using UnityEngine.UI;
	using UnityEngine.Events;
	using UnityEngine.EventSystems;
    using UnityEngine.Serialization;

    /// <summary>
    /// Lightweight record button with press-and-hold gesture.
    /// </summary>
    [RequireComponent(typeof(Image)), AddComponentMenu(@""), DisallowMultipleComponent]
    public sealed class VideoKitRecordButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

        #region --Inspector--
        [Header(@"Settings")]
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
        /// Event invoked when the button is tapped.
        /// </summary>
        [Tooltip(@"Event invoked when the button is tapped.")]
        public UnityEvent OnTap;

        /// <summary>
        /// Event invoked when the button starts being held.
        /// </summary>
        [Tooltip(@"Event invoked when the button starts being held."), FormerlySerializedAs(@"OnStartRecording")]
        public UnityEvent OnBeginHold;

        /// <summary>
        /// Event invoked when the button stops being held.
        /// </summary>
        [Tooltip(@"Event invoked when the button stops being held."), FormerlySerializedAs(@"OnStopRecording")]
        public UnityEvent OnEndHold;
        #endregion


        #region --Operations--
        private Image button;
        private bool touch;
        private const float TapDelay = 0.2f;

        private void Awake() => button = GetComponent<Image>();

        private void Start() => Zero();

        private void Zero() {
            button.fillAmount = 1.0f;
            countdown.fillAmount = 0.0f;
        }

        private IEnumerator Countdown() {
            touch = true;
            // Wait for false touch
            yield return new WaitForSeconds(TapDelay);
            if (!touch) {
                OnTap?.Invoke();
                yield break;
            }
            // Start recording
            OnBeginHold?.Invoke();
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
            OnEndHold?.Invoke();
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) => StartCoroutine(Countdown());

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) => touch = false;
        #endregion
    }
}