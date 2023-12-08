/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

using System.Reflection;
using System.Runtime.CompilerServices;
using VideoKit.Internal;

// Metadata
[assembly: AssemblyCompany(@"NatML Inc")]
[assembly: AssemblyTitle(@"VideoKit")]
[assembly: AssemblyVersionAttribute(VideoKitClient.Version)]
[assembly: AssemblyCopyright(@"Copyright © 2023 NatML Inc. All Rights Reserved.")]

// Friends
[assembly: InternalsVisibleTo(@"VideoKit.Editor")]
[assembly: InternalsVisibleTo(@"VideoKit.Tests")]