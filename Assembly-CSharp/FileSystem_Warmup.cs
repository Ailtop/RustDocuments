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

	private static bool running = false;

	public static string[] ExcludeFilter = new string[11]
	{
		"/bundled/prefabs/autospawn/monument", "/bundled/prefabs/autospawn/mountain", "/bundled/prefabs/autospawn/canyon", "/bundled/prefabs/autospawn/decor", "/bundled/prefabs/navmesh", "/content/ui/", "/prefabs/ui/", "/prefabs/world/", "/prefabs/system/", "/standard assets/",
		"/third party/"
	};

	public static void Run()
	{
		if (!Global.skipassetwarmup && run && !running)
		{
			running = true;
			string[] assetList = GetAssetList();
			for (int i = 0; i < assetList.Length; i++)
			{
				PrefabWarmup(assetList[i]);
			}
			running = (run = false);
		}
	}

	public static IEnumerator Run(float deltaTime, Action<string> statusFunction = null, string format = null)
	{
		if (Global.skipassetwarmup || !run || running)
		{
			yield break;
		}
		running = true;
		string[] prewarmAssets = GetAssetList();
		Stopwatch sw = Stopwatch.StartNew();
		for (int i = 0; i < prewarmAssets.Length; i++)
		{
			if (sw.Elapsed.TotalSeconds > (double)deltaTime || i == 0 || i == prewarmAssets.Length - 1)
			{
				statusFunction?.Invoke(string.Format((format != null) ? format : "{0}/{1}", i + 1, prewarmAssets.Length));
				yield return CoroutineEx.waitForEndOfFrame;
				sw.Reset();
				sw.Start();
			}
			PrefabWarmup(prewarmAssets[i]);
		}
		running = (run = false);
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

	public static string[] GetAssetList()
	{
		return (from x in GameManifest.Current.prefabProperties
			select x.name into x
			where !ShouldIgnore(x)
			select x).ToArray();
	}

	private static void PrefabWarmup(string path)
	{
		GameManager.server.FindPrefab(path);
	}
}
