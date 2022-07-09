using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using ConVar;
using UnityEngine;

public class FileSystem_Warmup : MonoBehaviour
{
	private static bool run = true;

	public static string[] ExcludeFilter = new string[13]
	{
		"/bundled/prefabs/autospawn/monument", "/bundled/prefabs/autospawn/mountain", "/bundled/prefabs/autospawn/canyon", "/bundled/prefabs/autospawn/decor", "/bundled/prefabs/navmesh", "/content/ui/", "/prefabs/ui/", "/prefabs/world/", "/prefabs/system/", "/standard assets/",
		"/third party/", "assets/scenes/prefabs/", "assets/content/structures/"
	};

	public static void Run()
	{
		if (!Global.skipassetwarmup && run)
		{
			string[] assetList = GetAssetList();
			for (int i = 0; i < assetList.Length; i++)
			{
				PrefabWarmup(assetList[i]);
			}
			run = false;
		}
	}

	public static IEnumerator RunAsync(string[] assetList, Action<string> statusFunction = null, string format = null, int priority = 0)
	{
		if (Global.skipassetwarmup || !run)
		{
			yield break;
		}
		Stopwatch statusSw = Stopwatch.StartNew();
		Stopwatch sw = Stopwatch.StartNew();
		AssetPreloadResult preload = FileSystem.PreloadAssets(assetList, Global.warmupConcurrency, priority);
		int warmupIndex = 0;
		while (preload.MoveNext() || warmupIndex < preload.TotalCount)
		{
			float num = CalculateFrameBudget();
			if (num > 0f)
			{
				while (warmupIndex < preload.Results.Count && sw.Elapsed.TotalSeconds < (double)num)
				{
					PrefabWarmup(preload.Results[warmupIndex++].AssetPath);
				}
			}
			if (warmupIndex == 0 || warmupIndex == preload.TotalCount || statusSw.Elapsed.TotalSeconds > 1.0)
			{
				statusFunction?.Invoke(string.Format(format ?? "{0}/{1}", warmupIndex, preload.TotalCount));
				statusSw.Restart();
			}
			yield return CoroutineEx.waitForEndOfFrame;
			sw.Restart();
		}
		run = false;
	}

	public static IEnumerator Run(string[] assetList, Action<string> statusFunction = null, string format = null)
	{
		if (Global.skipassetwarmup || !run)
		{
			yield break;
		}
		Stopwatch statusSw = Stopwatch.StartNew();
		Stopwatch sw = Stopwatch.StartNew();
		for (int i = 0; i < assetList.Length; i++)
		{
			PrefabWarmup(assetList[i]);
			if (i == 0 || i == assetList.Length - 1 || statusSw.Elapsed.TotalSeconds > 1.0)
			{
				statusFunction?.Invoke(string.Format(format ?? "{0}/{1}", i + 1, assetList.Length));
				statusSw.Restart();
			}
			if (sw.Elapsed.TotalSeconds >= (double)CalculateFrameBudget())
			{
				yield return CoroutineEx.waitForEndOfFrame;
				sw.Restart();
			}
		}
		run = false;
	}

	private static float CalculateFrameBudget()
	{
		return 2f;
	}

	private static bool ShouldIgnore(string path)
	{
		for (int i = 0; i < ExcludeFilter.Length; i++)
		{
			if (path.Contains(ExcludeFilter[i], CompareOptions.IgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	public static string[] GetAssetList(bool? poolFilter = null)
	{
		return (from x in GameManifest.Current.prefabProperties
			where !ShouldIgnore(x.name) && (!poolFilter.HasValue || x.pool == poolFilter)
			select x.name).Distinct(StringComparer.InvariantCultureIgnoreCase).ToArray();
	}

	private static void PrefabWarmup(string path)
	{
		GameManager.server.FindPrefab(path);
	}
}
