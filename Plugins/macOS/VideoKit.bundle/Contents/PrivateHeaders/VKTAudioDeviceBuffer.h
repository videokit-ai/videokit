//
//  VKTAudioDeviceBuffer.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 6/14/2023.
//  Copyright © 2023 NatML Inc. All rights reserved.
//

@import AVFoundation;

@interface VKTAudioDeviceBuffer : NSObject
@property (readonly, nonnull) CMSampleBufferRef sampleBuffer;
- (nonnull instancetype) initWithSampleBuffer:(nonnull CMSampleBufferRef) sampleBuffer;
@end

