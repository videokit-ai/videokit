/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Clocks {

    using System;
    using System.Runtime.CompilerServices;
    using Internal;
    using Status = VideoKit.Internal.VideoKit.Status;

    /// <summary>
    /// Realtime clock for generating timestamps.
    /// </summary>
    public sealed class RealtimeClock : IClock {

        #region --Client API--
        /// <summary>
        /// Current timestamp in nanoseconds.
        /// The very first value reported by this property will always be zero.
        /// </summary>
        public long timestamp {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => (isPaused ? pauseTime : CurrentTimestamp) - startTime;
        }

        /// <summary>
        /// Is the clock paused?
        /// </summary>
        public bool paused {
            [MethodImpl(MethodImplOptions.Synchronized)] get => isPaused;
            [MethodImpl(MethodImplOptions.Synchronized)]
            set {
                if (value == isPaused)
                    return;
                if (value)
                    pauseTime = CurrentTimestamp;
                else
                    startTime += CurrentTimestamp - pauseTime;
                isPaused = value;
            }
        }

        /// <summary>
        /// Create a realtime clock.
        /// </summary>
        public RealtimeClock () {
            this.startTime = CurrentTimestamp;
            this.isPaused = false;
            this.pauseTime = 0L;
        }
        #endregion


        #region --Operations--
        private long startTime;
        private bool isPaused;
        private long pauseTime;
        private static long CurrentTimestamp => VideoKit
            .GetHighResolutionTimestamp(out var timestamp)
            .Throw() == Status.Ok ? timestamp : 0L;
        #endregion
    }
}