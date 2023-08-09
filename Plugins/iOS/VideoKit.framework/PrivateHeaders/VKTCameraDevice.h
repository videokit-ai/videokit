//
//  VKTCameraDevice.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 6/2/2023.
//  Copyright © 2023 NatML Inc. All rights reserved.
//

#import "VKTMediaDevice.h"

@interface VKTCameraDevice : NSObject <VKTMediaDevice>
// Introspection
- (nonnull instancetype) initWithDevice:(nonnull AVCaptureDevice*) device;
@property (readonly, nonnull) AVCaptureDevice* device;
@property (readonly) CGSize fieldOfView;
// Settings
@property CGSize previewResolution;
@property CGSize photoResolution;
@property int frameRate;
@property VKTFlashMode flashMode;
@property VKTVideoStabilizationMode videoStabilizationMode;
- (void) capturePhoto:(nonnull VKTSampleBufferBlock) photoBufferBlock;
@end
