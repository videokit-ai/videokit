//
//  VKTTypes.h
//  VideoKit
//
//  Created by Yusuf Olokoba on 5/15/2023.
//  Copyright © 2023 NatML Inc. All rights reserved.
//

#pragma once

#ifdef __cplusplus
    #define VKT_BRIDGE extern "C"
#else
    #define VKT_BRIDGE extern
#endif

#ifdef _WIN64
    #define VKT_EXPORT __declspec(dllexport)
#else
    #define VKT_EXPORT
#endif

#ifdef __EMSCRIPTEN__
    #define VKT_API EMSCRIPTEN_KEEPALIVE
#elif defined(_WIN64)
    #define VKT_API APIENTRY
#else
    #define VKT_API
#endif