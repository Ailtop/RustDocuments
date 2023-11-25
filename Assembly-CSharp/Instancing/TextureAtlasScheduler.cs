using System;
using System.Collections.Generic;
using UnityEngine;

namespace Instancing;

public class TextureAtlasScheduler
{
	private class TextureAtlas
	{
		public int Resolution;

		public Texture2DArray TextureArray;

		public List<TextureAtlasItem> Textures = new List<TextureAtlasItem>();
	}

	private class TextureAtlasItem
	{
		public Texture Texture;

		public bool Occupied;
	}

	public static readonly TextureAtlasScheduler Instanced = new TextureAtlasScheduler();

	private Dictionary<int, TextureAtlas> textureAtlases = new Dictionary<int, TextureAtlas>();

	private int AddTexture(TextureAtlas atlas, Texture texture)
	{
		int num = atlas.Textures.FindIndex((TextureAtlasItem x) => !x.Occupied);
		if (num == -1)
		{
			atlas.Textures.Add(new TextureAtlasItem());
			num = atlas.Textures.Count - 1;
		}
		TextureAtlasItem textureAtlasItem = atlas.Textures[num];
		textureAtlasItem.Occupied = false;
		if (atlas.TextureArray.depth < atlas.Textures.Count)
		{
			Texture2DArray texture2DArray = new Texture2DArray(atlas.Resolution, atlas.Resolution, atlas.TextureArray.depth * 2, atlas.TextureArray.format, mipChain: false);
			Graphics.CopyTexture(atlas.TextureArray, texture2DArray);
			UnityEngine.Object.Destroy(atlas.TextureArray);
			atlas.TextureArray = texture2DArray;
		}
		textureAtlasItem.Texture = texture;
		textureAtlasItem.Occupied = true;
		return num;
	}

	private void UpdateTexture(TextureAtlas atlas, Texture texture, int index)
	{
		atlas.Textures[index].Texture = texture;
		Graphics.CopyTexture(texture, atlas.TextureArray);
	}

	public int AddTextureToAtlas(Texture texture)
	{
		TextureAtlas orCreateAtlas = GetOrCreateAtlas(texture.width, texture.height);
		return AddTexture(orCreateAtlas, texture);
	}

	public void ReplaceTextureInAtlas(Texture texture, int index)
	{
		GetOrCreateAtlas(texture.width, texture.height);
	}

	private TextureAtlas GetOrCreateAtlas(int width, int height)
	{
		if (width != height)
		{
			throw new NotSupportedException("Textures must be the same width and height");
		}
		if (!textureAtlases.TryGetValue(width, out var value))
		{
			value = new TextureAtlas
			{
				Resolution = width,
				TextureArray = new Texture2DArray(width, height, 8, TextureFormat.ARGB32, mipChain: false)
			};
			textureAtlases[width] = value;
		}
		return value;
	}

	private int GetResolutionKey(int xSize, int ySize)
	{
		return xSize * 10000 + ySize;
	}
}
