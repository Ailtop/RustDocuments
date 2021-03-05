using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlaceMonumentsRoadside : ProceduralComponent
{
	public struct SpawnInfo
	{
		public Prefab prefab;

		public Vector3 position;

		public Quaternion rotation;

		public Vector3 scale;
	}

	public class SpawnInfoGroup
	{
		public bool processed;

		public Prefab<MonumentInfo> prefab;

		public List<SpawnInfo> candidates;
	}

	private struct DistanceInfo
	{
		public float minDistanceSameType;

		public float maxDistanceSameType;

		public float minDistanceDifferentType;

		public float maxDistanceDifferentType;
	}

	public enum DistanceMode
	{
		Any,
		Min,
		Max
	}

	public enum RoadMode
	{
		SideRoadOrRingRoad,
		SideRoad,
		RingRoad,
		SideRoadOrDesireTrail,
		DesireTrail
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

	public RoadMode RoadType;

	public const int GroupCandidates = 10;

	public const int IndividualCandidates = 100;

	public static Quaternion rot90 = Quaternion.Euler(0f, 90f, 0f);

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
			ArrayEx.Shuffle(array, seed);
			ArrayEx.BubbleSort(array);
			SpawnInfoGroup[] array2 = new SpawnInfoGroup[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				Prefab<MonumentInfo> prefab = array[i];
				SpawnInfoGroup spawnInfoGroup = null;
				for (int j = 0; j < i; j++)
				{
					SpawnInfoGroup spawnInfoGroup2 = array2[j];
					Prefab<MonumentInfo> prefab2 = spawnInfoGroup2.prefab;
					if (prefab == prefab2)
					{
						spawnInfoGroup = spawnInfoGroup2;
						break;
					}
				}
				if (spawnInfoGroup == null)
				{
					spawnInfoGroup = new SpawnInfoGroup();
					spawnInfoGroup.prefab = array[i];
					spawnInfoGroup.candidates = new List<SpawnInfo>();
				}
				array2[i] = spawnInfoGroup;
			}
			SpawnInfoGroup[] array3 = array2;
			foreach (SpawnInfoGroup spawnInfoGroup3 in array3)
			{
				if (spawnInfoGroup3.processed)
				{
					continue;
				}
				Prefab<MonumentInfo> prefab3 = spawnInfoGroup3.prefab;
				if ((bool)prefab3.Component && World.Size < prefab3.Component.MinWorldSize)
				{
					continue;
				}
				foreach (PathList road in TerrainMeta.Path.Roads)
				{
					switch (RoadType)
					{
					case RoadMode.SideRoadOrRingRoad:
						if (road.IsExtraNarrow)
						{
							continue;
						}
						break;
					case RoadMode.SideRoad:
						if (road.IsExtraNarrow || road.IsExtraWide)
						{
							continue;
						}
						break;
					case RoadMode.RingRoad:
						if (!road.IsExtraWide)
						{
							continue;
						}
						break;
					case RoadMode.SideRoadOrDesireTrail:
						if (road.IsExtraWide)
						{
							continue;
						}
						break;
					case RoadMode.DesireTrail:
						if (!road.IsExtraNarrow)
						{
							continue;
						}
						break;
					}
					if (road.IsExtraNarrow)
					{
						if (road.Start)
						{
							MonumentInfo monumentInfo = TerrainMeta.Path.FindClosest(TerrainMeta.Path.Monuments, road.Path.GetStartPoint());
							if (monumentInfo.Type == MonumentType.WaterWell || (monumentInfo.Type == MonumentType.Building && monumentInfo.displayPhrase.token.StartsWith("mining_quarry")) || (monumentInfo.Type == MonumentType.Radtown && monumentInfo.displayPhrase.token.StartsWith("swamp")))
							{
								continue;
							}
						}
						if (road.End)
						{
							MonumentInfo monumentInfo2 = TerrainMeta.Path.FindClosest(TerrainMeta.Path.Monuments, road.Path.GetEndPoint());
							if (monumentInfo2.Type == MonumentType.WaterWell || (monumentInfo2.Type == MonumentType.Building && monumentInfo2.displayPhrase.token.StartsWith("mining_quarry")) || (monumentInfo2.Type == MonumentType.Radtown && monumentInfo2.displayPhrase.token.StartsWith("swamp")))
							{
								continue;
							}
						}
					}
					PathInterpolator path = road.Path;
					float num = 5f;
					float num2 = 5f;
					float num3 = path.StartOffset + num2;
					float num4 = path.Length - path.EndOffset - num2;
					for (float num5 = num3; num5 <= num4; num5 += num)
					{
						Vector3 vector = (road.Spline ? path.GetPointCubicHermite(num5) : path.GetPoint(num5));
						Vector3 tangent = path.GetTangent(num5);
						int num6 = 0;
						Vector3 zero = Vector3.zero;
						TerrainPathConnect[] componentsInChildren = prefab3.Object.GetComponentsInChildren<TerrainPathConnect>(true);
						foreach (TerrainPathConnect terrainPathConnect in componentsInChildren)
						{
							if (terrainPathConnect.Type == InfrastructureType.Road)
							{
								zero += terrainPathConnect.transform.position;
								num6++;
							}
						}
						if (num6 > 1)
						{
							zero /= (float)num6;
						}
						for (int m = -1; m <= 1; m += 2)
						{
							Quaternion quaternion = Quaternion.LookRotation(m * tangent.XZ3D());
							Vector3 pos = vector;
							Quaternion quaternion2 = quaternion;
							Vector3 localScale = prefab3.Object.transform.localScale;
							if (zero != Vector3.zero)
							{
								quaternion2 *= Quaternion.LookRotation(rot90 * -zero.XZ3D());
								pos -= quaternion2 * zero;
							}
							if ((!prefab3.Component || prefab3.Component.CheckPlacement(pos, quaternion2, localScale)) && prefab3.ApplyTerrainAnchors(ref pos, quaternion2, localScale, Filter) && prefab3.ApplyTerrainChecks(pos, quaternion2, localScale, Filter) && prefab3.ApplyTerrainFilters(pos, quaternion2, localScale) && prefab3.ApplyWaterChecks(pos, quaternion2, localScale) && !prefab3.CheckEnvironmentVolumes(pos, quaternion2, localScale, EnvironmentType.Underground))
							{
								SpawnInfo item = default(SpawnInfo);
								item.prefab = prefab3;
								item.position = pos;
								item.rotation = quaternion2;
								item.scale = localScale;
								spawnInfoGroup3.candidates.Add(item);
							}
						}
					}
				}
				spawnInfoGroup3.processed = true;
			}
			int num7 = 0;
			List<SpawnInfo> a = new List<SpawnInfo>();
			int num8 = 0;
			List<SpawnInfo> b = new List<SpawnInfo>();
			for (int n = 0; n < 10; n++)
			{
				num7 = 0;
				a.Clear();
				ArrayEx.Shuffle(array2, ref seed);
				array3 = array2;
				foreach (SpawnInfoGroup spawnInfoGroup4 in array3)
				{
					Prefab<MonumentInfo> prefab4 = spawnInfoGroup4.prefab;
					if ((bool)prefab4.Component && World.Size < prefab4.Component.MinWorldSize)
					{
						continue;
					}
					int num9 = (int)((!prefab4.Parameters) ? PrefabPriority.Low : (prefab4.Parameters.Priority + 1));
					int num10 = 100000 * num9 * num9 * num9 * num9;
					int num11 = 0;
					int num12 = 0;
					SpawnInfo item2 = default(SpawnInfo);
					spawnInfoGroup4.candidates.Shuffle(ref seed);
					for (int num13 = 0; num13 < spawnInfoGroup4.candidates.Count; num13++)
					{
						SpawnInfo spawnInfo = spawnInfoGroup4.candidates[num13];
						int num14 = Mathf.Max(MinDistance, prefab4.Component ? prefab4.Component.MinDistance : 0);
						DistanceInfo distanceInfo = GetDistanceInfo(a, spawnInfo.position);
						if (distanceInfo.minDistanceSameType < (float)num14)
						{
							continue;
						}
						int num15 = num10;
						if (distanceInfo.minDistanceSameType != float.MaxValue)
						{
							if (DistanceSameType == DistanceMode.Min)
							{
								num15 -= Mathf.RoundToInt(distanceInfo.minDistanceSameType * distanceInfo.minDistanceSameType * 2f);
							}
							else if (DistanceSameType == DistanceMode.Max)
							{
								num15 += Mathf.RoundToInt(distanceInfo.minDistanceSameType * distanceInfo.minDistanceSameType * 2f);
							}
						}
						if (distanceInfo.minDistanceDifferentType != float.MaxValue)
						{
							if (DistanceDifferentType == DistanceMode.Min)
							{
								num15 -= Mathf.RoundToInt(distanceInfo.minDistanceDifferentType * distanceInfo.minDistanceDifferentType);
							}
							else if (DistanceDifferentType == DistanceMode.Max)
							{
								num15 += Mathf.RoundToInt(distanceInfo.minDistanceDifferentType * distanceInfo.minDistanceDifferentType);
							}
						}
						if (num15 > num12)
						{
							num12 = num15;
							item2 = spawnInfo;
						}
						num11++;
						if (num11 >= 100 || DistanceDifferentType == DistanceMode.Any)
						{
							break;
						}
					}
					if (num12 > 0)
					{
						a.Add(item2);
						num7 += num12;
					}
					if (TargetCount > 0 && a.Count >= TargetCount)
					{
						break;
					}
				}
				if (num7 > num8)
				{
					num8 = num7;
					GenericsUtil.Swap(ref a, ref b);
				}
			}
			foreach (SpawnInfo item3 in b)
			{
				World.AddPrefab("Monument", item3.prefab, item3.position, item3.rotation, item3.scale);
			}
		}
	}

	public DistanceInfo GetDistanceInfo(List<SpawnInfo> spawns, Vector3 pos)
	{
		DistanceInfo result = default(DistanceInfo);
		result.minDistanceDifferentType = float.MaxValue;
		result.maxDistanceDifferentType = float.MinValue;
		result.minDistanceSameType = float.MaxValue;
		result.maxDistanceSameType = float.MinValue;
		if (TerrainMeta.Path != null)
		{
			foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
			{
				float sqrMagnitude = (monument.transform.position - pos).sqrMagnitude;
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
		if (spawns != null)
		{
			foreach (SpawnInfo spawn in spawns)
			{
				float sqrMagnitude2 = (spawn.position - pos).sqrMagnitude;
				if (sqrMagnitude2 < result.minDistanceSameType)
				{
					result.minDistanceSameType = sqrMagnitude2;
				}
				if (sqrMagnitude2 > result.maxDistanceSameType)
				{
					result.maxDistanceSameType = sqrMagnitude2;
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
