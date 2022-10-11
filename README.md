# VideoKit
VideoKit is the only user-generated content solution for Unity Engine. VideoKit allows:

- **Video Recording**, with support for recording MP4 videos, animated GIF images, audio files, and more.

- **Video Filters**, with support for color grading, human segmentation, face filters, and more.

- **Video Editing**, with support for slicing videos, combining videos, extracting thumbnails, and more.

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
    "ai.natml.videokit": "0.0.1",
    ...
  }
}
```

___

## Requirements
- Unity 2021.2+

## Supported Platforms
- Android API Level 24+
- iOS 13+
- macOS 10.15+ (Apple Silicon and Intel)
- Windows 10+ (64-bit only)
- WebGL:
  - Chrome 91+
  - Firefox 90+

## Resources
- Join the [NatML community on Discord](https://hub.natml.ai/community).
- See the [VideoKit documentation](https://docs.natml.ai/videokit).
- Check out [NatML on GitHub](https://github.com/natmlx).
- Read the [NatML blog](https://blog.natml.ai/).
- Contact us at [hi@natml.ai](mailto:hi@natml.ai).

Thank you very much!