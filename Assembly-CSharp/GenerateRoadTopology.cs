using System.Linq;

public class GenerateRoadTopology : ProceduralComponent
{
	public override void Process(uint seed)
	{
		foreach (PathList item in TerrainMeta.Path.Roads.AsEnumerable().Reverse())
		{
			item.AdjustTerrainTopology();
		}
		MarkRoadside();
		TerrainMeta.PlacementMap.Reset();
	}

	private void MarkRoadside()
	{
		TerrainHeightMap heightmap = TerrainMeta.HeightMap;
		TerrainTopologyMap topomap = TerrainMeta.TopologyMap;
		int[] map = topomap.dst;
		int res = topomap.res;
		ImageProcessing.Dilate2D(map, res, res, 6144, 6, delegate(int x, int y)
		{
			if (((uint)map[x * res + y] & 0x31u) != 0)
			{
				map[x * res + y] |= 4096;
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
