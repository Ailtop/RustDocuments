using UnityEngine;

public struct FixedSByteNorm3
{
	private const int FracBits = 7;

	private const float MaxFrac = 128f;

	private const float RcpMaxFrac = 0.0078125f;

	public sbyte x;

	public sbyte y;

	public sbyte z;

	public FixedSByteNorm3(Vector3 vec)
	{
		x = (sbyte)(vec.x * 128f);
		y = (sbyte)(vec.y * 128f);
		z = (sbyte)(vec.z * 128f);
	}

	public static explicit operator Vector3(FixedSByteNorm3 vec)
	{
		return new Vector3((float)vec.x * 0.0078125f, (float)vec.y * 0.0078125f, (float)vec.z * 0.0078125f);
	}
}
