using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PlaceMonuments : ProceduralComponent
{
	public struct SpawnInfo
	{
		public Prefab<MonumentInfo> prefab;

		public Vector3 position;

		public Quaternion rotation;

		public Vector3 scale;

		public bool dungeonEntrance;

		public Vector3 dungeonEntrancePos;
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
		Any = 0,
		Min = 1,
		Max = 2
	}

	public SpawnFilter Filter;

	public string ResourceFolder = string.Empty;

	public int TargetCount;

	[FormerlySerializedAs("MinDistance")]
	public int MinDistanceSameType = 500;

	public int MinDistanceDifferentType;

	[FormerlySerializedAs("MinSize")]
	public int MinWorldSize;

	[Tooltip("Distance to monuments of the same type")]
	public DistanceMode DistanceSameType = DistanceMode.Max;

	[Tooltip("Distance to monuments of a different type")]
	public DistanceMode DistanceDifferentType;

	public const int GroupCandidates = 10;

	public const int IndividualCandidates = 10;

	public const int Attempts = 10000;

	private const int MaxDepth = 100000;

	public override void Process(uint seed)
	{
		string[] array = (from folder in ResourceFolder.Split(',')
			select "assets/bundled/prefabs/autospawn/" + folder + "/").ToArray();
		if (World.Networked)
		{
			World.Spawn("Monument", array);
		}
		else
		{
			if (World.Size < MinWorldSize)
			{
				return;
			}
			TerrainHeightMap heightMap = TerrainMeta.HeightMap;
			PathFinder pathFinder = null;
			List<PathFinder.Point> endList = null;
			List<Prefab<MonumentInfo>> list = new List<Prefab<MonumentInfo>>();
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				Prefab<MonumentInfo>[] array3 = Prefab.Load<MonumentInfo>(array2[i]);
				ArrayEx.Shuffle(array3, ref seed);
				list.AddRange(array3);
			}
			Prefab<MonumentInfo>[] array4 = list.ToArray();
			if (array4 == null || array4.Length == 0)
			{
				return;
			}
			ArrayEx.BubbleSort(array4);
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
			for (int j = 0; j < 10; j++)
			{
				num = 0;
				a.Clear();
				Prefab<MonumentInfo>[] array5 = array4;
				foreach (Prefab<MonumentInfo> prefab in array5)
				{
					MonumentInfo component = prefab.Component;
					if (component == null || World.Size < component.MinWorldSize)
					{
						continue;
					}
					DungeonGridInfo dungeonEntrance = component.DungeonEntrance;
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
						Vector3 vector = pos;
						prefab.ApplyDecorComponents(ref pos, ref rot, ref scale);
						if (!prefab.ApplyTerrainAnchors(ref pos, rot, scale, Filter) || !component.CheckPlacement(pos, rot, scale))
						{
							continue;
						}
						if ((bool)dungeonEntrance)
						{
							Vector3 vector2 = pos + rot * Vector3.Scale(scale, dungeonEntrance.transform.position);
							Vector3 vector3 = dungeonEntrance.SnapPosition(vector2);
							pos += vector3 - vector2;
							if (!dungeonEntrance.IsValidSpawnPosition(vector3))
							{
								continue;
							}
							vector = vector3;
						}
						DistanceInfo distanceInfo = GetDistanceInfo(a, prefab, pos, rot, scale, vector);
						if (distanceInfo.minDistanceSameType < (float)MinDistanceSameType || distanceInfo.minDistanceDifferentType < (float)MinDistanceDifferentType || ((bool)dungeonEntrance && distanceInfo.minDistanceDungeonEntrance < (float)dungeonEntrance.CellSize) || !prefab.ApplyTerrainChecks(pos, rot, scale, Filter) || !prefab.ApplyTerrainFilters(pos, rot, scale) || !prefab.ApplyWaterChecks(pos, rot, scale) || prefab.CheckEnvironmentVolumes(pos, rot, scale, EnvironmentType.Underground | EnvironmentType.TrainTunnels))
						{
							continue;
						}
						bool flag = false;
						TerrainPathConnect[] componentsInChildren = prefab.Object.GetComponentsInChildren<TerrainPathConnect>(true);
						foreach (TerrainPathConnect terrainPathConnect in componentsInChildren)
						{
							if (terrainPathConnect.Type == InfrastructureType.Boat)
							{
								if (pathFinder == null)
								{
									int[,] array6 = TerrainPath.CreateBoatCostmap(2f);
									int length = array6.GetLength(0);
									pathFinder = new PathFinder(array6);
									endList = new List<PathFinder.Point>
									{
										new PathFinder.Point(0, 0),
										new PathFinder.Point(0, length / 2),
										new PathFinder.Point(0, length - 1),
										new PathFinder.Point(length / 2, 0),
										new PathFinder.Point(length / 2, length - 1),
										new PathFinder.Point(length - 1, 0),
										new PathFinder.Point(length - 1, length / 2),
										new PathFinder.Point(length - 1, length - 1)
									};
								}
								PathFinder.Point pathFinderPoint = terrainPathConnect.GetPathFinderPoint(pathFinder.GetResolution(0), pos + rot * Vector3.Scale(scale, terrainPathConnect.transform.localPosition));
								if (pathFinder.FindPathUndirected(new List<PathFinder.Point> { pathFinderPoint }, endList, 100000) == null)
								{
									flag = true;
									break;
								}
							}
						}
						if (flag)
						{
							continue;
						}
						SpawnInfo spawnInfo = default(SpawnInfo);
						spawnInfo.prefab = prefab;
						spawnInfo.position = pos;
						spawnInfo.rotation = rot;
						spawnInfo.scale = scale;
						if ((bool)dungeonEntrance)
						{
							spawnInfo.dungeonEntrance = true;
							spawnInfo.dungeonEntrancePos = vector;
						}
						int num8 = num4;
						if (distanceInfo.minDistanceSameType != float.MaxValue)
						{
							if (DistanceSameType == DistanceMode.Min)
							{
								num8 -= Mathf.RoundToInt(distanceInfo.minDistanceSameType * distanceInfo.minDistanceSameType * 2f);
							}
							else if (DistanceSameType == DistanceMode.Max)
							{
								num8 += Mathf.RoundToInt(distanceInfo.minDistanceSameType * distanceInfo.minDistanceSameType * 2f);
							}
						}
						if (distanceInfo.minDistanceDifferentType != float.MaxValue)
						{
							if (DistanceDifferentType == DistanceMode.Min)
							{
								num8 -= Mathf.RoundToInt(distanceInfo.minDistanceDifferentType * distanceInfo.minDistanceDifferentType);
							}
							else if (DistanceDifferentType == DistanceMode.Max)
							{
								num8 += Mathf.RoundToInt(distanceInfo.minDistanceDifferentType * distanceInfo.minDistanceDifferentType);
							}
						}
						if (num8 > num6)
						{
							num6 = num8;
							item = spawnInfo;
						}
						num5++;
						if (num5 >= 10 || DistanceDifferentType == DistanceMode.Any)
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

	public DistanceInfo GetDistanceInfo(List<SpawnInfo> spawns, Prefab<MonumentInfo> prefab, Vector3 monumentPos, Quaternion monumentRot, Vector3 monumentScale, Vector3 dungeonPos)
	{
		DistanceInfo result = default(DistanceInfo);
		result.minDistanceSameType = float.MaxValue;
		result.maxDistanceSameType = float.MinValue;
		result.minDistanceDifferentType = float.MaxValue;
		result.maxDistanceDifferentType = float.MinValue;
		result.minDistanceDungeonEntrance = float.MaxValue;
		result.maxDistanceDungeonEntrance = float.MinValue;
		OBB oBB = new OBB(monumentPos, monumentScale, monumentRot, prefab.Component.Bounds);
		if (spawns != null)
		{
			foreach (SpawnInfo spawn in spawns)
			{
				float num = new OBB(spawn.position, spawn.scale, spawn.rotation, spawn.prefab.Component.Bounds).SqrDistance(oBB);
				if (spawn.prefab.Folder == prefab.Folder)
				{
					if (num < result.minDistanceSameType)
					{
						result.minDistanceSameType = num;
					}
					if (num > result.maxDistanceSameType)
					{
						result.maxDistanceSameType = num;
					}
				}
				else
				{
					if (num < result.minDistanceDifferentType)
					{
						result.minDistanceDifferentType = num;
					}
					if (num > result.maxDistanceDifferentType)
					{
						result.maxDistanceDifferentType = num;
					}
				}
			}
			foreach (SpawnInfo spawn2 in spawns)
			{
				if (spawn2.dungeonEntrance)
				{
					float sqrMagnitude = (spawn2.dungeonEntrancePos - dungeonPos).sqrMagnitude;
					if (sqrMagnitude < result.minDistanceDungeonEntrance)
					{
						result.minDistanceDungeonEntrance = sqrMagnitude;
					}
					if (sqrMagnitude > result.maxDistanceDungeonEntrance)
					{
						result.maxDistanceDungeonEntrance = sqrMagnitude;
					}
				}
			}
		}
		if (TerrainMeta.Path != null)
		{
			foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
			{
				float num2 = monument.SqrDistance(oBB);
				if (num2 < result.minDistanceDifferentType)
				{
					result.minDistanceDifferentType = num2;
				}
				if (num2 > result.maxDistanceDifferentType)
				{
					result.maxDistanceDifferentType = num2;
				}
			}
			foreach (DungeonGridInfo dungeonGridEntrance in TerrainMeta.Path.DungeonGridEntrances)
			{
				float num3 = dungeonGridEntrance.SqrDistance(dungeonPos);
				if (num3 < result.minDistanceDungeonEntrance)
				{
					result.minDistanceDungeonEntrance = num3;
				}
				if (num3 > result.maxDistanceDungeonEntrance)
				{
					result.maxDistanceDungeonEntrance = num3;
				}
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
		if (result.minDistanceDifferentType != float.MaxValue)
		{
			result.minDistanceDifferentType = Mathf.Sqrt(result.minDistanceDifferentType);
		}
		if (result.maxDistanceDifferentType != float.MinValue)
		{
			result.maxDistanceDifferentType = Mathf.Sqrt(result.maxDistanceDifferentType);
		}
		if (result.minDistanceDungeonEntrance != float.MaxValue)
		{
			result.minDistanceDungeonEntrance = Mathf.Sqrt(result.minDistanceDungeonEntrance);
		}
		if (result.maxDistanceDungeonEntrance != float.MinValue)
		{
			result.maxDistanceDungeonEntrance = Mathf.Sqrt(result.maxDistanceDungeonEntrance);
		}
		return result;
	}
}
