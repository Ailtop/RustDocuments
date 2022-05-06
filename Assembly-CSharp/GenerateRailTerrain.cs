using System.Linq;
using UnityEngine;

public class GenerateRailTerrain : ProceduralComponent
{
	public const int SmoothenLoops = 8;

	public const int SmoothenIterations = 8;

	public const int SmoothenY = 64;

	public const int SmoothenXZ = 32;

	public override void Process(uint seed)
	{
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		for (int i = 0; i < 8; i++)
		{
			foreach (PathList item in TerrainMeta.Path.Rails.AsEnumerable().Reverse())
			{
				PathInterpolator path = item.Path;
				Vector3[] points = path.Points;
				for (int j = 0; j < points.Length; j++)
				{
					Vector3 vector = points[j];
					vector.y = heightMap.GetHeight(vector);
					points[j] = vector;
				}
				path.Smoothen(8, Vector3.up);
				path.RecalculateTangents();
				heightMap.Push();
				float intensity = 1f;
				float fade = 1f / (1f + (float)i / 3f);
				item.AdjustTerrainHeight(intensity, fade);
				heightMap.Pop();
			}
		}
		foreach (PathList rail in TerrainMeta.Path.Rails)
		{
			PathInterpolator path2 = rail.Path;
			Vector3[] points2 = path2.Points;
			for (int k = 0; k < points2.Length; k++)
			{
				Vector3 vector2 = points2[k];
				vector2.y = heightMap.GetHeight(vector2);
				points2[k] = vector2;
			}
			path2.RecalculateTangents();
		}
	}
}
