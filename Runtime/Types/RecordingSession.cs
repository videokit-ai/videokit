/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace NatML.VideoKit {

    using System;

    public sealed partial class VideoKitRecorder {

        #region --Types--
        /// <summary>
        /// Recording session.
        /// </summary>
        public readonly struct RecordingSession {

            #region --Client API--
            /// <summary>
            /// Recording path.
            /// This is only populated when:
            /// 1. The session completed successfully.
            /// 2. The recorder `destination` includes `Destination.Documents`.
            /// </summary>
            public readonly string path;

            /// <summary>
            /// Recording exception.
            /// This is `null` if the session completed successfully.
            /// </summary>
            public readonly Exception exception;

            /// <summary>
            /// Bundle ID of app that received recording from user sharing action.
            /// This is only populated when:
            /// 1. The session completed successfully.
            /// 2. The recorder `destination` includes `Destination.PromptUser`.
            /// 3. The user completed the native sharing action (i.e. did not cancel).
            /// </summary>
            public readonly string receiverApp;

            /// <summary>
            /// Whether the video successfully saved to the camera roll.
            /// </summary>
            public readonly bool savedToCameraRoll;
            #endregion


            #region --Operations--

            internal RecordingSession (string path, Exception exception, string receiver, bool cameraRoll) {
                this.path = path;
                this.exception = exception;
                this.receiverApp = receiver;
                this.savedToCameraRoll = cameraRoll;
            }
            #endregion
        }
        #endregion
    }
}