using UnityEngine;

public class SceneToPrefabTag : MonoBehaviour, IEditorComponent
{
	public enum TagType
	{
		ForceInclude = 0,
		ForceExclude = 1,
		SingleMaterial = 2,
		UseSpecificLOD = 3
	}

	public TagType Type;

	public int SpecificLOD;
}
