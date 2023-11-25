using UnityEngine;

namespace VacuumBreather;

public static class QuaternionExtensions
{
	public static Quaternion Multiply(this Quaternion quaternion, float scalar)
	{
		return new Quaternion((float)((double)quaternion.x * (double)scalar), (float)((double)quaternion.y * (double)scalar), (float)((double)quaternion.z * (double)scalar), (float)((double)quaternion.w * (double)scalar));
	}

	public static Quaternion RequiredRotation(Quaternion from, Quaternion to)
	{
		Quaternion result = to * Quaternion.Inverse(from);
		if (result.w < 0f)
		{
			result.x *= -1f;
			result.y *= -1f;
			result.z *= -1f;
			result.w *= -1f;
		}
		return result;
	}

	public static Quaternion Subtract(this Quaternion lhs, Quaternion rhs)
	{
		return new Quaternion((float)((double)lhs.x - (double)rhs.x), (float)((double)lhs.y - (double)rhs.y), (float)((double)lhs.z - (double)rhs.z), (float)((double)lhs.w - (double)rhs.w));
	}
}
