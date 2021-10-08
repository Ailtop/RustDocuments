using System.Collections.Generic;
using UnityEngine;

public class DungeonGridInfo : LandmarkInfo
{
	[Header("DungeonGridInfo")]
	public int CellSize = 216;

	public float LinkHeight = 1.5f;

	public float LinkRadius = 3f;

	internal List<GameObject> Links = new List<GameObject>();

	public float Distance(Vector3 position)
	{
		return (base.transform.position - position).magnitude;
	}

	public float SqrDistance(Vector3 position)
	{
		return (base.transform.position - position).sqrMagnitude;
	}

	public bool IsValidSpawnPosition(Vector3 position)
	{
		OBB bounds = GetComponentInChildren<DungeonVolume>().GetBounds(position, Quaternion.identity);
		Vector3 vector = WorldSpaceGrid.ClosestGridCell(bounds.position, TerrainMeta.Size.x * 2f, CellSize);
		Vector3 vector2 = bounds.position - vector;
		if (!(Mathf.Abs(vector2.x) > 3f))
		{
			return Mathf.Abs(vector2.z) > 3f;
		}
		return true;
	}

	public Vector3 SnapPosition(Vector3 pos)
	{
		pos.x = (float)Mathf.RoundToInt(pos.x / LinkRadius) * LinkRadius;
		pos.y = (float)Mathf.CeilToInt(pos.y / LinkHeight) * LinkHeight;
		pos.z = (float)Mathf.RoundToInt(pos.z / LinkRadius) * LinkRadius;
		return pos;
	}

	protected override void Awake()
	{
		base.Awake();
		if ((bool)TerrainMeta.Path)
		{
			TerrainMeta.Path.DungeonGridEntrances.Add(this);
		}
	}

	protected void Start()
	{
		TransformEx.SetHierarchyGroup(base.transform, "Dungeon");
	}
}
