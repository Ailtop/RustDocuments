using UnityEngine;

public static class AssetStorage
{
	public static void Save<T>(ref T asset, string path) where T : Object
	{
		bool flag = (bool)(Object)asset;
	}

	public static void Save(ref Texture2D asset)
	{
	}

	public static void Save(ref Texture2D asset, string path, bool linear, bool compress)
	{
		bool flag = (bool)asset;
	}

	public static void Load<T>(ref T asset, string path) where T : Object
	{
	}

	public static void Delete<T>(ref T asset) where T : Object
	{
		if ((bool)(Object)asset)
		{
			Object.Destroy(asset);
			asset = null;
		}
	}
}
