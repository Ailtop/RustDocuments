using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlaceMonuments : ProceduralComponent
{
	public struct SpawnInfo
	{
		public Prefab prefab;

		public Vector3 position;

		public Quaternion rotation;

		public Vector3 scale;
	}

	private struct DistanceInfo
	{
		public float minDistanceSameType;

		public float maxDistanceSameType;

		public float minDistanceDifferentType;

		public float maxDistanceDifferentType;

		public float minDistanceDungeonEntrance;

		public float maxDistanceDungeonEntrance;
	}

	public enum DistanceMode
	{
		Any,
		Min,
		Max
	}

	public SpawnFilter Filter;

	public string ResourceFolder = string.Empty;

	public int TargetCount;

	[FormerlySerializedAs("Distance")]
	public int MinDistance = 500;

	[FormerlySerializedAs("MinSize")]
	public int MinWorldSize;

	[Tooltip("Distance to monuments of the same type")]
	public DistanceMode DistanceSameType = DistanceMode.Max;

	[Tooltip("Distance to monuments of a different type")]
	public DistanceMode DistanceDifferentType;

	public const int GroupCandidates = 10;

	public const int IndividualCandidates = 100;

	public const int Attempts = 10000;

	public override void Process(uint seed)
	{
		if (World.Networked)
		{
			World.Spawn("Monument", "assets/bundled/prefabs/autospawn/" + ResourceFolder + "/");
		}
		else
		{
			if (World.Size < MinWorldSize)
			{
				return;
			}
			TerrainHeightMap heightMap = TerrainMeta.HeightMap;
			Prefab<MonumentInfo>[] array = Prefab.Load<MonumentInfo>("assets/bundled/prefabs/autospawn/" + ResourceFolder);
			if (array == null || array.Length == 0)
			{
				return;
			}
			array.Shuffle(seed);
			array.BubbleSort();
			Vector3 position = TerrainMeta.Position;
			Vector3 size = TerrainMeta.Size;
			float x = position.x;
			float z = position.z;
			float max = position.x + size.x;
			float max2 = position.z + size.z;
			int num = 0;
			List<SpawnInfo> a = new List<SpawnInfo>();
			int num2 = 0;
			List<SpawnInfo> b = new List<SpawnInfo>();
			for (int i = 0; i < 10; i++)
			{
				num = 0;
				a.Clear();
				Prefab<MonumentInfo>[] array2 = array;
				foreach (Prefab<MonumentInfo> prefab in array2)
				{
					MonumentInfo component = prefab.Component;
					if ((bool)component && World.Size < component.MinWorldSize)
					{
						continue;
					}
					DungeonInfo dungeonInfo = (component ? component.DungeonEntrance : null);
					int num3 = (int)((!prefab.Parameters) ? PrefabPriority.Low : (prefab.Parameters.Priority + 1));
					int num4 = 100000 * num3 * num3 * num3 * num3;
					int num5 = 0;
					int num6 = 0;
					SpawnInfo item = default(SpawnInfo);
					for (int k = 0; k < 10000; k++)
					{
						float x2 = SeedRandom.Range(ref seed, x, max);
						float z2 = SeedRandom.Range(ref seed, z, max2);
						float normX = TerrainMeta.NormalizeX(x2);
						float normZ = TerrainMeta.NormalizeZ(z2);
						float num7 = SeedRandom.Value(ref seed);
						float factor = Filter.GetFactor(normX, normZ);
						if (factor * factor < num7)
						{
							continue;
						}
						float height = heightMap.GetHeight(normX, normZ);
						Vector3 pos = new Vector3(x2, height, z2);
						Quaternion rot = prefab.Object.transform.localRotation;
						Vector3 scale = prefab.Object.transform.localScale;
						prefab.ApplyDecorComponents(ref pos, ref rot, ref scale);
						int num8 = Mathf.Max(MinDistance, component ? component.MinDistance : 0);
						DistanceInfo distanceInfo = GetDistanceInfo(a, pos, pos + rot * Vector3.Scale(scale, dungeonInfo ? dungeonInfo.transform.position : Vector3.zero));
						if (distanceInfo.minDistanceSameType < (float)num8 || ((bool)dungeonInfo && distanceInfo.minDistanceDungeonEntrance < (float)dungeonInfo.CellSize) || ((bool)prefab.Component && !prefab.Component.CheckPlacement(pos, rot, scale)) || !prefab.ApplyTerrainAnchors(ref pos, rot, scale, Filter) || !prefab.ApplyTerrainChecks(pos, rot, scale, Filter) || !prefab.ApplyTerrainFilters(pos, rot, scale) || !prefab.ApplyWaterChecks(pos, rot, scale) || prefab.CheckEnvironmentVolumes(pos, rot, scale, EnvironmentType.Underground))
						{
							continue;
						}
						SpawnInfo spawnInfo = default(SpawnInfo);
						spawnInfo.prefab = prefab;
						spawnInfo.position = pos;
						spawnInfo.rotation = rot;
						spawnInfo.scale = scale;
						int num9 = num4;
						if (distanceInfo.minDistanceSameType != float.MaxValue)
						{
							if (DistanceSameType == DistanceMode.Min)
							{
								num9 -= Mathf.RoundToInt(distanceInfo.minDistanceSameType * distanceInfo.minDistanceSameType * 2f);
							}
							else if (DistanceSameType == DistanceMode.Max)
							{
								num9 += Mathf.RoundToInt(distanceInfo.minDistanceSameType * distanceInfo.minDistanceSameType * 2f);
							}
						}
						if (distanceInfo.minDistanceDifferentType != float.MaxValue)
						{
							if (DistanceDifferentType == DistanceMode.Min)
							{
								num9 -= Mathf.RoundToInt(distanceInfo.minDistanceDifferentType * distanceInfo.minDistanceDifferentType);
							}
							else if (DistanceDifferentType == DistanceMode.Max)
							{
								num9 += Mathf.RoundToInt(distanceInfo.minDistanceDifferentType * distanceInfo.minDistanceDifferentType);
							}
						}
						if (num9 > num6)
						{
							num6 = num9;
							item = spawnInfo;
						}
						num5++;
						if (num5 >= 100 || DistanceDifferentType == DistanceMode.Any)
						{
							break;
						}
					}
					if (num6 > 0)
					{
						a.Add(item);
						num += num6;
					}
					if (TargetCount > 0 && a.Count >= TargetCount)
					{
						break;
					}
				}
				if (num > num2)
				{
					num2 = num;
					GenericsUtil.Swap(ref a, ref b);
				}
			}
			foreach (SpawnInfo item2 in b)
			{
				World.AddPrefab("Monument", item2.prefab, item2.position, item2.rotation, item2.scale);
			}
		}
	}

	public DistanceInfo GetDistanceInfo(List<SpawnInfo> spawns, Vector3 monumentPos, Vector3 dungeonPos)
	{
		DistanceInfo result = default(DistanceInfo);
		result.minDistanceSameType = float.MaxValue;
		result.maxDistanceSameType = float.MinValue;
		result.minDistanceDifferentType = float.MaxValue;
		result.maxDistanceDifferentType = float.MinValue;
		result.minDistanceDungeonEntrance = float.MaxValue;
		result.maxDistanceDungeonEntrance = float.MinValue;
		if (TerrainMeta.Path != null)
		{
			foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
			{
				float sqrMagnitude = (monument.transform.position - monumentPos).sqrMagnitude;
				if (sqrMagnitude < result.minDistanceDifferentType)
				{
					result.minDistanceDifferentType = sqrMagnitude;
				}
				if (sqrMagnitude > result.maxDistanceDifferentType)
				{
					result.maxDistanceDifferentType = sqrMagnitude;
				}
			}
			if (result.minDistanceDifferentType != float.MaxValue)
			{
				result.minDistanceDifferentType = Mathf.Sqrt(result.minDistanceDifferentType);
			}
			if (result.maxDistanceDifferentType != float.MinValue)
			{
				result.maxDistanceDifferentType = Mathf.Sqrt(result.maxDistanceDifferentType);
			}
		}
		if (TerrainMeta.Path != null)
		{
			foreach (DungeonInfo dungeonEntrance in TerrainMeta.Path.DungeonEntrances)
			{
				float sqrMagnitude2 = (dungeonEntrance.transform.position - dungeonPos).sqrMagnitude;
				if (sqrMagnitude2 < result.minDistanceDungeonEntrance)
				{
					result.minDistanceDungeonEntrance = sqrMagnitude2;
				}
				if (sqrMagnitude2 > result.maxDistanceDungeonEntrance)
				{
					result.maxDistanceDungeonEntrance = sqrMagnitude2;
				}
			}
			if (result.minDistanceDungeonEntrance != float.MaxValue)
			{
				result.minDistanceDungeonEntrance = Mathf.Sqrt(result.minDistanceDungeonEntrance);
			}
			if (result.maxDistanceDungeonEntrance != float.MinValue)
			{
				result.maxDistanceDungeonEntrance = Mathf.Sqrt(result.maxDistanceDungeonEntrance);
			}
		}
		if (spawns != null)
		{
			foreach (SpawnInfo spawn in spawns)
			{
				float sqrMagnitude3 = (spawn.position - monumentPos).sqrMagnitude;
				if (sqrMagnitude3 < result.minDistanceSameType)
				{
					result.minDistanceSameType = sqrMagnitude3;
				}
				if (sqrMagnitude3 > result.maxDistanceSameType)
				{
					result.maxDistanceSameType = sqrMagnitude3;
				}
			}
			if (result.minDistanceSameType != float.MaxValue)
			{
				result.minDistanceSameType = Mathf.Sqrt(result.minDistanceSameType);
			}
			if (result.maxDistanceSameType != float.MinValue)
			{
				result.maxDistanceSameType = Mathf.Sqrt(result.maxDistanceSameType);
			}
		}
		return result;
	}
}
