# Slow Motion Capture Sample
This sample demonstrates recording ultra-high frame rate 
video (120FPS or 240FPS) from the camera to a video.

We can't use the `VideoKitRecorder` component for this because it 
records camera device pixel buffers from the `VideoKitCameraView` component. 
But the `VideoKitCameraView` component only updates based on the app frame rate, not the 
camera preview frame rate.

As a result, we register a listener on the `VideoKitCameraManager.OnPixelBuffer` event 
to get the raw camera device pixel buffers at the high frame rate. We will then 
convert these to `RGBA8888` using the `PixelBuffer.CopyTo` method and append them to the 
recorder.