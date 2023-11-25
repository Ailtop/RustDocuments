using System;
using System.Collections.Generic;
using Rust.Workshop;
using UnityEngine;

namespace Instancing;

public class WorkshopSkinScheduler
{
	private class SkinnableData
	{
		public List<DefaultSkinTexture> DefaultTextures = new List<DefaultSkinTexture>();
	}

	private class SkinData
	{
		public ulong SkinId;

		public SkinTexture[] Textures;
	}

	public struct DefaultSkinTexture
	{
		public int Resolution;

		public string Path;

		public Texture Texture;
	}

	public struct SkinTexture
	{
		public int Resolution;

		public int TextureIndex;

		public SkinTexture(int resolution, int textureIndex)
		{
			Resolution = resolution;
			TextureIndex = textureIndex;
		}
	}

	private Dictionary<ulong, SkinData> skinLookup = new Dictionary<ulong, SkinData>();

	private Dictionary<ulong, Skinnable> skinnableLookup = new Dictionary<ulong, Skinnable>();

	public SkinTexture[] GetTextures(Skin skinDef, ulong skinId)
	{
		if (!skinLookup.TryGetValue(skinId, out var value))
		{
			value = InitializeSkin(skinDef, skinId);
		}
		return value.Textures;
	}

	private SkinnableData GetOrCreateSkinnable(Skin skinDef)
	{
		new SkinnableData();
		throw new NotImplementedException();
	}

	private SkinData InitializeSkin(Skin skinDef, ulong skinId)
	{
		SkinData skinData = new SkinData
		{
			SkinId = skinId
		};
		skinLookup.Add(skinId, skinData);
		SkinnableData orCreateSkinnable = GetOrCreateSkinnable(skinDef);
		List<SkinTexture> list = new List<SkinTexture>();
		foreach (DefaultSkinTexture defaultTexture in orCreateSkinnable.DefaultTextures)
		{
			int textureIndex = TextureAtlasScheduler.Instanced.AddTextureToAtlas(defaultTexture.Texture);
			list.Add(new SkinTexture(defaultTexture.Resolution, textureIndex));
			string path = FindSkinTexturePath(skinId);
			LoadTextureAsync(path, defaultTexture.Resolution, textureIndex);
		}
		return skinData;
	}

	private string FindSkinTexturePath(ulong workshopSkin)
	{
		throw new NotImplementedException();
	}

	private void LoadTextureAsync(string path, int resolution, int textureIndex)
	{
		Texture texture = new Texture2D(resolution, resolution);
		TextureAtlasScheduler.Instanced.ReplaceTextureInAtlas(texture, textureIndex);
	}
}
