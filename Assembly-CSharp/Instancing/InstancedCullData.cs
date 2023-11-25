using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace Instancing;

[StructLayout(LayoutKind.Explicit)]
public struct InstancedCullData
{
	[Flags]
	public enum BitFlags : uint
	{
		Visible = 1u,
		HasShadow = 2u,
		HasMesh = 4u,
		LastLOD = 8u
	}

	[FieldOffset(0)]
	public float3 CullPosition;

	[FieldOffset(12)]
	public float3 BoundsMin;

	[FieldOffset(24)]
	public float3 BoundsMax;

	[FieldOffset(36)]
	public float MinDistance;

	[FieldOffset(40)]
	public float MaxDistance;

	[FieldOffset(44)]
	public int RendererId;

	[FieldOffset(48)]
	public int SliceIndex;

	[FieldOffset(52)]
	public BitFlags Flags;

	[FieldOffset(56)]
	public long VirtualMeshId;

	public bool IsVisible
	{
		get
		{
			return (Flags & BitFlags.Visible) == BitFlags.Visible;
		}
		set
		{
			if (value)
			{
				Flags |= BitFlags.Visible;
			}
			else
			{
				Flags &= ~BitFlags.Visible;
			}
		}
	}

	public bool HasShadow
	{
		get
		{
			return (Flags & BitFlags.HasShadow) == BitFlags.HasShadow;
		}
		set
		{
			if (value)
			{
				Flags |= BitFlags.HasShadow;
			}
			else
			{
				Flags &= ~BitFlags.HasShadow;
			}
		}
	}

	public bool HasMesh
	{
		get
		{
			return (Flags & BitFlags.HasMesh) == BitFlags.HasMesh;
		}
		set
		{
			if (value)
			{
				Flags |= BitFlags.HasMesh;
			}
			else
			{
				Flags &= ~BitFlags.HasMesh;
			}
		}
	}

	public bool LastLOD
	{
		get
		{
			return (Flags & BitFlags.LastLOD) == BitFlags.LastLOD;
		}
		set
		{
			if (value)
			{
				Flags |= BitFlags.LastLOD;
			}
			else
			{
				Flags &= ~BitFlags.LastLOD;
			}
		}
	}
}
