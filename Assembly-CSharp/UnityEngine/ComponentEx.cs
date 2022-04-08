using Facepunch;

namespace UnityEngine;

public static class ComponentEx
{
	public static T Instantiate<T>(this T component) where T : Component
	{
		return Facepunch.Instantiate.GameObject(component.gameObject).GetComponent<T>();
	}

	public static bool HasComponent<T>(this Component component) where T : Component
	{
		return (Object)component.GetComponent<T>() != (Object)null;
	}
}
