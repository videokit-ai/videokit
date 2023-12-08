//
//  VKTMediaDevice.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 1/3/2020.
//  Copyright © 2023 NatML Inc. All rights reserved.
//

@import AVFoundation;
#include <VideoKit/VideoKit.h>

@protocol VKTMediaDevice;

typedef void (^VKTSampleBufferBlock) (id _Nonnull sampleBuffer);
typedef void (^VKTDeviceDisconnectBlock) (id<VKTMediaDevice> _Nonnull device);

@protocol VKTMediaDevice <NSObject>
@required
@property (readonly, nonnull) NSString* uniqueID;
@property (readonly, nonnull) NSString* name;
@property (readonly) VKTDeviceFlags flags;
@property (readonly) bool running;
@property (nullable) VKTDeviceDisconnectBlock disconnectHandler;
- (void) startRunning:(nonnull VKTSampleBufferBlock) sampleBufferHandler;
- (void) stopRunning;
@end
