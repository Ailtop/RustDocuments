using System.Linq;

public class GenerateRailTopology : ProceduralComponent
{
	public override void Process(uint seed)
	{
		foreach (PathList item in TerrainMeta.Path.Rails.AsEnumerable().Reverse())
		{
			item.AdjustTerrainTopology();
		}
		MarkRailside();
		TerrainMeta.PlacementMap.Reset();
	}

	private void MarkRailside()
	{
		TerrainHeightMap heightmap = TerrainMeta.HeightMap;
		TerrainTopologyMap topomap = TerrainMeta.TopologyMap;
		int[] map = topomap.dst;
		int res = topomap.res;
		ImageProcessing.Dilate2D(map, res, res, 1572864, 6, delegate(int x, int y)
		{
			if (((uint)map[x * res + y] & 0x31u) != 0)
			{
				map[x * res + y] |= 1048576;
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
