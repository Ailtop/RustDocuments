#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Network;
using Network.Visibility;
using Oxide.Core;
using Rust;
using UnityEngine;
using UnityEngine.Serialization;

public class NetworkVisibilityGrid : MonoBehaviour, Provider
{
	public const int overworldLayer = 0;

	public const int cavesLayer = 1;

	public const int tunnelsLayer = 2;

	public const int dynamicDungeonsFirstLayer = 10;

	public int startID = 1024;

	public int gridSize = 100;

	public int cellCount = 32;

	[FormerlySerializedAs("visibilityRadius")]
	public int visibilityRadiusFar = 2;

	public int visibilityRadiusNear = 1;

	public float switchTolerance = 20f;

	public float cavesThreshold = -5f;

	public float tunnelsThreshold = -50f;

	public float dynamicDungeonsThreshold = 1000f;

	public float dynamicDungeonsInterval = 100f;

	private float halfGridSize;

	private float cellSize;

	private float halfCellSize;

	private int numIDsPerLayer;

	private void Awake()
	{
		Debug.Assert(Network.Net.sv != null, "Network.Net.sv is NULL when creating Visibility Grid");
		Debug.Assert(Network.Net.sv.visibility == null, "Network.Net.sv.visibility is being set multiple times");
		Network.Net.sv.visibility = new Manager(this);
	}

	private void OnEnable()
	{
		halfGridSize = (float)gridSize / 2f;
		cellSize = (float)gridSize / (float)cellCount;
		halfCellSize = cellSize / 2f;
		numIDsPerLayer = cellCount * cellCount;
	}

