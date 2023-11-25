using Unity.Mathematics;
using UnityEngine;

namespace Rust.Water5;

public struct OceanDisplacementShort3
{
	private const float precision = 20f;

	private const float float2short = 32766f;

	private const float short2float = 3.051944E-05f;

	public short x;

	public short y;

	public short z;

	public static implicit operator Vector3(OceanDisplacementShort3 v)
	{
		Vector3 result = default(Vector3);
		result.x = 3.051944E-05f * (float)v.x * 20f;
		result.y = 3.051944E-05f * (float)v.y * 20f;
		result.z = 3.051944E-05f * (float)v.z * 20f;
		return result;
	}

	public static implicit operator OceanDisplacementShort3(Vector3 v)
	{
		OceanDisplacementShort3 result = default(OceanDisplacementShort3);
		result.x = (short)(v.x / 20f * 32766f + 0.5f);
		result.y = (short)(v.y / 20f * 32766f + 0.5f);
		result.z = (short)(v.z / 20f * 32766f + 0.5f);
		return result;
	}

	public static implicit operator OceanDisplacementShort3(float3 v)
	{
		OceanDisplacementShort3 result = default(OceanDisplacementShort3);
		result.x = (short)(v.x / 20f * 32766f + 0.5f);
		result.y = (short)(v.y / 20f * 32766f + 0.5f);
		result.z = (short)(v.z / 20f * 32766f + 0.5f);
		return result;
	}
}
