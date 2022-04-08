namespace UnityEngine;

public static class RayEx
{
	public static Vector3 ClosestPoint(this Ray ray, Vector3 pos)
	{
		return ray.origin + Vector3.Dot(pos - ray.origin, ray.direction) * ray.direction;
	}

	public static float Distance(this Ray ray, Vector3 pos)
	{
		return Vector3.Cross(ray.direction, pos - ray.origin).magnitude;
	}

	public static float SqrDistance(this Ray ray, Vector3 pos)
	{
		return Vector3.Cross(ray.direction, pos - ray.origin).sqrMagnitude;
	}

	public static bool IsNaNOrInfinity(this Ray r)
	{
		if (!r.origin.IsNaNOrInfinity())
		{
			return r.direction.IsNaNOrInfinity();
		}
		return true;
	}
}
