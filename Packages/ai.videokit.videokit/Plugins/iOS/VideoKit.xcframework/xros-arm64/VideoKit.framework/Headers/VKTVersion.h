//
//  VKTStatus.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 3/05/2024.
//  Copyright © 2025 Yusuf Olokoba. All rights reserved.
//

#pragma once

#include <VideoKit/VKTStatus.h>

#define VKT_VERSION_MAJOR 1
#define VKT_VERSION_MINOR 0
#define VKT_VERSION_PATCH 0

/*!
 @function VKTGetVersion

 @abstract Get the VideoKit version.

 @discussion Get the VideoKit version.

 @returns VideoKit version string.
*/
VKT_API const char* VKTGetVersion (void);
