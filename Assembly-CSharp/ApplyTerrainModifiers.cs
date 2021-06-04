using UnityEngine;

public class ApplyTerrainModifiers : MonoBehaviour
{
	protected void Awake()
	{
		BaseEntity component = GetComponent<BaseEntity>();
		TerrainModifier[] modifiers = null;
		if (component.isServer)
		{
			modifiers = PrefabAttribute.server.FindAll<TerrainModifier>(component.prefabID);
		}
		TerrainModifierEx.ApplyTerrainModifiers(base.transform, modifiers);
		GameManager.Destroy(this);
	}
}
