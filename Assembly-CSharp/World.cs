using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using ConVar;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;

public static class World
{
	public static uint Seed { get; set; }

	public static uint Salt { get; set; }

	public static uint Size { get; set; }

	public static string Checksum { get; set; }

	public static string Url { get; set; }

	public static bool Procedural { get; set; }

	public static bool Cached { get; set; }

	public static bool Networked { get; set; }

	public static bool Receiving { get; set; }

	public static bool Transfer { get; set; }

	public static bool LoadedFromSave { get; set; }

	public static int SpawnIndex { get; set; }

	public static WorldSerialization Serialization { get; set; }

	public static string Name
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		get
		{
			if (CanLoadFromUrl())
			{
				return Path.GetFileNameWithoutExtension(WWW.UnEscapeURL(Url));
			}
			return Application.loadedLevelName;
		}
	}

	public static string MapFileName
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		get
		{
			if (CanLoadFromUrl())
			{
				return Name + ".map";
			}
			return Name.Replace(" ", "").ToLower() + "." + Size + "." + Seed + "." + 234 + ".map";
		}
	}

	public static string MapFolderName
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		get
		{
			return Server.rootFolder;
		}
	}

	public static string SaveFileName
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		get
		{
			if (CanLoadFromUrl())
			{
				return Name + "." + 234 + ".sav";
			}
			return Name.Replace(" ", "").ToLower() + "." + Size + "." + Seed + "." + 234 + ".sav";
		}
	}

	public static string SaveFolderName
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		get
		{
			return Server.rootFolder;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static string GetServerBrowserMapName()
	{
		if (!CanLoadFromUrl())
		{
			return Name;
		}
		if (Name.StartsWith("proceduralmap."))
		{
			return "Procedural Map";
		}
		return "Custom Map";
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static bool CanLoadFromUrl()
	{
		return !string.IsNullOrEmpty(Url);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static bool CanLoadFromDisk()
	{
		return File.Exists(MapFolderName + "/" + MapFileName);
	}

	public static void CleanupOldFiles()
	{
		Regex regex1 = new Regex("proceduralmap\\.[0-9]+\\.[0-9]+\\.[0-9]+\\.map");
		Regex regex2 = new Regex("\\.[0-9]+\\.[0-9]+\\." + 234 + "\\.map");
		foreach (string item in from path in Directory.GetFiles(MapFolderName, "*.map")
			where regex1.IsMatch(path) && !regex2.IsMatch(path)
			select path)
		{
			try
			{
				File.Delete(item);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError(ex.Message);
			}
		}
	}

	public static void InitSeed(int seed)
	{
		InitSeed((uint)seed);
	}

	public static void InitSeed(uint seed)
	{
		if (seed == 0)
		{
			seed = MurmurHashEx.MurmurHashUnsigned(SeedIdentifier()) % 2147483647u;
		}
		if (seed == 0)
		{
			seed = 123456u;
		}
		Seed = seed;
		Server.seed = (int)seed;
	}

	private static string SeedIdentifier()
	{
		return SystemInfo.deviceUniqueIdentifier + "_" + 234 + "_" + Server.identity;
	}

	public static void InitSalt(int salt)
	{
		InitSalt((uint)salt);
	}

	public static void InitSalt(uint salt)
	{
		if (salt == 0)
		{
			salt = MurmurHashEx.MurmurHashUnsigned(SaltIdentifier()) % 2147483647u;
		}
		if (salt == 0)
		{
			salt = 654321u;
		}
		Salt = salt;
		Server.salt = (int)salt;
	}

	private static string SaltIdentifier()
	{
		return SystemInfo.deviceUniqueIdentifier + "_salt";
	}

	public static void InitSize(int size)
	{
		InitSize((uint)size);
	}

	public static void InitSize(uint size)
	{
		if (size == 0)
		{
			size = 4500u;
		}
		if (size < 1000)
		{
			size = 1000u;
		}
		if (size > 6000)
		{
			size = 6000u;
		}
		Size = size;
		Server.worldsize = (int)size;
	}

	public static byte[] GetMap(string name)
	{
		return Serialization.GetMap(name)?.data;
	}

	public static int GetCachedHeightMapResolution()
	{
		return Mathf.RoundToInt(Mathf.Sqrt(GetMap("height").Length / 2));
	}

	public static int GetCachedSplatMapResolution()
	{
		return Mathf.RoundToInt(Mathf.Sqrt(GetMap("splat").Length / 8));
	}

	public static void AddMap(string name, byte[] data)
	{
		Serialization.AddMap(name, data);
	}

	public static void AddPrefab(string category, Prefab prefab, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		Serialization.AddPrefab(category, prefab.ID, position, rotation, scale);
		if (!Cached)
		{
			rotation = Quaternion.Euler(rotation.eulerAngles);
			Spawn(category, prefab, position, rotation, scale);
		}
	}

	public static PathData PathListToPathData(PathList src)
	{
		return new PathData
		{
			name = src.Name,
			spline = src.Spline,
			start = src.Start,
			end = src.End,
			width = src.Width,
			innerPadding = src.InnerPadding,
			outerPadding = src.OuterPadding,
			innerFade = src.InnerFade,
			outerFade = src.OuterFade,
			randomScale = src.RandomScale,
			meshOffset = src.MeshOffset,
			terrainOffset = src.TerrainOffset,
			splat = src.Splat,
			topology = src.Topology,
			hierarchy = src.Hierarchy,
			nodes = VectorArrayToList(src.Path.Points)
		};
	}

	public static PathList PathDataToPathList(PathData src)
	{
		PathList pathList = new PathList(src.name, VectorListToArray(src.nodes));
		pathList.Spline = src.spline;
		pathList.Start = src.start;
		pathList.End = src.end;
		pathList.Width = src.width;
		pathList.InnerPadding = src.innerPadding;
		pathList.OuterPadding = src.outerPadding;
		pathList.InnerFade = src.innerFade;
		pathList.OuterFade = src.outerFade;
		pathList.RandomScale = src.randomScale;
		pathList.MeshOffset = src.meshOffset;
		pathList.TerrainOffset = src.terrainOffset;
		pathList.Splat = src.splat;
		pathList.Topology = src.topology;
		pathList.Hierarchy = src.hierarchy;
		pathList.Path.RecalculateTangents();
		return pathList;
	}

	public static Vector3[] VectorListToArray(List<VectorData> src)
	{
		Vector3[] array = new Vector3[src.Count];
		for (int i = 0; i < array.Length; i++)
		{
			VectorData vectorData = src[i];
			Vector3 vector = default(Vector3);
			vector.x = vectorData.x;
			vector.y = vectorData.y;
			vector.z = vectorData.z;
			array[i] = vector;
		}
		return array;
	}

	public static List<VectorData> VectorArrayToList(Vector3[] src)
	{
		List<VectorData> list = new List<VectorData>(src.Length);
		for (int i = 0; i < src.Length; i++)
		{
			Vector3 vector = src[i];
			VectorData item = default(VectorData);
			item.x = vector.x;
			item.y = vector.y;
			item.z = vector.z;
			list.Add(item);
		}
		return list;
	}

	public static IEnumerable<PathList> GetPaths(string name)
	{
		return from p in Serialization.GetPaths(name)
			select PathDataToPathList(p);
	}

	public static void AddPaths(IEnumerable<PathList> paths)
	{
		foreach (PathList path in paths)
		{
			AddPath(path);
		}
	}

	public static void AddPath(PathList path)
	{
		Serialization.AddPath(PathListToPathData(path));
	}

	public static IEnumerator SpawnAsync(float deltaTime, Action<string> statusFunction = null)
	{
		int totalCount = 0;
		Dictionary<string, List<PrefabData>> assetGroups = new Dictionary<string, List<PrefabData>>(StringComparer.InvariantCultureIgnoreCase);
		foreach (PrefabData prefab2 in Serialization.world.prefabs)
		{
			string text = StringPool.Get(prefab2.id);
			if (string.IsNullOrWhiteSpace(text))
			{
				UnityEngine.Debug.LogWarning($"Could not find path for prefab ID {prefab2.id}, skipping spawn");
				continue;
			}
			if (!assetGroups.TryGetValue(text, out var value))
			{
				value = new List<PrefabData>();
				assetGroups.Add(text, value);
			}
			value.Add(prefab2);
			totalCount++;
		}
		int spawnedCount = 0;
		int resultIndex = 0;
		Stopwatch sw = Stopwatch.StartNew();
		AssetPreloadResult load = FileSystem.PreloadAssets(assetGroups.Keys, Global.preloadConcurrency, 10);
		while (load != null && (load.MoveNext() || assetGroups.Count > 0))
		{
			while (resultIndex < load.Results.Count && sw.Elapsed.TotalSeconds < (double)deltaTime)
			{
				string item = load.Results[resultIndex].AssetPath;
				if (!assetGroups.TryGetValue(item, out var value2))
				{
					resultIndex++;
					continue;
				}
				if (value2.Count == 0)
				{
					assetGroups.Remove(item);
					resultIndex++;
					continue;
				}
				int index = value2.Count - 1;
				PrefabData prefab = value2[index];
				value2.RemoveAt(index);
				Spawn(prefab);
				spawnedCount++;
			}
			Status(statusFunction, "Spawning World ({0}/{1})", spawnedCount, totalCount);
			yield return CoroutineEx.waitForEndOfFrame;
			sw.Restart();
		}
	}

	public static IEnumerator Spawn(float deltaTime, Action<string> statusFunction = null)
	{
		Stopwatch sw = Stopwatch.StartNew();
		for (int i = 0; i < Serialization.world.prefabs.Count; i++)
		{
			if (sw.Elapsed.TotalSeconds > (double)deltaTime || i == 0 || i == Serialization.world.prefabs.Count - 1)
			{
				Status(statusFunction, "Spawning World ({0}/{1})", i + 1, Serialization.world.prefabs.Count);
				yield return CoroutineEx.waitForEndOfFrame;
				sw.Reset();
				sw.Start();
			}
			Spawn(Serialization.world.prefabs[i]);
		}
	}

	public static void Spawn()
	{
		for (int i = 0; i < Serialization.world.prefabs.Count; i++)
		{
			Spawn(Serialization.world.prefabs[i]);
		}
	}

	public static void Spawn(string category, string folder = null)
	{
		for (int i = SpawnIndex; i < Serialization.world.prefabs.Count; i++)
		{
			PrefabData prefabData = Serialization.world.prefabs[i];
			if (!(prefabData.category != category))
			{
				string text = StringPool.Get(prefabData.id);
				if (string.IsNullOrEmpty(folder) || text.StartsWith(folder))
				{
					Spawn(prefabData);
					SpawnIndex++;
					continue;
				}
				break;
			}
			break;
		}
	}

	public static void Spawn(string category, string[] folders)
	{
		for (int i = SpawnIndex; i < Serialization.world.prefabs.Count; i++)
		{
			PrefabData prefabData = Serialization.world.prefabs[i];
			if (!(prefabData.category != category))
			{
				string str = StringPool.Get(prefabData.id);
				if (folders == null || str.StartsWithAny(folders))
				{
					Spawn(prefabData);
					SpawnIndex++;
					continue;
				}
				break;
			}
			break;
		}
	}

	private static void Spawn(PrefabData prefab)
	{
		Spawn(prefab.category, Prefab.Load(prefab.id), prefab.position, prefab.rotation, prefab.scale);
	}

	private static void Spawn(string category, Prefab prefab, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		if (prefab != null && (bool)prefab.Object)
		{
			if (!Cached)
			{
				prefab.ApplyTerrainPlacements(position, rotation, scale);
				prefab.ApplyTerrainModifiers(position, rotation, scale);
			}
			GameObject gameObject = prefab.Spawn(position, rotation, scale);
			if ((bool)gameObject)
			{
				Interface.CallHook("OnWorldPrefabSpawned", gameObject, category);
				GameObjectEx.SetHierarchyGroup(gameObject, category);
			}
		}
	}

	private static void Status(Action<string> statusFunction, string status, object obj1)
	{
		statusFunction?.Invoke(string.Format(status, obj1));
	}

	private static void Status(Action<string> statusFunction, string status, object obj1, object obj2)
	{
		statusFunction?.Invoke(string.Format(status, obj1, obj2));
	}

	private static void Status(Action<string> statusFunction, string status, object obj1, object obj2, object obj3)
	{
		statusFunction?.Invoke(string.Format(status, obj1, obj2, obj3));
	}

	private static void Status(Action<string> statusFunction, string status, params object[] objs)
	{
		statusFunction?.Invoke(string.Format(status, objs));
	}
}
