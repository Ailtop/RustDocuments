using System.Collections.Generic;
using UnityEngine;

public class DungeonInfo : LandmarkInfo
{
	[Header("DungeonInfo")]
	public int CellSize = 216;

	public float LinkHeight = 1.5f;

	public float LinkRadius = 3f;

	public const float LinkRotation = 90f;

	internal MonumentInfo Monument;

	internal List<GameObject> Links = new List<GameObject>();

	public float Distance(Vector3 position)
	{
		return (base.transform.position - position).magnitude;
	}

	public float SqrDistance(Vector3 position)
	{
		return (base.transform.position - position).sqrMagnitude;
	}

	public bool IsValidSpawnPosition(Vector3 position, Quaternion rotation)
	{
		Vector3 euler = SnapRotation(rotation.eulerAngles);
		Vector3 position2 = SnapPosition(position);
		OBB bounds = GetComponentInChildren<DungeonLinkBlockVolume>().GetBounds(position2, Quaternion.Euler(euler));
		Vector3 vector = WorldSpaceGrid.ClosestGridCell(bounds.position, TerrainMeta.Size.x * 2f, CellSize);
		Vector3 vector2 = bounds.position - vector;
		if (!(Mathf.Abs(vector2.x) > 3f))
		{
			return Mathf.Abs(vector2.z) > 3f;
		}
		return true;
	}

	private Vector3 SnapPosition(Vector3 pos)
	{
		pos.x = (float)Mathf.RoundToInt(pos.x / LinkRadius) * LinkRadius;
		pos.y = (float)Mathf.CeilToInt(pos.y / LinkHeight) * LinkHeight;
		pos.z = (float)Mathf.RoundToInt(pos.z / LinkRadius) * LinkRadius;
		return pos;
	}

	private Vector3 SnapRotation(Vector3 rot)
	{
		rot.x = 0f;
		rot.y = (float)Mathf.RoundToInt(rot.y / 90f) * 90f;
		rot.z = 0f;
		return rot;
	}

	protected override void Awake()
	{
		Vector3 euler = SnapRotation(base.transform.rotation.eulerAngles);
		Vector3 position = SnapPosition(base.transform.position);
		base.transform.SetPositionAndRotation(position, Quaternion.Euler(euler));
		base.Awake();
		Monument = base.transform.GetComponentInParent<MonumentInfo>();
		if ((bool)TerrainMeta.Path)
		{
			TerrainMeta.Path.DungeonEntrances.Add(this);
		}
	}

	protected void Start()
	{
		TransformEx.SetHierarchyGroup(base.transform, "Dungeon");
	}
}
