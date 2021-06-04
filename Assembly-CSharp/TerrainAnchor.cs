using System;
using UnityEngine;

public class TerrainAnchor : PrefabAttribute
{
	public float Extents = 1f;

	public float Offset;

	public void Apply(out float height, out float min, out float max, Vector3 pos, Vector3 scale)
	{
		float num = Extents * scale.y;
		float num2 = Offset * scale.y;
		height = TerrainMeta.HeightMap.GetHeight(pos);
		min = height - num2 - num;
		max = height - num2 + num;
	}

	protected override Type GetIndexedType()
	{
		return typeof(TerrainAnchor);
	}
}
