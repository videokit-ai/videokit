//
//  VKTSession.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 5/15/2023.
//  Copyright © 2023 NatML Inc. All rights reserved.
//

#pragma once

#include <VideoKit/VKTTypes.h>

#pragma region --Enumerations--
/*!
 @enum VKTStatus

 @abstract VideoKit status codes.

 @constant VKT_OK
 Successful operation.

 @constant VKT_ERROR_INVALID_ARGUMENT
 Provided argument is invalid.

 @constant VKT_ERROR_INVALID_OPERATION
 Operation is invalid in current state.

 @constant VKT_ERROR_NOT_IMPLEMENTED
 Operation has not been implemented.

 @constant VKT_ERROR_INVALID_SESSION
 VideoKit session has not been set or is invalid.
 
 @constant VKT_ERROR_INVALID_PLAN
 Current VideoKit plan does not allow the operation.

 @constant VKT_WARNING_LIMITED_PLAN
 Current VideoKit plan only allows for limited functionality.
 */
enum VKTStatus {
    VKT_OK                       = 0,
    VKT_ERROR_INVALID_ARGUMENT   = 1,
    VKT_ERROR_INVALID_OPERATION  = 2,
    VKT_ERROR_NOT_IMPLEMENTED    = 3,
    VKT_ERROR_INVALID_SESSION    = 101,
    VKT_ERROR_INVALID_PLAN       = 104,
    VKT_WARNING_LIMITED_PLAN     = 105,
};
typedef enum VKTStatus VKTStatus;
#pragma endregion


#pragma region --Client API--
/*!
 @function VKTGetBundleIdentifier
 
 @abstract Get the application bundle ID for generating a session token.
 
 @discussion Get the application bundle ID for generating a session token.
 
 @param bundle
 Destination bundle ID string.

 @returns Operation status.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTGetBundleIdentifier (char * bundle);

/*!
 @function VKTGetSessionStatus
 
 @abstract Get the VideoKit session status.
 
 @discussion Get the VideoKit session status.

 @returns Session status.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTGetSessionStatus (void);

/*!
 @function VKTSetSessionToken
 
 @abstract Set the VideoKit session token.
 
 @discussion Set the VideoKit session token.
 
 @param token
 VideoKit session token.

 @returns Session status.
*/
VKT_BRIDGE VKT_EXPORT VKTStatus VKT_API VKTSetSessionToken (const char* token);
#pragma endregion
