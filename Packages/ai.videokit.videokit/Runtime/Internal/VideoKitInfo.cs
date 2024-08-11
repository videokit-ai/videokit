/* 
*   VideoKit
*   Copyright © 2024 Yusuf Olokoba. All Rights Reserved.
*/

using System.Reflection;
using System.Runtime.CompilerServices;
using VideoKit.Internal;

// Metadata
[assembly: AssemblyCompany(@"Yusuf Olokoba")]
[assembly: AssemblyTitle(@"VideoKit")]
[assembly: AssemblyVersion(VideoKitClient.Version)]
[assembly: AssemblyCopyright(@"Copyright © 2024 Yusuf Olokoba. All Rights Reserved.")]

// Friends
[assembly: InternalsVisibleTo(@"VideoKit.Editor")]
[assembly: InternalsVisibleTo(@"VideoKit.Tests.Editor")]
[assembly: InternalsVisibleTo(@"VideoKit.Tests.Runtime")]