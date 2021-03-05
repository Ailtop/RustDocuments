using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Vehicles/WorldSpline Shared Data", fileName = "WorldSpline Prefab Shared Data")]
public class WorldSplineSharedData : ScriptableObject
{
	[SerializeField]
	private List<WorldSplineData> dataList;

	public static WorldSplineSharedData instance;

	private static string[] worldSplineFolders = new string[2]
	{
		"Assets/Content/Structures",
		"Assets/bundled/Prefabs/autospawn"
	};

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		instance = Resources.Load<WorldSplineSharedData>("WorldSpline Prefab Shared Data");
	}

	public static WorldSplineData GetDataFor(WorldSpline worldSpline)
	{
		if (instance == null)
		{
			Debug.LogError("No instance of WorldSplineSharedData found.");
			return null;
		}
		if (worldSpline.dataIndex < 0 || worldSpline.dataIndex >= instance.dataList.Count)
		{
			Debug.LogError($"Data index out of range ({worldSpline.dataIndex}/{instance.dataList.Count}) for world spline: {worldSpline.name}", worldSpline.gameObject);
			return null;
		}
		return instance.dataList[worldSpline.dataIndex];
	}
}
