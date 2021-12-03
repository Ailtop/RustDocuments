using System.Collections.Generic;
using UnityEngine;

public static class Vis
{
	private static int colCount = 0;

	public static Collider[] colBuffer = new Collider[8192];

	private static void Buffer(Vector3 position, float radius, int layerMask = -1, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide)
	{
		layerMask = GamePhysics.HandleTerrainCollision(position, layerMask);
		int num = colCount;
		colCount = Physics.OverlapSphereNonAlloc(position, radius, colBuffer, layerMask, triggerInteraction);
		for (int i = colCount; i < num; i++)
		{
			colBuffer[i] = null;
		}
		if (colCount >= colBuffer.Length)
		{
			Debug.LogWarning("Vis query is exceeding collider buffer length.");
			colCount = colBuffer.Length;
		}
	}

	public static bool AnyColliders(Vector3 position, float radius, int layerMask = -1, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
	{
		Buffer(position, radius, layerMask, triggerInteraction);
		return colCount > 0;
	}

	public static void Colliders<T>(Vector3 position, float radius, List<T> list, int layerMask = -1, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide) where T : Collider
	{
		Buffer(position, radius, layerMask, triggerInteraction);
		for (int i = 0; i < colCount; i++)
		{
			T val = colBuffer[i] as T;
			if (!((Object)val == (Object)null) && val.enabled)
			{
				list.Add(val);
			}
		}
	}

	public static void Components<T>(Vector3 position, float radius, List<T> list, int layerMask = -1, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide) where T : Component
	{
		Buffer(position, radius, layerMask, triggerInteraction);
		for (int i = 0; i < colCount; i++)
		{
			Collider collider = colBuffer[i];
			if (!(collider == null) && collider.enabled)
			{
				T component = collider.GetComponent<T>();
				if (!((Object)component == (Object)null))
				{
					list.Add(component);
				}
			}
		}
	}

	public static void Entities<T>(Vector3 position, float radius, List<T> list, int layerMask = -1, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide) where T : class
	{
		Buffer(position, radius, layerMask, triggerInteraction);
		for (int i = 0; i < colCount; i++)
		{
			Collider collider = colBuffer[i];
			if (!(collider == null) && collider.enabled)
			{
				T val = GameObjectEx.ToBaseEntity(collider) as T;
				if (val != null)
				{
					list.Add(val);
				}
			}
		}
	}

	public static void EntityComponents<T>(Vector3 position, float radius, List<T> list, int layerMask = -1, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide) where T : EntityComponentBase
	{
		Buffer(position, radius, layerMask, triggerInteraction);
		for (int i = 0; i < colCount; i++)
		{
			Collider collider = colBuffer[i];
			if (collider == null || !collider.enabled)
			{
				continue;
			}
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(collider);
			if (!(baseEntity == null))
			{
				T component = baseEntity.GetComponent<T>();
				if (!((Object)component == (Object)null))
				{
					list.Add(component);
				}
			}
		}
	}

	private static void Buffer(OBB bounds, int layerMask = -1, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide)
	{
		layerMask = GamePhysics.HandleTerrainCollision(bounds.position, layerMask);
		int num = colCount;
		colCount = Physics.OverlapBoxNonAlloc(bounds.position, bounds.extents, colBuffer, bounds.rotation, layerMask, triggerInteraction);
		for (int i = colCount; i < num; i++)
		{
			colBuffer[i] = null;
		}
		if (colCount >= colBuffer.Length)
		{
			Debug.LogWarning("Vis query is exceeding collider buffer length.");
			colCount = colBuffer.Length;
		}
	}

	public static void Colliders<T>(OBB bounds, List<T> list, int layerMask = -1, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide) where T : Collider
	{
		Buffer(bounds, layerMask, triggerInteraction);
		for (int i = 0; i < colCount; i++)
		{
			T val = colBuffer[i] as T;
			if (!((Object)val == (Object)null) && val.enabled)
			{
				list.Add(val);
			}
		}
	}

	public static void Components<T>(OBB bounds, List<T> list, int layerMask = -1, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide) where T : Component
	{
		Buffer(bounds, layerMask, triggerInteraction);
		for (int i = 0; i < colCount; i++)
		{
			Collider collider = colBuffer[i];
			if (!(collider == null) && collider.enabled)
			{
				T component = collider.GetComponent<T>();
				if (!((Object)component == (Object)null))
				{
					list.Add(component);
				}
			}
		}
	}

	public static void Entities<T>(OBB bounds, List<T> list, int layerMask = -1, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide) where T : BaseEntity
	{
		Buffer(bounds, layerMask, triggerInteraction);
		for (int i = 0; i < colCount; i++)
		{
			Collider collider = colBuffer[i];
			if (!(collider == null) && collider.enabled)
			{
				T val = GameObjectEx.ToBaseEntity(collider) as T;
				if (!((Object)val == (Object)null))
				{
					list.Add(val);
				}
			}
		}
	}

	public static void EntityComponents<T>(OBB bounds, List<T> list, int layerMask = -1, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide) where T : EntityComponentBase
	{
		Buffer(bounds, layerMask, triggerInteraction);
		for (int i = 0; i < colCount; i++)
		{
			Collider collider = colBuffer[i];
			if (collider == null || !collider.enabled)
			{
				continue;
			}
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(collider);
			if (!(baseEntity == null))
			{
				T component = baseEntity.GetComponent<T>();
				if (!((Object)component == (Object)null))
				{
					list.Add(component);
				}
			}
		}
	}

	private static void Buffer(Vector3 startPosition, Vector3 endPosition, float radius, int layerMask = -1, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide)
	{
		layerMask = GamePhysics.HandleTerrainCollision(startPosition, layerMask);
		int num = colCount;
		colCount = Physics.OverlapCapsuleNonAlloc(startPosition, endPosition, radius, colBuffer, layerMask, triggerInteraction);
		for (int i = colCount; i < num; i++)
		{
			colBuffer[i] = null;
		}
		if (colCount >= colBuffer.Length)
		{
			Debug.LogWarning("Vis query is exceeding collider buffer length.");
			colCount = colBuffer.Length;
		}
	}

	public static void Entities<T>(Vector3 startPosition, Vector3 endPosition, float radius, List<T> list, int layerMask = -1, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide) where T : BaseEntity
	{
		Buffer(startPosition, endPosition, radius, layerMask, triggerInteraction);
		for (int i = 0; i < colCount; i++)
		{
			Collider collider = colBuffer[i];
			if (!(collider == null) && collider.enabled)
			{
				T val = GameObjectEx.ToBaseEntity(collider) as T;
				if (!((Object)val == (Object)null))
				{
					list.Add(val);
				}
			}
		}
	}
}
