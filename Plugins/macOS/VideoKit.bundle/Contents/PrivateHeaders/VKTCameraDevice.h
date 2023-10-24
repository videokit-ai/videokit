//
//  VKTCameraDevice.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 6/14/2023.
//  Copyright © 2023 NatML Inc. All rights reserved.
//

#import "VKTMediaDevice.h"

@interface VKTCameraDevice : NSObject <VKTMediaDevice>
// Introspection
- (nonnull instancetype) initWithDevice:(nonnull AVCaptureDevice*) device;
@property (readonly, nonnull) AVCaptureDevice* device;
// Settings
@property CGSize previewResolution;
@property int frameRate;
- (void) capturePhoto:(nonnull VKTSampleBufferBlock) photoBufferBlock;
@end
