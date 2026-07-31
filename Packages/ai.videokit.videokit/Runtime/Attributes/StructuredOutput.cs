/* 
*   VideoKit
*   Copyright © 2026 Yusuf Olokoba. All Rights Reserved.
*/

#nullable enable

namespace VideoKit {

    using System;

    /// <summary>
    /// Mark a struct or class type as a structured output.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class StructuredOutputAttribute : Attribute { }
}