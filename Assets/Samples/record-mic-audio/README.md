# Record Microphone Audio
This sample records the micropohone audio using the `VideoKitRecorder` component, then plays the recorded audio back 
using an `AudioSource` component. The `RecordMicAudio` script defines a handler method, `OnRecordingCompleted`, which 
receives the recorded media asset. The `MediaAsset.ToAudioClip` method is then used to create an `AudioClip`. This 
audio clip can then be played back in Unity Engine.