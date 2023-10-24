/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Utilities {

    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal sealed class SharedSignal {

        #region --Client API--
        /// <summary>
        /// Whether the shared signal has been triggered.
        /// </summary>
        public bool signaled {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get;
            private set;
        }

        /// <summary>
        /// Event raised when the shared signal is triggered.
        /// </summary>
        public event Action OnSignal;

        /// <summary>
        /// Create a shared signal.
        /// </summary>
        /// <param name="count">Number of unique signals required to trigger the shared signal.</param>
        public SharedSignal (int count) {
            this.record = new HashSet<object>();
            this.count = count;
        }

        /// <summary>
        /// Send a signal.
        /// </summary>
        /// <param name-"key">Key to identify signal.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Signal (object key) {
            if (signaled)
                return;
            record.Add(key);
            if (record.Count == count) {
                OnSignal?.Invoke();
                signaled = true;
            }
        }
        #endregion


        #region --Operations--
        private readonly int count;
        private readonly HashSet<object> record;
        #endregion
    }
}