namespace UnityEngine;

public static class CollisionEx
{
	public static BaseEntity GetEntity(this Collision col)
	{
		return GameObjectEx.ToBaseEntity(col.transform);
	}
}
