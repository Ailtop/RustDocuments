using UnityEngine;

public static class TerrainAnchorEx
{
	public static bool ApplyTerrainAnchors(this Transform transform, TerrainAnchor[] anchors, ref Vector3 pos, Quaternion rot, Vector3 scale, SpawnFilter filter = null)
	{
		return ApplyTerrainAnchors(transform, anchors, ref pos, rot, scale, TerrainAnchorMode.MinimizeError, filter);
	}

	public static bool ApplyTerrainAnchors(this Transform transform, TerrainAnchor[] anchors, ref Vector3 pos, Quaternion rot, Vector3 scale, TerrainAnchorMode mode, SpawnFilter filter = null)
	{
		if (anchors.Length == 0)
		{
			return true;
		}
		float num = 0f;
		float num2 = float.MinValue;
		float num3 = float.MaxValue;
		foreach (TerrainAnchor terrainAnchor in anchors)
		{
			Vector3 point = Vector3.Scale(terrainAnchor.worldPosition, scale);
			point = rot * point;
			Vector3 vector = pos + point;
			if (TerrainMeta.OutOfBounds(vector))
			{
				return false;
			}
			if (filter != null && filter.GetFactor(vector) == 0f)
			{
				return false;
			}
			float height;
			float min;
			float max;
			terrainAnchor.Apply(out height, out min, out max, vector);
			num += height - point.y;
			num2 = Mathf.Max(num2, min - point.y);
			num3 = Mathf.Min(num3, max - point.y);
			if (num3 < num2)
			{
				return false;
			}
		}
		if (mode == TerrainAnchorMode.MinimizeError)
		{
			pos.y = Mathf.Clamp(num / (float)anchors.Length, num2, num3);
		}
		else
		{
			pos.y = Mathf.Clamp(pos.y, num2, num3);
		}
		return true;
	}

	public static void ApplyTerrainAnchors(this Transform transform, TerrainAnchor[] anchors)
	{
		Vector3 pos = transform.position;
		ApplyTerrainAnchors(transform, anchors, ref pos, transform.rotation, transform.lossyScale);
		transform.position = pos;
	}
}
