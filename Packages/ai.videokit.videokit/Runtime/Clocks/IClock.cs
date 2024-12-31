/* 
*   VideoKit
*   Copyright © 2025 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit.Clocks {

    /// <summary>
    /// Clock for generating recording timestamps.
    /// Clocks are important for synchronizing audio and video tracks when recording with audio.
    /// Clocks are thread-safe, so they can be used on multiple threads simultaneously.
    /// Clocks timestamps should always start from zero.
    /// </summary>
    public interface IClock {
        
        /// <summary>
        /// Current timestamp in nanoseconds.
        /// </summary>
        long timestamp { get; }
    }
}