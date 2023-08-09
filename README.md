# VideoKit
VideoKit is the only full feature user-generated content solution for Unity Engine. VideoKit allows:

- **Video Recording**. Record MP4 videos, animated GIF images, audio files, and more in as little as zero lines of code. Simply drop a component in your scene, and setup buttons to start and stop recording.

- **Interactive Video Effects**. Build TikTok and Snapchat-style video effects which leverage hardware machine learning, including color grading, human segmentation, face filters, and much more.

- **Seamless Video Editing**. Create video editing user flows with support for slicing videos, combining videos, extracting thumbnails, and more.

- **Extensive Camera Control**. Build full-featured camera apps with focus controls, flash, manual exposure, white balance, and more.

- **Audio Captioning**. Add audio-based user experiences in your application with audio captioning (speech-to-text).

- **Text Commands**. Convert any natural language prompt to a `struct` for building text and voice command functionality.

- **Social Sharing**. Power your app's virality engine by enabling your users to share their user-generated content in your app.

- **Gallery Picking**. Pick images and videos from the camera roll to build highly personalized content flows.

## Installing VideoKit
Add the following items to your Unity project's `Packages/manifest.json`:
```json
{
  "scopedRegistries": [
    {
      "name": "NatML",
      "url": "https://registry.npmjs.com",
      "scopes": ["ai.natml", "ai.fxn"]
    }
  ],
  "dependencies": {
    "ai.natml.videokit": "0.0.14",
  }
}
```

> VideoKit is still in alpha. As such, behaviours are expected to change more drastically between releases.

## Retrieving your Access Key
To use VideoKit, you will need to generate an access key. First, head over to [hub.natml.ai](https://hub.natml.ai/account/developers) to create an account by logging in. Once you do, generate an access key:

![generating an access key](https://raw.githubusercontent.com/natmlx/videokit/main/Media/create-access-key.gif)

Then add the key to your Unity project in `Project Settings > VideoKit`:

![set the access key](https://raw.githubusercontent.com/natmlx/videokit/main/Media/set-access-key.gif)

## Using VideoKit
Here are a few things you can do with VideoKit:

### Social Sharing
Share images, audio, and video files with the native share sheet with the `MediaAsset.Share` method:
```csharp
Texture2D image = ...
ImageAsset asset = await MediaAsset.FromTexture(image);
string receiverAppId = await asset.Share();
```

### Saving to the Camera Roll
Save images and videos to the camera roll with the `MediaAsset.SaveToCameraRoll` method:
```csharp
Texture2D image = ...
ImageAsset asset = await MediaAsset.FromTexture(image);
bool saved = await asset.Share();
```

### Picking from the Camera Roll
Pick images and videos from the camera roll with the `MediaAsset.FromCameraRoll<T>` method:
```csharp
// This will present the native gallery UI
var asset = await MediaAsset.FromCameraRoll<ImageAsset>() as ImageAsset;
// Do stuff with the asset...
```

### Camera Streaming
Stream the camera preview with the `VideoKitCameraManager` component:

![stream the camera preview](https://raw.githubusercontent.com/natmlx/videokit/main/Media/camera-streaming.gif)

> Camera streaming requires an active [VideoKit Core](https://hub.natml.ai/account/billing) subscription.

### Record Videos
Record MP4, HEVC, WEBM videos; animated GIF images; JPEG image sequences; and WAV audio files with the `VideoKitRecorder` component:

![recording a video](https://raw.githubusercontent.com/natmlx/videokit/main/Media/video-recording.gif)

> Video recording requires an active [VideoKit Core](https://hub.natml.ai/account/billing) subscription. But you can record `MP4` videos that are downsized and watermarked without a subscription.

### Human Texture
Remove the background from the camera preview with the `VideoKitCameraManager` component:

![using the human texture](https://raw.githubusercontent.com/natmlx/videokit/main/Media/human-texture.gif)

> Human texture requires an active [VideoKit AI](https://hub.natml.ai/account/billing) subscription.

### Speech-to-Text
Caption audio with the `AudioAsset.Caption` method:
```csharp
AudioClip clip = ...;
var asset = await MediaAsset.FromAudioClip(clip);
var caption = await asset.Caption();
Debug.Log(caption);
```

> Audio captioning requires an active [VideoKit AI](https://hub.natml.ai/account/billing) subscription.

### Text Commands
Convert a natural language prompt into a `struct` with the `TextAsset.To<T>` method. This enables features like text commands, and can be combined with audio captioning for voice control:
```csharp
using System.ComponentModel; // for `DescriptionAttribute`
using VideoKit.Assets;

struct Command { // Define this however you want

    [Description(@"The user's name")]
    public string name;

    [Description(@"The user's age")]
    public int age;
}

async void ParseCommand () {
    var prompt = "My name is Jake and I'm thirteen years old.";
    var asset = await MediaAsset.FromText(prompt);
    var command = await asset.To<Command>();
    // command = { "name": "Jake", "age": 13 }
}
```

> Text commands can be used without a subscription, but will require an active [VideoKit AI](https://hub.natml.ai/account/billing) subscription in a later VideoKit release.

___

## Requirements
- Unity 2022.3+

## Supported Platforms
- Android API Level 24+
- iOS 13+
- macOS 11+ (Apple Silicon and Intel)
- Windows 10+ (64-bit only)
- WebGL:
  - Chrome 91+
  - Firefox 90+
  - Safari 16.4+

## Resources
- Join the [NatML community on Discord](https://natml.ai/community).
- See the [VideoKit documentation](https://docs.natml.ai/videokit).
- Check out [NatML on GitHub](https://github.com/natmlx).
- Contact us at [hi@natml.ai](mailto:hi@natml.ai).

Thank you very much!