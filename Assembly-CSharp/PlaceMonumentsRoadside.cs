using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PlaceMonumentsRoadside : ProceduralComponent
{
	public struct SpawnInfo
	{
		public Prefab<MonumentInfo> prefab;

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
		Any = 0,
		Min = 1,
		Max = 2
	}

	public enum RoadMode
	{
		SideRoadOrRingRoad = 0,
		SideRoad = 1,
		RingRoad = 2,
		SideRoadOrDesireTrail = 3,
		DesireTrail = 4
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

	public RoadMode RoadType;

	public const int GroupCandidates = 8;

	public const int IndividualCandidates = 8;

	public static Quaternion rot90 = Quaternion.Euler(0f, 90f, 0f);

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
			_ = TerrainMeta.HeightMap;
			List<Prefab<MonumentInfo>> list = new List<Prefab<MonumentInfo>>();
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (!text.Contains("tunnel-entrance") || World.Config.BelowGroundRails)
				{
					Prefab<MonumentInfo>[] array3 = Prefab.Load<MonumentInfo>(text);
					ArrayEx.Shuffle(array3, ref seed);
					list.AddRange(array3);
				}
			}
			Prefab<MonumentInfo>[] array4 = list.ToArray();
			if (array4 == null || array4.Length == 0)
			{
				return;
			}
			ArrayEx.BubbleSort(array4);
			SpawnInfoGroup[] array5 = new SpawnInfoGroup[array4.Length];
			for (int j = 0; j < array4.Length; j++)
			{
				Prefab<MonumentInfo> prefab = array4[j];
				SpawnInfoGroup spawnInfoGroup = null;
				for (int k = 0; k < j; k++)
				{
					SpawnInfoGroup spawnInfoGroup2 = array5[k];
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
					spawnInfoGroup.prefab = array4[j];
					spawnInfoGroup.candidates = new List<SpawnInfo>();
				}
				array5[j] = spawnInfoGroup;
			}
			SpawnInfoGroup[] array6 = array5;
			foreach (SpawnInfoGroup spawnInfoGroup3 in array6)
			{
				if (spawnInfoGroup3.processed)
				{
					continue;
				}
				Prefab<MonumentInfo> prefab3 = spawnInfoGroup3.prefab;
				MonumentInfo component = prefab3.Component;
				if (component == null || World.Size < component.MinWorldSize)
				{
					continue;
				}
				int num = 0;
				Vector3 zero = Vector3.zero;
				TerrainPathConnect[] componentsInChildren = prefab3.Object.GetComponentsInChildren<TerrainPathConnect>(includeInactive: true);
				foreach (TerrainPathConnect terrainPathConnect in componentsInChildren)
				{
					if (terrainPathConnect.Type == InfrastructureType.Road)
					{
						zero += terrainPathConnect.transform.position;
						num++;
					}
				}
				Vector3 vector = -zero.XZ3D().normalized;
				Vector3 forward = rot90 * vector;
				if (num > 1)
				{
					zero /= (float)num;
				}
				foreach (PathList road in TerrainMeta.Path.Roads)
				{
					bool flag = false;
					switch (RoadType)
					{
					case RoadMode.SideRoadOrRingRoad:
						flag = road.Hierarchy == 0 || road.Hierarchy == 1;
						break;
					case RoadMode.SideRoad:
						flag = road.Hierarchy == 1;
						break;
					case RoadMode.RingRoad:
						flag = road.Hierarchy == 0;
						break;
					case RoadMode.SideRoadOrDesireTrail:
						flag = road.Hierarchy == 1 || road.Hierarchy == 2;
						break;
					case RoadMode.DesireTrail:
						flag = road.Hierarchy == 2;
						break;
					}
					if (!flag)
					{
						continue;
					}
					PathInterpolator path = road.Path;
					float num2 = 5f;
					float num3 = 5f;
					float num4 = path.StartOffset + num3;
					float num5 = path.Length - path.EndOffset - num3;
					for (float num6 = num4; num6 <= num5; num6 += num2)
					{
						Vector3 vector2 = (road.Spline ? path.GetPointCubicHermite(num6) : path.GetPoint(num6));
						Vector3 tangent = path.GetTangent(num6);
						for (int m = -1; m <= 1; m += 2)
						{
							Quaternion quaternion = Quaternion.LookRotation(m * tangent.XZ3D());
							Vector3 position = vector2;
							Quaternion quaternion2 = quaternion;
							Vector3 localScale = prefab3.Object.transform.localScale;
							quaternion2 *= Quaternion.LookRotation(forward);
							position -= quaternion2 * zero;
							SpawnInfo item = default(SpawnInfo);
							item.prefab = prefab3;
							item.position = position;
							item.rotation = quaternion2;
							item.scale = localScale;
							spawnInfoGroup3.candidates.Add(item);
						}
					}
				}
				spawnInfoGroup3.processed = true;
			}
			int num7 = 0;
			List<SpawnInfo> a = new List<SpawnInfo>();
			int num8 = 0;
			List<SpawnInfo> b = new List<SpawnInfo>();
			for (int n = 0; n < 8; n++)
			{
				num7 = 0;
				a.Clear();
				ArrayEx.Shuffle(array5, ref seed);
				array6 = array5;
				foreach (SpawnInfoGroup spawnInfoGroup4 in array6)
				{
					Prefab<MonumentInfo> prefab4 = spawnInfoGroup4.prefab;
					MonumentInfo component2 = prefab4.Component;
					if (component2 == null || World.Size < component2.MinWorldSize)
					{
						continue;
					}
					DungeonGridInfo dungeonEntrance = component2.DungeonEntrance;
					int num9 = (int)((!prefab4.Parameters) ? PrefabPriority.Low : (prefab4.Parameters.Priority + 1));
					int num10 = 100000 * num9 * num9 * num9 * num9;
					int num11 = 0;
					int num12 = 0;
					SpawnInfo item2 = default(SpawnInfo);
					spawnInfoGroup4.candidates.Shuffle(ref seed);
					for (int num13 = 0; num13 < spawnInfoGroup4.candidates.Count; num13++)
					{
						SpawnInfo spawnInfo = spawnInfoGroup4.candidates[num13];
						DistanceInfo distanceInfo = GetDistanceInfo(a, prefab4, spawnInfo.position, spawnInfo.rotation, spawnInfo.scale);
						if (distanceInfo.minDistanceSameType < (float)MinDistanceSameType || distanceInfo.minDistanceDifferentType < (float)MinDistanceDifferentType)
						{
							continue;
						}
						int num14 = num10;
						if (distanceInfo.minDistanceSameType != float.MaxValue)
						{
							if (DistanceSameType == DistanceMode.Min)
							{
								num14 -= Mathf.RoundToInt(distanceInfo.minDistanceSameType * distanceInfo.minDistanceSameType * 2f);
							}
							else if (DistanceSameType == DistanceMode.Max)
							{
								num14 += Mathf.RoundToInt(distanceInfo.minDistanceSameType * distanceInfo.minDistanceSameType * 2f);
							}
						}
						if (distanceInfo.minDistanceDifferentType != float.MaxValue)
						{
							if (DistanceDifferentType == DistanceMode.Min)
							{
								num14 -= Mathf.RoundToInt(distanceInfo.minDistanceDifferentType * distanceInfo.minDistanceDifferentType);
							}
							else if (DistanceDifferentType == DistanceMode.Max)
							{
								num14 += Mathf.RoundToInt(distanceInfo.minDistanceDifferentType * distanceInfo.minDistanceDifferentType);
							}
						}
						if (num14 <= num12 || !prefab4.ApplyTerrainAnchors(ref spawnInfo.position, spawnInfo.rotation, spawnInfo.scale, Filter) || !component2.CheckPlacement(spawnInfo.position, spawnInfo.rotation, spawnInfo.scale))
						{
							continue;
						}
						if ((bool)dungeonEntrance)
						{
							Vector3 vector3 = spawnInfo.position + spawnInfo.rotation * Vector3.Scale(spawnInfo.scale, dungeonEntrance.transform.position);
							Vector3 vector4 = dungeonEntrance.SnapPosition(vector3);
							spawnInfo.position += vector4 - vector3;
							if (!dungeonEntrance.IsValidSpawnPosition(vector4))
							{
								continue;
							}
						}
						if (prefab4.ApplyTerrainChecks(spawnInfo.position, spawnInfo.rotation, spawnInfo.scale, Filter) && prefab4.ApplyTerrainFilters(spawnInfo.position, spawnInfo.rotation, spawnInfo.scale) && prefab4.ApplyWaterChecks(spawnInfo.position, spawnInfo.rotation, spawnInfo.scale) && !prefab4.CheckEnvironmentVolumes(spawnInfo.position, spawnInfo.rotation, spawnInfo.scale, EnvironmentType.Underground | EnvironmentType.TrainTunnels))
						{
							num12 = num14;
							item2 = spawnInfo;
							num11++;
							if (num11 >= 8 || DistanceDifferentType == DistanceMode.Any)
							{
								break;
							}
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

	private DistanceInfo GetDistanceInfo(List<SpawnInfo> spawns, Prefab<MonumentInfo> prefab, Vector3 monumentPos, Quaternion monumentRot, Vector3 monumentScale)
	{
		DistanceInfo result = default(DistanceInfo);
		result.minDistanceDifferentType = float.MaxValue;
		result.maxDistanceDifferentType = float.MinValue;
		result.minDistanceSameType = float.MaxValue;
		result.maxDistanceSameType = float.MinValue;
		OBB oBB = new OBB(monumentPos, monumentScale, monumentRot, prefab.Component.Bounds);
		if (TerrainMeta.Path != null)
		{
			foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
			{
				if (!prefab.Component.HasDungeonLink || (!monument.HasDungeonLink && monument.WantsDungeonLink))
				{
					float num = monument.SqrDistance(oBB);
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
				float num2 = new OBB(spawn.position, spawn.scale, spawn.rotation, spawn.prefab.Component.Bounds).SqrDistance(oBB);
				if (num2 < result.minDistanceSameType)
				{
					result.minDistanceSameType = num2;
				}
				if (num2 > result.maxDistanceSameType)
				{
					result.maxDistanceSameType = num2;
				}
			}
			if (prefab.Component.HasDungeonLink)
			{
				foreach (MonumentInfo monument2 in TerrainMeta.Path.Monuments)
				{
					if (monument2.HasDungeonLink || !monument2.WantsDungeonLink)
					{
						float num3 = monument2.SqrDistance(oBB);
						if (num3 < result.minDistanceSameType)
						{
							result.minDistanceSameType = num3;
						}
						if (num3 > result.maxDistanceSameType)
						{
							result.maxDistanceSameType = num3;
						}
					}
				}
				foreach (DungeonGridInfo dungeonGridEntrance in TerrainMeta.Path.DungeonGridEntrances)
				{
					float num4 = dungeonGridEntrance.SqrDistance(monumentPos);
					if (num4 < result.minDistanceSameType)
					{
						result.minDistanceSameType = num4;
					}
					if (num4 > result.maxDistanceSameType)
					{
						result.maxDistanceSameType = num4;
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
		}
		return result;
	}
}
