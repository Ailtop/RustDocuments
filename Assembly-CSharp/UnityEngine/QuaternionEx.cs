namespace UnityEngine;

public static class QuaternionEx
{
	public static Quaternion AlignToNormal(this Quaternion rot, Vector3 normal)
	{
		return Quaternion.FromToRotation(Vector3.up, normal) * rot;
	}

	public static Quaternion LookRotationWithOffset(Vector3 offset, Vector3 forward, Vector3 up)
	{
		return Quaternion.LookRotation(forward, Vector3.up) * Quaternion.Inverse(Quaternion.LookRotation(offset, Vector3.up));
	}

	public static Quaternion LookRotationForcedUp(Vector3 forward, Vector3 up)
	{
		if (forward == up)
		{
			return Quaternion.LookRotation(up);
		}
		Vector3 rhs = Vector3.Cross(forward, up);
		forward = Vector3.Cross(up, rhs);
		return Quaternion.LookRotation(forward, up);
	}

	public static Quaternion LookRotationGradient(Vector3 normal, Vector3 up)
	{
		Vector3 rhs = ((normal == Vector3.up) ? Vector3.forward : Vector3.Cross(normal, Vector3.up));
		return LookRotationForcedUp(Vector3.Cross(normal, rhs), up);
	}

	public static Quaternion LookRotationNormal(Vector3 normal, Vector3 up = default(Vector3))
	{
		if (up != Vector3.zero)
		{
			return LookRotationForcedUp(up, normal);
		}
		if (normal == Vector3.up)
		{
			return LookRotationForcedUp(Vector3.forward, normal);
		}
		if (normal == Vector3.down)
		{
			return LookRotationForcedUp(Vector3.back, normal);
		}
		if (normal.y == 0f)
		{
			return LookRotationForcedUp(Vector3.up, normal);
		}
		Vector3 rhs = Vector3.Cross(normal, Vector3.up);
		return LookRotationForcedUp(-Vector3.Cross(normal, rhs), normal);
	}

	public static Quaternion EnsureValid(this Quaternion rot, float epsilon = float.Epsilon)
	{
		if (!(Quaternion.Dot(rot, rot) >= epsilon))
		{
			return Quaternion.identity;
		}
		return rot;
	}
}
