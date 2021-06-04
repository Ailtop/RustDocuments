using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateRiverTopology : ProceduralComponent
{
	private const int Smoothen = 8;

	public override void Process(uint seed)
	{
		List<PathList> rivers = TerrainMeta.Path.Rivers;
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		foreach (PathList item in rivers)
		{
			if (!World.Networked)
			{
				PathInterpolator path = item.Path;
				item.TrimTopology(16384);
				path.Smoothen(8, Vector3.up);
				path.RecalculateTangents();
				item.ResetTrims();
			}
			heightMap.Push();
			item.AdjustTerrainHeight();
			heightMap.Pop();
		}
		foreach (PathList item2 in rivers.AsEnumerable().Reverse())
		{
			item2.AdjustTerrainTexture();
			item2.AdjustTerrainTopology();
		}
		MarkRiverside();
	}

	public void MarkRiverside()
	{
		TerrainHeightMap heightmap = TerrainMeta.HeightMap;
		TerrainTopologyMap topomap = TerrainMeta.TopologyMap;
		int[] map = topomap.dst;
		int res = topomap.res;
		ImageProcessing.Dilate2D(map, res, res, 49152, 6, delegate(int x, int y)
		{
			if (((uint)map[x * res + y] & 0x31u) != 0)
			{
				map[x * res + y] |= 32768;
			}
			float normX = topomap.Coordinate(x);
			float normZ = topomap.Coordinate(y);
			if (heightmap.GetSlope(normX, normZ) > 40f)
			{
				map[x * res + y] |= 2;
			}
		});
	}
}
