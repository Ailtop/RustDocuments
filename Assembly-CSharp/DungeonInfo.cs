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

	protected override void Awake()
	{
		Vector3 eulerAngles = base.transform.rotation.eulerAngles;
		Vector3 position = base.transform.position;
		eulerAngles.x = 0f;
		eulerAngles.y = (float)Mathf.RoundToInt(eulerAngles.y / 90f) * 90f;
		eulerAngles.z = 0f;
		position.x = (float)Mathf.RoundToInt(position.x / LinkRadius) * LinkRadius;
		position.y = (float)Mathf.CeilToInt(position.y / LinkHeight) * LinkHeight;
		position.z = (float)Mathf.RoundToInt(position.z / LinkRadius) * LinkRadius;
		base.transform.SetPositionAndRotation(position, Quaternion.Euler(eulerAngles));
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
