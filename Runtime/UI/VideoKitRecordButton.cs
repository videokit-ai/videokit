/* 
*   NatCorder
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace NatML.Examples.UI {

	using System.Collections;
	using UnityEngine;
	using UnityEngine.UI;
	using UnityEngine.Events;
	using UnityEngine.EventSystems;

    /// <summary>
    /// </summary>
    [RequireComponent(typeof(Image)), AddComponentMenu(@""), DisallowMultipleComponent]
    public sealed class VideoKitRecordButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler { // INCOMPLETE // Doc

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
        /// Event raised when the button is pressed.
        /// </summary>
        [Tooltip(@"Event raised when the button is pressed.")]
        public UnityEvent OnTouchDown;

        /// <summary>
        /// Event raised when the button is released.
        /// </summary>
        [Tooltip(@"Event raised when the button is released.")]
        public UnityEvent OnTouchUp;
        #endregion


        #region --Operations--
        private Image button;
        private bool touch;

        private void Awake () => button = GetComponent<Image>();

        private void Start () => Reset();

        private void Reset () {
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
            OnTouchDown?.Invoke();
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
            Reset();
            OnTouchUp?.Invoke();
        }

        void IPointerDownHandler.OnPointerDown (PointerEventData eventData) => StartCoroutine(Countdown());

        void IPointerUpHandler.OnPointerUp (PointerEventData eventData) => touch = false;
        #endregion
    }
}