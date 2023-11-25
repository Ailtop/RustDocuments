using System.Runtime.InteropServices;

namespace Instancing;

[StructLayout(LayoutKind.Explicit)]
public struct RenderSlice
{
	[FieldOffset(0)]
	public uint StartIndex;

	[FieldOffset(4)]
	public uint Length;
}
