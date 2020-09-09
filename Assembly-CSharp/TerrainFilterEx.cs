using UnityEngine;

public static class TerrainFilterEx
{
	public static bool ApplyTerrainFilters(this Transform transform, TerrainFilter[] filters, Vector3 pos, Quaternion rot, Vector3 scale, SpawnFilter globalFilter = null)
	{
		if (filters.Length == 0)
		{
			return true;
		}
		foreach (TerrainFilter terrainFilter in filters)
		{
			Vector3 point = Vector3.Scale(terrainFilter.worldPosition, scale);
			point = rot * point;
			Vector3 vector = pos + point;
			if (TerrainMeta.OutOfBounds(vector))
			{
				return false;
			}
			if (globalFilter != null && globalFilter.GetFactor(vector) == 0f)
			{
				return false;
			}
			if (!terrainFilter.Check(vector))
			{
				return false;
			}
		}
		return true;
	}
}
