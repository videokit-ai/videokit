//
//  VKTStatus.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 3/05/2024.
//  Copyright © 2024 Yusuf Olokoba. All rights reserved.
//

#pragma once

#include <VideoKit/VKTAPI.h>

#define VKT_VERSION_MAJOR 0
#define VKT_VERSION_MINOR 0
#define VKT_VERSION_PATCH 22

/*!
 @function VKTGetVersion

 @abstract Get the VideoKit version.

 @discussion Get the VideoKit version.

 @returns VideoKit version string.
*/
VKT_BRIDGE VKT_EXPORT const char* VKT_API VKTGetVersion (void);
