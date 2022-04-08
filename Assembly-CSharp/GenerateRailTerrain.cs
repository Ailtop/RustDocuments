using UnityEngine;

public class GenerateRailTerrain : ProceduralComponent
{
	private const int Smoothen = 64;

	public override void Process(uint seed)
	{
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		foreach (PathList rail in TerrainMeta.Path.Rails)
		{
			if (rail.AdjustTerrainHeightCalls == 0)
			{
				PathInterpolator path = rail.Path;
				Vector3[] points = path.Points;
				for (int i = 0; i < points.Length; i++)
				{
					Vector3 vector = points[i];
					vector.y = heightMap.GetHeight(vector);
					points[i] = vector;
				}
				rail.TrimTopology(524288);
				path.Smoothen(64, Vector3.up);
				path.RecalculateTangents();
				rail.ResetTrims();
			}
			heightMap.Push();
			rail.AdjustTerrainHeight();
			heightMap.Pop();
		}
	}
}
