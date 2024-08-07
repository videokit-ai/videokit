//
//  VKTClock.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 12/11/2023.
//  Copyright © 2024 Yusuf Olokoba. All rights reserved.
//

#pragma once

#include <stdint.h>
#include <VideoKit/VKTSession.h>

#pragma region --Realtime Clock--
/*!
 @function VKTClockGetHighResolutionTimestamp

 @abstract Get the high resolution timestamp.

 @discussion Get the high resolution timestamp.

 @param timestamp
 High resolution timestamp in nanoseconds.

 @returns Status.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTClockGetHighResolutionTimestamp (int64_t* timestamp);
#pragma endregion