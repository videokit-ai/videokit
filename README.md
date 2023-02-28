# VideoKit
VideoKit is the only full feature user-generated content solution for Unity Engine. VideoKit allows:

- **Low-code Video Recording**. Record MP4 videos, animated GIF images, audio files, and more in as little as zero lines of code. Simply drop a component in your scene, and setup buttons to start and stop recording.

- **Interactive Video Effectss**. Build TikTok and Snapchat-style video effects which leverage hardware machine learning, including color grading, human segmentation, face filters, and much more.

- **Seamless Video Editing**. Create video editing user flows with support for slicing videos, combining videos, extracting thumbnails, and more.

## Installing VideoKit
Add the following items to your Unity project's `Packages/manifest.json`:
```json
{
  "scopedRegistries": [
    {
      "name": "NatML",
      "url": "https://registry.npmjs.com",
      "scopes": ["ai.natml"]
    }
  ],
  "dependencies": {
    "ai.natml.videokit": "0.0.11",
  }
}
```

> Using VideoKit requires an active [NatML VideoKit](https://www.natml.ai/videokit) subscription. You can try it out for free, but functionality is limited. [See the docs](https://docs.natml.ai/videokit/prelims/faq) for more info.

> VideoKit is still in alpha. As such, behaviours are expected to change more drastically between releases.
___

## Requirements
- Unity 2021.2+

## Supported Platforms
- Android API Level 24+
- iOS 14+
- macOS 10.15+ (Apple Silicon and Intel)
- Windows 10+ (64-bit only)
- WebGL:
  - Chrome 91+
  - Firefox 90+

## Resources
- Join the [NatML community on Discord](https://natml.ai/community).
- See the [VideoKit documentation](https://docs.natml.ai/videokit).
- Check out [NatML on GitHub](https://github.com/natmlx).
- Contact us at [hi@natml.ai](mailto:hi@natml.ai).

Thank you very much!