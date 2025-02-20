//
//  VKTStatus.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 3/05/2024.
//  Copyright © 2025 Yusuf Olokoba. All rights reserved.
//

#pragma once

#ifdef __cplusplus
    #ifdef _WIN64
        #define VKT_API extern "C" __declspec(dllexport)
    #else
        #define VKT_API extern "C"
    #endif
#else
    #define VKT_API extern
#endif

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
 */
enum VKTStatus {
    VKT_OK                       = 0,
    VKT_ERROR_INVALID_ARGUMENT   = 1,
    VKT_ERROR_INVALID_OPERATION  = 2,
    VKT_ERROR_NOT_IMPLEMENTED    = 3,
    VKT_ERROR_INVALID_SESSION    = 101,
    VKT_ERROR_INVALID_PLAN       = 104,
};
typedef enum VKTStatus VKTStatus;
#pragma endregion
