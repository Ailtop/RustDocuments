using UnityEngine;

public class DungeonCell : MonoBehaviour
{
	public DungeonConnectionType North;

	public DungeonConnectionType South;

	public DungeonConnectionType West;

	public DungeonConnectionType East;

	public DungeonConnectionVariant NorthVariant;

	public DungeonConnectionVariant SouthVariant;

	public DungeonConnectionVariant WestVariant;

	public DungeonConnectionVariant EastVariant;

	public GameObjectRef[] AvoidNeighbours;

	public MeshRenderer[] MapRenderers;

	public bool ShouldAvoid(uint id)
	{
		GameObjectRef[] avoidNeighbours = AvoidNeighbours;
		for (int i = 0; i < avoidNeighbours.Length; i++)
		{
			if (avoidNeighbours[i].resourceID == id)
			{
				return true;
			}
		}
		return false;
	}

	protected void Awake()
	{
		if ((bool)TerrainMeta.Path)
		{
			TerrainMeta.Path.DungeonCells.Add(this);
		}
	}
}
