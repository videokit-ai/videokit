//
//  VKTStatus.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 3/05/2024.
//  Copyright © 2024 Yusuf Olokoba. All rights reserved.
//

#pragma once

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