	private void OnDisable()
	{
		if (!Rust.Application.isQuitting && Network.Net.sv != null && Network.Net.sv.visibility != null)
		{
			Network.Net.sv.visibility.Dispose();
			Network.Net.sv.visibility = null;
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Vector3 position = base.transform.position;
		for (int i = 0; i <= cellCount; i++)
		{
			float num = 0f - halfGridSize + (float)i * cellSize - halfCellSize;
			Gizmos.DrawLine(new Vector3(halfGridSize, position.y, num), new Vector3(0f - halfGridSize, position.y, num));
			Gizmos.DrawLine(new Vector3(num, position.y, halfGridSize), new Vector3(num, position.y, 0f - halfGridSize));
		}
	}

	private int PositionToGrid(float value)
	{
		return Mathf.RoundToInt((value + halfGridSize) / cellSize);
	}

	private float GridToPosition(int value)
	{
		return (float)value * cellSize - halfGridSize;
	}

	private int PositionToLayer(float y)
	{
		if (y < tunnelsThreshold)
		{
			return 2;
		}
		if (y < cavesThreshold)
		{
			return 1;
		}
		if (y >= dynamicDungeonsThreshold)
		{
			return 10 + Mathf.FloorToInt((y - dynamicDungeonsThreshold) / dynamicDungeonsInterval);
		}
		return 0;
	}

	private uint CoordToID(int x, int y, int layer)
	{
		return (uint)(layer * numIDsPerLayer + (x * cellCount + y) + startID);
	}

	private uint GetID(Vector3 vPos)
	{
		int num = PositionToGrid(vPos.x);
		int num2 = PositionToGrid(vPos.z);
		int num3 = PositionToLayer(vPos.y);
		if (num < 0)
		{
			return 0u;
		}
		if (num >= cellCount)
		{
			return 0u;
		}
		if (num2 < 0)
		{
			return 0u;
		}
		if (num2 >= cellCount)
		{
			return 0u;
		}
		uint num4 = CoordToID(num, num2, num3);
		if (num4 < startID)
		{
			Debug.LogError($"NetworkVisibilityGrid.GetID - group is below range {num} {num2} {num3} {num4} {cellCount}");
		}
		return num4;
	}

	private (int x, int y, int layer) DeconstructGroupId(int groupId)
	{
		groupId -= startID;
		int result;
		int item = Math.DivRem(groupId, numIDsPerLayer, out result);
		int result2;
		return (x: Math.DivRem(result, cellCount, out result2), y: result2, layer: item);
	}

	private Bounds GetBounds(uint uid)
	{
		(int x, int y, int layer) tuple = DeconstructGroupId((int)uid);
		int item = tuple.x;
		int item2 = tuple.y;
		int item3 = tuple.layer;
		Vector3 min = new Vector3(GridToPosition(item) - halfCellSize, 0f, GridToPosition(item2) - halfCellSize);
		Vector3 max = new Vector3(min.x + cellSize, 0f, min.z + cellSize);
		if (item3 == 0)
		{
			min.y = cavesThreshold;
			max.y = dynamicDungeonsThreshold;
		}
		else if (item3 == 1)
		{
			min.y = tunnelsThreshold;
			max.y = cavesThreshold - float.Epsilon;
		}
		else if (item3 == 2)
		{
			min.y = -10000f;
			max.y = tunnelsThreshold - float.Epsilon;
		}
		else if (item3 >= 10)
		{
			int num = item3 - 10;
			min.y = dynamicDungeonsThreshold + (float)num * dynamicDungeonsInterval + float.Epsilon;
			max.y = min.y + dynamicDungeonsInterval;
		}
		else
		{
			Debug.LogError($"Cannot get bounds for unknown layer {item3}!", this);
		}
		Bounds result = default(Bounds);
		result.min = min;
		result.max = max;
		return result;
	}

	public void OnGroupAdded(Group group)
	{
		group.bounds = GetBounds(group.ID);
	}

	public bool IsInside(Group group, Vector3 vPos)
	{
		if (0 == 0 && group.ID != 0 && !group.bounds.Contains(vPos))
		{
			return group.bounds.SqrDistance(vPos) < switchTolerance;
		}
		return true;
	}

	public Group GetGroup(Vector3 vPos)
	{
		uint iD = GetID(vPos);
		if (iD == 0)
		{
			return null;
		}
		Group group = Network.Net.sv.visibility.Get(iD);
		if (!IsInside(group, vPos))
		{
			float num = group.bounds.SqrDistance(vPos);
			string[] obj = new string[6]
			{
				"Group is inside is all fucked ",
				iD.ToString(),
				"/",
				num.ToString(),
				"/",
				null
			};
			Vector3 vector = vPos;
			obj[5] = vector.ToString();
			Debug.Log(string.Concat(obj));
		}
		return group;
	}

	public void GetVisibleFromFar(Group group, List<Group> groups)
	{
		int visibilityRadiusFarOverride = ConVar.Net.visibilityRadiusFarOverride;
		int radius = ((visibilityRadiusFarOverride > 0) ? visibilityRadiusFarOverride : visibilityRadiusFar);
		GetVisibleFrom(group, groups, radius);
	}

	public void GetVisibleFromNear(Group group, List<Group> groups)
	{
		int visibilityRadiusNearOverride = ConVar.Net.visibilityRadiusNearOverride;
		int radius = ((visibilityRadiusNearOverride > 0) ? visibilityRadiusNearOverride : visibilityRadiusNear);
		GetVisibleFrom(group, groups, radius);
	}

	private void GetVisibleFrom(Group group, List<Group> groups, int radius)
	{
		if (Interface.CallHook("OnNetworkSubscriptionsGather", this, group, groups, radius) != null)
		{
			return;
		}
		List<Group> groups2 = groups;
		groups2.Add(Network.Net.sv.visibility.Get(0u));
		int iD = (int)group.ID;
		if (iD < startID)
		{
			return;
		}
		var (num, num2, groupLayer2) = DeconstructGroupId(iD);
		AddLayers(num, num2, groupLayer2);
		for (int i = 1; i <= radius; i++)
		{
			AddLayers(num - i, num2, groupLayer2);
			AddLayers(num + i, num2, groupLayer2);
			AddLayers(num, num2 - i, groupLayer2);
			AddLayers(num, num2 + i, groupLayer2);
			for (int j = 1; j < i; j++)
			{
				AddLayers(num - i, num2 - j, groupLayer2);
				AddLayers(num - i, num2 + j, groupLayer2);
				AddLayers(num + i, num2 - j, groupLayer2);
				AddLayers(num + i, num2 + j, groupLayer2);
				AddLayers(num - j, num2 - i, groupLayer2);
				AddLayers(num + j, num2 - i, groupLayer2);
				AddLayers(num - j, num2 + i, groupLayer2);
				AddLayers(num + j, num2 + i, groupLayer2);
			}
			AddLayers(num - i, num2 - i, groupLayer2);
			AddLayers(num - i, num2 + i, groupLayer2);
			AddLayers(num + i, num2 - i, groupLayer2);
			AddLayers(num + i, num2 + i, groupLayer2);
		}
		void Add(int groupX, int groupY, int groupLayer)
		{
			groups2.Add(Network.Net.sv.visibility.Get(CoordToID(groupX, groupY, groupLayer)));
		}
		void AddLayers(int groupX, int groupY, int groupLayer)
		{
			Add(groupX, groupY, groupLayer);
			if (groupLayer == 0)
			{
				Add(groupX, groupY, 1);
			}
			if (groupLayer == 1)
			{
				Add(groupX, groupY, 2);
				Add(groupX, groupY, 0);
			}
		}
	}
}
