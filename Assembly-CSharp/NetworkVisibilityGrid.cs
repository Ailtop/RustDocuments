#define UNITY_ASSERTIONS
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
	public int startID = 1024;

	public int gridSize = 100;

	public int cellCount = 32;

	[FormerlySerializedAs("visibilityRadius")]
	public int visibilityRadiusFar = 2;

	public int visibilityRadiusNear = 1;

	public float switchTolerance = 20f;

	private void Awake()
	{
		Debug.Assert(Network.Net.sv != null, "Network.Net.sv is NULL when creating Visibility Grid");
		Debug.Assert(Network.Net.sv.visibility == null, "Network.Net.sv.visibility is being set multiple times");
		Network.Net.sv.visibility = new Manager(this);
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
		float num = CellSize();
		float num2 = (float)gridSize / 2f;
		Vector3 position = base.transform.position;
		for (int i = 0; i <= cellCount; i++)
		{
			float num3 = 0f - num2 + (float)i * num - num / 2f;
			Gizmos.DrawLine(new Vector3(num2, position.y, num3), new Vector3(0f - num2, position.y, num3));
			Gizmos.DrawLine(new Vector3(num3, position.y, num2), new Vector3(num3, position.y, 0f - num2));
		}
	}

	private int PositionToGrid(float f)
	{
		f += (float)gridSize / 2f;
		return Mathf.RoundToInt(f / CellSize());
	}

	private float GridToPosition(int i)
	{
		return (float)i * CellSize() - (float)gridSize / 2f;
	}

	public uint CoordToID(int x, int y)
	{
		return (uint)(x * cellCount + y + startID);
	}

	public uint GetID(Vector3 vPos)
	{
		int num = PositionToGrid(vPos.x);
		int num2 = PositionToGrid(vPos.z);
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
		uint num3 = CoordToID(num, num2);
		if (num3 < startID)
		{
			Debug.LogError("NetworkVisibilityGrid.GetID - group is below range " + num + " " + num2 + " " + num3 + " " + cellCount);
		}
		if (num3 > startID + cellCount * cellCount)
		{
			Debug.LogError("NetworkVisibilityGrid.GetID - group is higher than range " + num + " " + num2 + " " + num3 + " " + cellCount);
		}
		return num3;
	}

	public Vector3 GetPosition(uint uid)
	{
		uid -= (uint)startID;
		int i = (int)((long)uid / (long)cellCount);
		int i2 = (int)((long)uid % (long)cellCount);
		return new Vector3(GridToPosition(i), 0f, GridToPosition(i2));
	}

	public Bounds GetBounds(uint uid)
	{
		float num = CellSize();
		return new Bounds(GetPosition(uid), new Vector3(num, 1048576f, num));
	}

	public float CellSize()
	{
		return (float)gridSize / (float)cellCount;
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
			Debug.Log("Group is inside is all fucked " + iD + "/" + num + "/" + vPos);
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
		groups.Add(Network.Net.sv.visibility.Get(0u));
		uint iD = group.ID;
		if (iD < startID)
		{
			return;
		}
		iD -= (uint)startID;
		int num = (int)((long)iD / (long)cellCount);
		int num2 = (int)((long)iD % (long)cellCount);
		groups.Add(Network.Net.sv.visibility.Get(CoordToID(num, num2)));
		for (int i = 1; i <= radius; i++)
		{
			groups.Add(Network.Net.sv.visibility.Get(CoordToID(num - i, num2)));
			groups.Add(Network.Net.sv.visibility.Get(CoordToID(num + i, num2)));
			groups.Add(Network.Net.sv.visibility.Get(CoordToID(num, num2 - i)));
			groups.Add(Network.Net.sv.visibility.Get(CoordToID(num, num2 + i)));
			for (int j = 1; j < i; j++)
			{
				groups.Add(Network.Net.sv.visibility.Get(CoordToID(num - i, num2 - j)));
				groups.Add(Network.Net.sv.visibility.Get(CoordToID(num - i, num2 + j)));
				groups.Add(Network.Net.sv.visibility.Get(CoordToID(num + i, num2 - j)));
				groups.Add(Network.Net.sv.visibility.Get(CoordToID(num + i, num2 + j)));
				groups.Add(Network.Net.sv.visibility.Get(CoordToID(num - j, num2 - i)));
				groups.Add(Network.Net.sv.visibility.Get(CoordToID(num + j, num2 - i)));
				groups.Add(Network.Net.sv.visibility.Get(CoordToID(num - j, num2 + i)));
				groups.Add(Network.Net.sv.visibility.Get(CoordToID(num + j, num2 + i)));
			}
			groups.Add(Network.Net.sv.visibility.Get(CoordToID(num - i, num2 - i)));
			groups.Add(Network.Net.sv.visibility.Get(CoordToID(num - i, num2 + i)));
			groups.Add(Network.Net.sv.visibility.Get(CoordToID(num + i, num2 - i)));
			groups.Add(Network.Net.sv.visibility.Get(CoordToID(num + i, num2 + i)));
		}
	}
}
