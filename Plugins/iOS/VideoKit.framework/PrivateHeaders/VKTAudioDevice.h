//
//  VKTAudioDevice.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 6/2/2023.
//  Copyright © 2023 NatML Inc. All rights reserved.
//

#import "VKTMediaDevice.h"

@interface VKTAudioDevice : NSObject <VKTMediaDevice>
// Introspection
- (nonnull instancetype) initWithPort:(nonnull AVAudioSessionPortDescription*) port;
@property (readonly, nonnull) AVAudioSessionPortDescription* port;
// Settings
@property bool echoCancellation;
@property int sampleRate;
@property int channelCount;
@end
