using System;
using System.IO;
using UnityEngine;

namespace ConVar;

[Factory("world")]
public class World : ConsoleSystem
{
	[ServerVar]
	[ClientVar]
	public static bool cache = true;

	[ClientVar]
	public static bool streaming = true;

	[ClientVar]
	[ServerVar]
	public static void monuments(Arg arg)
	{
		if (!TerrainMeta.Path)
		{
			return;
		}
		TextTable textTable = new TextTable();
		textTable.AddColumn("type");
		textTable.AddColumn("name");
		textTable.AddColumn("pos");
		foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
		{
			textTable.AddRow(monument.Type.ToString(), monument.name, monument.transform.position.ToString());
		}
		arg.ReplyWith(textTable.ToString());
	}

	[ServerVar(Clientside = true, Help = "Renders a high resolution PNG of the current map")]
	public static void rendermap(Arg arg)
	{
		float @float = arg.GetFloat(0, 1f);
		int imageWidth;
		int imageHeight;
		Color background;
		byte[] array = MapImageRenderer.Render(out imageWidth, out imageHeight, out background, @float, lossy: false);
		if (array == null)
		{
			arg.ReplyWith("Failed to render the map (is a map loaded now?)");
			return;
		}
		string fullPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, $"map_{global::World.Size}_{global::World.Seed}.png"));
		File.WriteAllBytes(fullPath, array);
		arg.ReplyWith("Saved map render to: " + fullPath);
	}

	[ServerVar(Clientside = true, Help = "Renders a PNG of the current map's tunnel network")]
	public static void rendertunnels(Arg arg)
	{
		RenderMapLayerToFile(arg, "tunnels", MapLayer.TrainTunnels);
	}

	[ServerVar(Clientside = true, Help = "Renders a PNG of the current map's underwater labs, for a specific floor")]
	public static void renderlabs(Arg arg)
	{
		int underwaterLabFloorCount = MapLayerRenderer.GetOrCreate().GetUnderwaterLabFloorCount();
		int @int = arg.GetInt(0);
		if (@int < 0 || @int >= underwaterLabFloorCount)
		{
			arg.ReplyWith($"Floor number must be between 0 and {underwaterLabFloorCount}");
		}
		else
		{
			RenderMapLayerToFile(arg, $"labs_{@int}", (MapLayer)(1 + @int));
		}
	}

	private static void RenderMapLayerToFile(Arg arg, string name, MapLayer layer)
	{
		try
		{
			MapLayerRenderer orCreate = MapLayerRenderer.GetOrCreate();
			orCreate.Render(layer);
			string fullPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, $"{name}_{global::World.Size}_{global::World.Seed}.png"));
			RenderTexture targetTexture = orCreate.renderCamera.targetTexture;
			Texture2D texture2D = new Texture2D(targetTexture.width, targetTexture.height);
			RenderTexture active = RenderTexture.active;
			try
			{
				RenderTexture.active = targetTexture;
				texture2D.ReadPixels(new Rect(0f, 0f, targetTexture.width, targetTexture.height), 0, 0);
				texture2D.Apply();
				File.WriteAllBytes(fullPath, texture2D.EncodeToPNG());
			}
			finally
			{
				RenderTexture.active = active;
				UnityEngine.Object.DestroyImmediate(texture2D);
			}
			arg.ReplyWith("Saved " + name + " render to: " + fullPath);
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
			arg.ReplyWith("Failed to render " + name);
		}
	}
}
