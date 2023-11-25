using System.Runtime.InteropServices;

namespace Instancing;

[StructLayout(LayoutKind.Explicit)]
public struct DrawCallJobData
{
	[FieldOffset(0)]
	public int DrawCallIndex;

	[FieldOffset(4)]
	public int RendererIndex;

	[FieldOffset(8)]
	public uint IndexCount;

	[FieldOffset(12)]
	public uint IndexStart;

	[FieldOffset(16)]
	public uint VertexStart;

	[FieldOffset(20)]
	public uint MultidrawIndexStart;

	[FieldOffset(24)]
	public uint MultidrawVertexStart;

	[FieldOffset(28)]
	public int Padding1;
}
