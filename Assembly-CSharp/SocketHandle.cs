using System;
using UnityEngine;

public class SocketHandle : PrefabAttribute
{
	protected override Type GetIndexedType()
	{
		return typeof(SocketHandle);
	}

	internal void AdjustTarget(ref Construction.Target target, float maxplaceDistance)
	{
		Vector3 worldPosition = base.worldPosition;
		Vector3 a = target.ray.origin + target.ray.direction * maxplaceDistance - worldPosition;
		target.ray.direction = (a - target.ray.origin).normalized;
	}
}
