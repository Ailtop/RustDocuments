using UnityEngine;

public struct CachedTransform<T> where T : Component
{
	public T component;

	public Vector3 position;

	public Quaternion rotation;

	public Vector3 localScale;

	public Matrix4x4 localToWorldMatrix => Matrix4x4.TRS(position, rotation, localScale);

	public Matrix4x4 worldToLocalMatrix => localToWorldMatrix.inverse;

	public Vector3 forward => rotation * Vector3.forward;

	public Vector3 up => rotation * Vector3.up;

	public Vector3 right => rotation * Vector3.right;

	public CachedTransform(T instance)
	{
		component = instance;
		if ((bool)(Object)component)
		{
			position = component.transform.position;
			rotation = component.transform.rotation;
			localScale = component.transform.localScale;
		}
		else
		{
			position = Vector3.zero;
			rotation = Quaternion.identity;
			localScale = Vector3.one;
		}
	}

	public void Apply()
	{
		if ((bool)(Object)component)
		{
			component.transform.SetPositionAndRotation(position, rotation);
			component.transform.localScale = localScale;
		}
	}

	public void RotateAround(Vector3 center, Vector3 axis, float angle)
	{
		Quaternion rhs = Quaternion.AngleAxis(angle, axis);
		Vector3 b = rhs * (position - center);
		position = center + b;
		rotation *= Quaternion.Inverse(rotation) * rhs * rotation;
	}

	public static implicit operator bool(CachedTransform<T> instance)
	{
		return (Object)instance.component != (Object)null;
	}
}
