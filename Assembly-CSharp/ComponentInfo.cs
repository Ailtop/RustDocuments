using UnityEngine;

public abstract class ComponentInfo<T> : ComponentInfo
{
	public T component;

	public void Initialize(T source)
	{
		component = source;
		Setup();
	}
}
public abstract class ComponentInfo : MonoBehaviour
{
	public abstract void Setup();

	public abstract void Reset();
}
