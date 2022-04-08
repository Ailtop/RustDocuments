using System.Collections.Generic;
using UnityEngine;

public class SpawnDistribution
{
	internal SpawnHandler Handler;

	public float Density;

	internal int Count;

	private WorldSpaceGrid<int> grid;

	private Dictionary<uint, int> dict = new Dictionary<uint, int>();

	private ByteQuadtree quadtree = new ByteQuadtree();

	private Vector3 origin;

	private Vector3 area;

	public SpawnDistribution(SpawnHandler handler, byte[] baseValues, Vector3 origin, Vector3 area)
	{
		Handler = handler;
		quadtree.UpdateValues(baseValues);
		this.origin = origin;
		float num = 0f;
		for (int i = 0; i < baseValues.Length; i++)
		{
			num += (float)(int)baseValues[i];
		}
		Density = num / (float)(255 * baseValues.Length);
		Count = 0;
		this.area = new Vector3(area.x / (float)quadtree.Size, area.y, area.z / (float)quadtree.Size);
		grid = new WorldSpaceGrid<int>(area.x, 20f);
	}

	public bool Sample(out Vector3 spawnPos, out Quaternion spawnRot, bool alignToNormal = false, float dithering = 0f)
	{
		return Sample(out spawnPos, out spawnRot, SampleNode(), alignToNormal, dithering);
	}

	public bool Sample(out Vector3 spawnPos, out Quaternion spawnRot, ByteQuadtree.Element node, bool alignToNormal = false, float dithering = 0f)
	{
		if (Handler == null || TerrainMeta.HeightMap == null)
		{
			spawnPos = Vector3.zero;
			spawnRot = Quaternion.identity;
			return false;
		}
		LayerMask placementMask = Handler.PlacementMask;
		LayerMask placementCheckMask = Handler.PlacementCheckMask;
		float placementCheckHeight = Handler.PlacementCheckHeight;
		LayerMask radiusCheckMask = Handler.RadiusCheckMask;
		float radiusCheckDistance = Handler.RadiusCheckDistance;
		for (int i = 0; i < 15; i++)
		{
			spawnPos = origin;
			spawnPos.x += node.Coords.x * area.x;
			spawnPos.z += node.Coords.y * area.z;
			spawnPos.x += Random.value * area.x;
			spawnPos.z += Random.value * area.z;
			spawnPos.x += Random.Range(0f - dithering, dithering);
			spawnPos.z += Random.Range(0f - dithering, dithering);
			Vector3 vector = new Vector3(spawnPos.x, TerrainMeta.HeightMap.GetHeight(spawnPos), spawnPos.z);
			if (vector.y <= spawnPos.y)
			{
				continue;
			}
			if ((int)placementCheckMask != 0 && Physics.Raycast(vector + Vector3.up * placementCheckHeight, Vector3.down, out var hitInfo, placementCheckHeight, placementCheckMask))
			{
				if (((1 << hitInfo.transform.gameObject.layer) & (int)placementMask) == 0)
				{
					continue;
				}
				vector.y = hitInfo.point.y;
			}
			if ((int)radiusCheckMask == 0 || !Physics.CheckSphere(vector, radiusCheckDistance, radiusCheckMask))
			{
				spawnPos.y = vector.y;
				spawnRot = Quaternion.Euler(new Vector3(0f, Random.Range(0f, 360f), 0f));
				if (alignToNormal)
				{
					Vector3 normal = TerrainMeta.HeightMap.GetNormal(spawnPos);
					spawnRot = QuaternionEx.LookRotationForcedUp(spawnRot * Vector3.forward, normal);
				}
				return true;
			}
		}
		spawnPos = Vector3.zero;
		spawnRot = Quaternion.identity;
		return false;
	}

	public ByteQuadtree.Element SampleNode()
	{
		ByteQuadtree.Element result = quadtree.Root;
		while (!result.IsLeaf)
		{
			result = result.RandChild;
		}
		return result;
	}

	public void AddInstance(Spawnable spawnable)
	{
		UpdateCount(spawnable, 1);
	}

	public void RemoveInstance(Spawnable spawnable)
	{
		UpdateCount(spawnable, -1);
	}

	private void UpdateCount(Spawnable spawnable, int delta)
	{
		Count += delta;
		grid[spawnable.SpawnPosition] += delta;
		BaseEntity component = spawnable.GetComponent<BaseEntity>();
		if ((bool)component)
		{
			if (dict.TryGetValue(component.prefabID, out var value))
			{
				dict[component.prefabID] = value + delta;
				return;
			}
			value = delta;
			dict.Add(component.prefabID, value);
		}
	}

	public int GetCount(uint prefabID)
	{
		dict.TryGetValue(prefabID, out var value);
		return value;
	}

	public int GetCount(Vector3 position)
	{
		return grid[position];
	}

	public float GetGridCellArea()
	{
		return grid.CellArea;
	}
}
