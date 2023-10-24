//
//  VKTAudioDevice.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 6/14/2023.
//  Copyright © 2023 NatML Inc. All rights reserved.
//

#import "VKTMediaDevice.h"

@interface VKTAudioDevice : NSObject <VKTMediaDevice>
// Introspection
- (nonnull instancetype) initWithDevice:(nonnull AVCaptureDevice*) device;
@property (readonly, nonnull) AVCaptureDevice* device;
// Settings
@property int sampleRate;
@property int channelCount;
@end
