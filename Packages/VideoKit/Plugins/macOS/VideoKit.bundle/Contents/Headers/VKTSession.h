//
//  VKTSession.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 5/15/2023.
//  Copyright © 2024 NatML Inc. All rights reserved.
//

#pragma once

#include <stdint.h>
#include <VideoKit/VKTAPI.h>
#include <VideoKit/VKTStatus.h>

#pragma region --Client API--
/*!
 @function VKTSessionGetIdentifier
 
 @abstract Get the session identifier for generating a session token.
 
 @discussion Get the session identifier for generating a session token.
 
 @param identifier
 Destination string.

 @param size
 Destination buffer size.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTSessionGetIdentifier (
    char * identifier,
    int32_t size
);

/*!
 @function VKTSessionGetStatus
 
 @abstract Get the VideoKit session status.
 
 @discussion Get the VideoKit session status.

 @returns Session status.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTSessionGetStatus (void);

/*!
 @function VKTSessionSetToken
 
 @abstract Set the VideoKit session token.
 
 @discussion Set the VideoKit session token.
 
 @param token
 VideoKit session token.

 @returns Session status.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTSessionSetToken (const char* token);
#pragma endregion