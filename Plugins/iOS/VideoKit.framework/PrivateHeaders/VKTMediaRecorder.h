//
//  VKTNativeRecorder.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 6/26/2018.
//  Copyright © 2023 NatML Inc. All rights reserved.
//

@import AVFoundation;

typedef void (^VKTRecordingCompletionBlock) (NSString* _Nullable url);

@protocol VKTMediaRecorder <NSObject>
@property (readonly) CMVideoDimensions frameSize;
- (void) commitFrame:(nonnull CVPixelBufferRef) pixelBuffer timestamp:(int64_t) timestamp;
- (void) commitSamples:(nonnull const float*) sampleBuffer sampleCount:(int) sampleCount timestamp:(int64_t) timestamp;
- (void) finishWritingWithCompletionHandler:(nonnull VKTRecordingCompletionBlock) completionHandler;
@end
