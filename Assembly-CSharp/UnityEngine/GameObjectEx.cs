using System.Collections.Generic;
using Facepunch;
using Rust;
using Rust.Registry;

namespace UnityEngine;

public static class GameObjectEx
{
	public static BaseEntity ToBaseEntity(this GameObject go)
	{
		return ToBaseEntity(go.transform);
	}

	public static BaseEntity ToBaseEntity(this Collider collider)
	{
		return ToBaseEntity(collider.transform);
	}

	public static BaseEntity ToBaseEntity(this Transform transform)
	{
		IEntity entity = GetEntityFromRegistry(transform);
		if (entity == null && !transform.gameObject.activeInHierarchy)
		{
			entity = GetEntityFromComponent(transform);
		}
		return entity as BaseEntity;
	}

	public static bool IsOnLayer(this GameObject go, Layer rustLayer)
	{
		return IsOnLayer(go, (int)rustLayer);
	}

	public static bool IsOnLayer(this GameObject go, int layer)
	{
		if (go != null)
		{
			return go.layer == layer;
		}
		return false;
	}

	private static IEntity GetEntityFromRegistry(Transform transform)
	{
		Transform transform2 = transform;
		IEntity entity = Entity.Get(transform2);
		while (entity == null && transform2.parent != null)
		{
			transform2 = transform2.parent;
			entity = Entity.Get(transform2);
		}
		if (entity != null && !entity.IsDestroyed)
		{
			return entity;
		}
		return null;
	}

	private static IEntity GetEntityFromComponent(Transform transform)
	{
		Transform transform2 = transform;
		IEntity component = transform2.GetComponent<IEntity>();
		while (component == null && transform2.parent != null)
		{
			transform2 = transform2.parent;
			component = transform2.GetComponent<IEntity>();
		}
		if (component != null && !component.IsDestroyed)
		{
			return component;
		}
		return null;
	}

	public static void SetHierarchyGroup(this GameObject obj, string strRoot, bool groupActive = true, bool persistant = false)
	{
		obj.transform.SetParent(HierarchyUtil.GetRoot(strRoot, groupActive, persistant).transform, worldPositionStays: true);
	}

	public static bool HasComponent<T>(this GameObject obj) where T : Component
	{
		return (Object)obj.GetComponent<T>() != (Object)null;
	}

	public static void SetChildComponentsEnabled<T>(this GameObject gameObject, bool enabled) where T : MonoBehaviour
	{
		List<T> obj = Pool.GetList<T>();
		gameObject.GetComponentsInChildren(includeInactive: true, obj);
		foreach (T item in obj)
		{
			item.enabled = enabled;
		}
		Pool.FreeList(ref obj);
	}
}
