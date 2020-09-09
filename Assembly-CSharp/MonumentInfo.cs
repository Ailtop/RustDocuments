using UnityEngine;

public class MonumentInfo : MonoBehaviour, IPrefabPreProcess
{
	public MonumentType Type = MonumentType.Building;

	[InspectorFlags]
	public MonumentTier Tier = (MonumentTier)(-1);

	public int MinWorldSize;

	public int MinDistance;

	public Bounds Bounds = new Bounds(Vector3.zero, Vector3.zero);

	public bool HasNavmesh;

	public bool IsSafeZone;

	public bool shouldDisplayOnMap;

	public Translate.Phrase displayPhrase;

	protected void Awake()
	{
		if ((bool)TerrainMeta.Path)
		{
			TerrainMeta.Path.Monuments.Add(this);
		}
	}

	public bool CheckPlacement(Vector3 pos, Quaternion rot, Vector3 scale)
	{
		OBB oBB = new OBB(pos, scale, rot, Bounds);
		Vector3 point = oBB.GetPoint(-1f, 0f, -1f);
		Vector3 point2 = oBB.GetPoint(-1f, 0f, 1f);
		Vector3 point3 = oBB.GetPoint(1f, 0f, -1f);
		Vector3 point4 = oBB.GetPoint(1f, 0f, 1f);
		int topology = TerrainMeta.TopologyMap.GetTopology(point);
		int topology2 = TerrainMeta.TopologyMap.GetTopology(point2);
		int topology3 = TerrainMeta.TopologyMap.GetTopology(point3);
		int topology4 = TerrainMeta.TopologyMap.GetTopology(point4);
		int num = TierToMask(Tier);
		if ((num & topology) == 0)
		{
			return false;
		}
		if ((num & topology2) == 0)
		{
			return false;
		}
		if ((num & topology3) == 0)
		{
			return false;
		}
		if ((num & topology4) == 0)
		{
			return false;
		}
		return true;
	}

	public bool IsInBounds(Vector3 position)
	{
		return new OBB(base.transform.position, base.transform.rotation, Bounds).Contains(position);
	}

	public Vector3 ClosestPointOnBounds(Vector3 position)
	{
		return new OBB(base.transform.position, base.transform.rotation, Bounds).ClosestPoint(position);
	}

	protected void OnDrawGizmosSelected()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = new Color(0f, 0.7f, 1f, 0.1f);
		Gizmos.DrawCube(Bounds.center, Bounds.size);
		Gizmos.color = new Color(0f, 0.7f, 1f, 1f);
		Gizmos.DrawWireCube(Bounds.center, Bounds.size);
	}

	public MonumentNavMesh GetMonumentNavMesh()
	{
		return GetComponent<MonumentNavMesh>();
	}

	public static int TierToMask(MonumentTier tier)
	{
		int num = 0;
		if ((tier & MonumentTier.Tier0) != 0)
		{
			num |= 0x4000000;
		}
		if ((tier & MonumentTier.Tier1) != 0)
		{
			num |= 0x8000000;
		}
		if ((tier & MonumentTier.Tier2) != 0)
		{
			num |= 0x10000000;
		}
		return num;
	}

	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
	}
}
