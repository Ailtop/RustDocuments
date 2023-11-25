using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public static class WaterLevel
{
	public struct WaterInfo
	{
		public bool isValid;

		public float currentDepth;

		public float overallDepth;

		public float surfaceLevel;
	}

	public static float Factor(Vector3 start, Vector3 end, float radius, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		using (TimeWarning.New("WaterLevel.Factor"))
		{
			WaterInfo waterInfo = GetWaterInfo(start, end, radius, waves, volumes, forEntity);
			return waterInfo.isValid ? Mathf.InverseLerp(Mathf.Min(start.y, end.y) - radius, Mathf.Max(start.y, end.y) + radius, waterInfo.surfaceLevel) : 0f;
		}
	}

	public static float Factor(Bounds bounds, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		using (TimeWarning.New("WaterLevel.Factor"))
		{
			if (bounds.size == Vector3.zero)
			{
				bounds.size = new Vector3(0.1f, 0.1f, 0.1f);
			}
			WaterInfo waterInfo = GetWaterInfo(bounds, waves, volumes, forEntity);
			return waterInfo.isValid ? Mathf.InverseLerp(bounds.min.y, bounds.max.y, waterInfo.surfaceLevel) : 0f;
		}
	}

	public static bool Test(Vector3 pos, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		using (TimeWarning.New("WaterLevel.Test"))
		{
			return GetWaterInfo(pos, waves, volumes, forEntity).isValid;
		}
	}

	public static float GetWaterDepth(Vector3 pos, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		using (TimeWarning.New("WaterLevel.GetWaterDepth"))
		{
			return GetWaterInfo(pos, waves, volumes, forEntity).currentDepth;
		}
	}

	public static float GetOverallWaterDepth(Vector3 pos, bool waves, bool volumes, BaseEntity forEntity = null, bool noEarlyExit = false)
	{
		using (TimeWarning.New("WaterLevel.GetOverallWaterDepth"))
		{
			return GetWaterInfo(pos, waves, volumes, forEntity, noEarlyExit).overallDepth;
		}
	}

	public static WaterInfo GetBuoyancyWaterInfo(Vector3 pos, Vector2 posUV, float terrainHeight, float waterHeight, bool doDeepwaterChecks, BaseEntity forEntity)
	{
		using (TimeWarning.New("WaterLevel.GetWaterInfo"))
		{
			WaterInfo result = default(WaterInfo);
			if (pos.y > waterHeight)
			{
				return GetWaterInfoFromVolumes(pos, forEntity);
			}
			bool flag = pos.y < terrainHeight - 1f;
			if (flag)
			{
				waterHeight = 0f;
				if (pos.y > waterHeight)
				{
					return result;
				}
			}
			bool flag2 = doDeepwaterChecks && pos.y < waterHeight - 10f;
			int num = (TerrainMeta.TopologyMap ? TerrainMeta.TopologyMap.GetTopologyFast(posUV) : 0);
			if ((flag || flag2 || (num & 0x3C180) == 0) && (bool)WaterSystem.Collision && WaterSystem.Collision.GetIgnore(pos))
			{
				return result;
			}
			if (flag2 && Physics.Raycast(pos, Vector3.up, out var hitInfo, 5f, 16, QueryTriggerInteraction.Collide))
			{
				waterHeight = Mathf.Min(waterHeight, hitInfo.point.y);
			}
			result.isValid = true;
			result.currentDepth = Mathf.Max(0f, waterHeight - pos.y);
			result.overallDepth = Mathf.Max(0f, waterHeight - terrainHeight);
			result.surfaceLevel = waterHeight;
			return result;
		}
	}

	public static WaterInfo GetWaterInfo(Vector3 pos, bool waves, bool volumes, BaseEntity forEntity = null, bool noEarlyExit = false)
	{
		using (TimeWarning.New("WaterLevel.GetWaterInfo"))
		{
			WaterInfo waterInfo = default(WaterInfo);
			float num = GetWaterLevel(pos);
			if (pos.y > num)
			{
				if (!noEarlyExit)
				{
					return volumes ? GetWaterInfoFromVolumes(pos, forEntity) : waterInfo;
				}
				waterInfo = (volumes ? GetWaterInfoFromVolumes(pos, forEntity) : waterInfo);
			}
			float num2 = (TerrainMeta.HeightMap ? TerrainMeta.HeightMap.GetHeight(pos) : 0f);
			if (pos.y < num2 - 1f)
			{
				num = 0f;
				if (pos.y > num && !noEarlyExit)
				{
					return waterInfo;
				}
			}
			if ((bool)WaterSystem.Collision && WaterSystem.Collision.GetIgnore(pos))
			{
				return waterInfo;
			}
			waterInfo.isValid = true;
			waterInfo.currentDepth = Mathf.Max(0f, num - pos.y);
			waterInfo.overallDepth = Mathf.Max(0f, num - num2);
			waterInfo.surfaceLevel = num;
			return waterInfo;
		}
	}

	public static WaterInfo GetWaterInfo(Bounds bounds, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		using (TimeWarning.New("WaterLevel.GetWaterInfo"))
		{
			WaterInfo waterInfo = default(WaterInfo);
			float num = GetWaterLevel(bounds.center);
			if (bounds.min.y > num)
			{
				return volumes ? GetWaterInfoFromVolumes(bounds, forEntity) : waterInfo;
			}
			float num2 = (TerrainMeta.HeightMap ? TerrainMeta.HeightMap.GetHeight(bounds.center) : 0f);
			if (bounds.max.y < num2 - 1f)
			{
				num = 0f;
				if (bounds.min.y > num)
				{
					return waterInfo;
				}
			}
			if ((bool)WaterSystem.Collision && WaterSystem.Collision.GetIgnore(bounds))
			{
				return waterInfo;
			}
			waterInfo.isValid = true;
			waterInfo.currentDepth = Mathf.Max(0f, num - bounds.min.y);
			waterInfo.overallDepth = Mathf.Max(0f, num - num2);
			waterInfo.surfaceLevel = num;
			return waterInfo;
		}
	}

	public static WaterInfo GetWaterInfo(Vector3 start, Vector3 end, float radius, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		using (TimeWarning.New("WaterLevel.GetWaterInfo"))
		{
			WaterInfo waterInfo = default(WaterInfo);
			Vector3 vector = (start + end) * 0.5f;
			float num = Mathf.Min(start.y, end.y) - radius;
			float num2 = Mathf.Max(start.y, end.y) + radius;
			float num3 = GetWaterLevel(vector);
			if (num > num3)
			{
				return volumes ? GetWaterInfoFromVolumes(start, end, radius, forEntity) : waterInfo;
			}
			float num4 = (TerrainMeta.HeightMap ? TerrainMeta.HeightMap.GetHeight(vector) : 0f);
			if (num2 < num4 - 1f)
			{
				num3 = 0f;
				if (num > num3)
				{
					return waterInfo;
				}
			}
			if ((bool)WaterSystem.Collision && WaterSystem.Collision.GetIgnore(start, end, radius))
			{
				Vector3 pos = vector.WithY(Mathf.Lerp(num, num2, 0.75f));
				if (WaterSystem.Collision.GetIgnore(pos))
				{
					return waterInfo;
				}
				num3 = Mathf.Min(num3, pos.y);
			}
			waterInfo.isValid = true;
			waterInfo.currentDepth = Mathf.Max(0f, num3 - num);
			waterInfo.overallDepth = Mathf.Max(0f, num3 - num4);
			waterInfo.surfaceLevel = num3;
			return waterInfo;
		}
	}

	public static WaterInfo GetWaterInfo(Camera cam, bool waves, bool volumes, BaseEntity forEntity = null, bool noEarlyExit = false)
	{
		using (TimeWarning.New("WaterLevel.GetWaterInfo"))
		{
			if (cam.transform.position.y < WaterSystem.MinLevel() - 1f)
			{
				return GetWaterInfo(cam.transform.position, waves, volumes, forEntity, noEarlyExit);
			}
			return GetWaterInfo(cam.transform.position - Vector3.up, waves, volumes, forEntity, noEarlyExit);
		}
	}

	private static float GetWaterLevel(Vector3 pos)
	{
		float num = (TerrainMeta.HeightMap ? TerrainMeta.WaterMap.GetHeight(pos) : 0f);
		if (num < WaterSystem.MaxLevel())
		{
			float height = WaterSystem.GetHeight(pos);
			if (num > WaterSystem.OceanLevel)
			{
				return Mathf.Max(num, height);
			}
			return height;
		}
		return num;
	}

	private static WaterInfo GetWaterInfoFromVolumes(Bounds bounds, BaseEntity forEntity)
	{
		WaterInfo info = default(WaterInfo);
		if (forEntity == null)
		{
			List<WaterVolume> obj = Pool.GetList<WaterVolume>();
			Vis.Components(new OBB(bounds), obj, 262144);
			using (List<WaterVolume>.Enumerator enumerator = obj.GetEnumerator())
			{
				while (enumerator.MoveNext() && !enumerator.Current.Test(bounds, out info))
				{
				}
			}
			Pool.FreeList(ref obj);
			return info;
		}
		forEntity.WaterTestFromVolumes(bounds, out info);
		return info;
	}

	private static WaterInfo GetWaterInfoFromVolumes(Vector3 pos, BaseEntity forEntity)
	{
		WaterInfo info = default(WaterInfo);
		if (forEntity == null)
		{
			List<WaterVolume> obj = Pool.GetList<WaterVolume>();
			Vis.Components(pos, 0.1f, obj, 262144);
			using (List<WaterVolume>.Enumerator enumerator = obj.GetEnumerator())
			{
				while (enumerator.MoveNext() && !enumerator.Current.Test(pos, out info))
				{
				}
			}
			Pool.FreeList(ref obj);
			return info;
		}
		forEntity.WaterTestFromVolumes(pos, out info);
		return info;
	}

	private static WaterInfo GetWaterInfoFromVolumes(Vector3 start, Vector3 end, float radius, BaseEntity forEntity)
	{
		WaterInfo info = default(WaterInfo);
		if (forEntity == null)
		{
			List<WaterVolume> obj = Pool.GetList<WaterVolume>();
			Vis.Components(start, end, radius, obj, 262144);
			using (List<WaterVolume>.Enumerator enumerator = obj.GetEnumerator())
			{
				while (enumerator.MoveNext() && !enumerator.Current.Test(start, end, radius, out info))
				{
				}
			}
			Pool.FreeList(ref obj);
			return info;
		}
		forEntity.WaterTestFromVolumes(start, end, radius, out info);
		return info;
	}
}
