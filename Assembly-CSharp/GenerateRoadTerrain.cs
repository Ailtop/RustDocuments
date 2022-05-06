using System.Linq;
using UnityEngine;

public class GenerateRoadTerrain : ProceduralComponent
{
	public const int SmoothenLoops = 2;

	public const int SmoothenIterations = 8;

	public const int SmoothenY = 16;

	public const int SmoothenXZ = 4;

	public override void Process(uint seed)
	{
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		TerrainTopologyMap topologymap = TerrainMeta.TopologyMap;
		for (int j = 0; j < 2; j++)
		{
			foreach (PathList item in TerrainMeta.Path.Roads.AsEnumerable().Reverse())
			{
				PathInterpolator path = item.Path;
				Vector3[] points = path.Points;
				for (int k = 0; k < points.Length; k++)
				{
					Vector3 vector = points[k];
					vector.y = heightMap.GetHeight(vector);
					points[k] = vector;
				}
				path.Smoothen(8, Vector3.up, delegate(int i)
				{
					int topology = topologymap.GetTopology(path.Points[i]);
					if (((uint)topology & 0x80000u) != 0)
					{
						return 0f;
					}
					return (((uint)topology & 0x100000u) != 0) ? 0.5f : 1f;
				});
				path.RecalculateTangents();
				heightMap.Push();
				float intensity = 1f;
				float fade = 1f / (1f + (float)j / 3f);
				item.AdjustTerrainHeight(intensity, fade);
				heightMap.Pop();
			}
			foreach (PathList item2 in TerrainMeta.Path.Rails.AsEnumerable().Reverse())
			{
				heightMap.Push();
				float intensity2 = 1f;
				float num = 1f / (1f + (float)j / 3f);
				item2.AdjustTerrainHeight(intensity2, num / 4f);
				heightMap.Pop();
			}
		}
		foreach (PathList road in TerrainMeta.Path.Roads)
		{
			PathInterpolator path2 = road.Path;
			Vector3[] points2 = path2.Points;
			for (int l = 0; l < points2.Length; l++)
			{
				Vector3 vector2 = points2[l];
				vector2.y = heightMap.GetHeight(vector2);
				points2[l] = vector2;
			}
			path2.RecalculateTangents();
		}
	}
}
