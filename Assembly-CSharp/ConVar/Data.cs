using System.IO;
using UnityEngine;

namespace ConVar;

[Factory("data")]
public class Data : ConsoleSystem
{
	[ServerVar]
	[ClientVar]
	public static void export(Arg args)
	{
		string @string = args.GetString(0, "none");
		string text = Path.Combine(Application.persistentDataPath, @string + ".raw");
		switch (@string)
		{
		case "splatmap":
			if ((bool)TerrainMeta.SplatMap)
			{
				RawWriter.Write(TerrainMeta.SplatMap.ToEnumerable(), text);
			}
			break;
		case "heightmap":
			if ((bool)TerrainMeta.HeightMap)
			{
				RawWriter.Write(TerrainMeta.HeightMap.ToEnumerable(), text);
			}
			break;
		case "biomemap":
			if ((bool)TerrainMeta.BiomeMap)
			{
				RawWriter.Write(TerrainMeta.BiomeMap.ToEnumerable(), text);
			}
			break;
		case "topologymap":
			if ((bool)TerrainMeta.TopologyMap)
			{
				RawWriter.Write(TerrainMeta.TopologyMap.ToEnumerable(), text);
			}
			break;
		case "alphamap":
			if ((bool)TerrainMeta.AlphaMap)
			{
				RawWriter.Write(TerrainMeta.AlphaMap.ToEnumerable(), text);
			}
			break;
		case "watermap":
			if ((bool)TerrainMeta.WaterMap)
			{
				RawWriter.Write(TerrainMeta.WaterMap.ToEnumerable(), text);
			}
			break;
		default:
			args.ReplyWith("Unknown export source: " + @string);
			return;
		}
		args.ReplyWith("Export written to " + text);
	}
}
