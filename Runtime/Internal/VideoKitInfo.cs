/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

using System.Reflection;
using System.Runtime.CompilerServices;
using NatML.VideoKit.Internal;

// Metadata
[assembly: AssemblyCompany(@"NatML Inc")]
[assembly: AssemblyTitle(@"VideoKit")]
[assembly: AssemblyVersionAttribute(VideoKitSettings.Version)]
[assembly: AssemblyCopyright(@"Copyright © 2023 NatML Inc. All Rights Reserved.")]

// Friends
[assembly: InternalsVisibleTo(@"NatML.VideoKit.Editor")]
[assembly: InternalsVisibleTo(@"NatML.VideoKit.Tests")]