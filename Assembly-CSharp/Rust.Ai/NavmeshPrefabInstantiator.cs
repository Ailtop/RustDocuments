using UnityEngine;

namespace Rust.Ai;

public class NavmeshPrefabInstantiator : MonoBehaviour
{
	public GameObjectRef NavmeshPrefab;

	private void Start()
	{
		if (NavmeshPrefab != null)
		{
			NavmeshPrefab.Instantiate(base.transform).SetActive(value: true);
			Object.Destroy(this);
		}
	}
}
