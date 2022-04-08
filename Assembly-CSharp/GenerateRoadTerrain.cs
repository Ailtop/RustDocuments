using UnityEngine;

public class GenerateRoadTerrain : ProceduralComponent
{
	private const int Smoothen = 16;

	public override void Process(uint seed)
	{
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		TerrainTopologyMap topologymap = TerrainMeta.TopologyMap;
		foreach (PathList road in TerrainMeta.Path.Roads)
		{
			if (road.AdjustTerrainHeightCalls == 0)
			{
				PathInterpolator path = road.Path;
				Vector3[] points = path.Points;
				for (int i = 0; i < points.Length; i++)
				{
					Vector3 vector = points[i];
					vector.y = heightMap.GetHeight(vector);
					points[i] = vector;
				}
				road.TrimTopology(2048);
				path.Smoothen(16, Vector3.up, (Vector3 x) => !topologymap.GetTopology(x, 1572864));
				path.RecalculateTangents();
				road.ResetTrims();
			}
			heightMap.Push();
			road.AdjustTerrainHeight();
			heightMap.Pop();
		}
	}
}
