using UnityEngine;

public class GenerateRiverTerrain : ProceduralComponent
{
	private const int Smoothen = 8;

	public override void Process(uint seed)
	{
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		foreach (PathList river in TerrainMeta.Path.Rivers)
		{
			if (!World.Networked)
			{
				PathInterpolator path = river.Path;
				river.TrimTopology(16384);
				path.Smoothen(8, Vector3.up);
				path.RecalculateTangents();
				river.ResetTrims();
			}
			heightMap.Push();
			river.AdjustTerrainHeight();
			heightMap.Pop();
		}
	}
}
