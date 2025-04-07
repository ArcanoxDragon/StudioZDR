﻿using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace StudioZDR.TegraSwizzle;

[PublicAPI]
[StructLayout(LayoutKind.Sequential)]
public struct BlockDim
{
	public uint width;
	public uint height;
	public uint depth;
}