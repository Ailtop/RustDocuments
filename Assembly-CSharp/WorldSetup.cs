using System.Collections;
using System.Collections.Generic;
using System.IO;
using ConVar;
using Rust;
using UnityEngine;
using UnityEngine.Networking;

public class WorldSetup : SingletonComponent<WorldSetup>
{
	public bool AutomaticallySetup;

	public bool BypassProceduralSpawn;

	public GameObject terrain;

	public GameObject decorPrefab;

	public GameObject grassPrefab;

	public GameObject spawnPrefab;

	private TerrainMeta terrainMeta;

	public uint EditorSeed;

	public uint EditorSalt;

	public uint EditorSize;

	public string EditorUrl = string.Empty;

	public string EditorConfigFile = string.Empty;

	[TextArea]
	public string EditorConfigString = string.Empty;

	public List<ProceduralObject> ProceduralObjects = new List<ProceduralObject>();

	internal List<MonumentNode> MonumentNodes = new List<MonumentNode>();

	public void OnValidate()
	{
		if (this.terrain == null)
		{
			UnityEngine.Terrain terrain = Object.FindObjectOfType<UnityEngine.Terrain>();
			if (terrain != null)
			{
				this.terrain = terrain.gameObject;
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Prefab[] array = Prefab.Load("assets/bundled/prefabs/world", null, null, useProbabilities: false, useWorldConfig: false);
		foreach (Prefab prefab in array)
		{
			if (prefab.Object.GetComponent<BaseEntity>() != null)
			{
				prefab.SpawnEntity(Vector3.zero, Quaternion.identity).Spawn();
			}
			else
			{
				prefab.Spawn(Vector3.zero, Quaternion.identity);
			}
		}
		SingletonComponent[] array2 = Object.FindObjectsOfType<SingletonComponent>();
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].SingletonSetup();
		}
		if ((bool)terrain)
		{
			if ((bool)terrain.GetComponent<TerrainGenerator>())
			{
				World.Procedural = true;
			}
			else
			{
				World.Procedural = false;
				terrainMeta = terrain.GetComponent<TerrainMeta>();
				terrainMeta.Init();
				terrainMeta.SetupComponents();
				terrainMeta.BindShaderProperties();
				terrainMeta.PostSetupComponents();
				World.InitSize(Mathf.RoundToInt(TerrainMeta.Size.x));
				CreateObject(decorPrefab);
				CreateObject(grassPrefab);
				CreateObject(spawnPrefab);
			}
		}
		World.Serialization = new WorldSerialization();
		World.Cached = false;
		World.CleanupOldFiles();
		if (!string.IsNullOrEmpty(EditorConfigString))
		{
			ConVar.World.configString = EditorConfigString;
		}
		if (!string.IsNullOrEmpty(EditorConfigFile))
		{
			ConVar.World.configFile = EditorConfigFile;
		}
		if (AutomaticallySetup)
		{
			StartCoroutine(InitCoroutine());
		}
	}

	public void CreateObject(GameObject prefab)
	{
		if (!(prefab == null))
		{
			GameObject gameObject = Object.Instantiate(prefab);
			if (gameObject != null)
			{
				gameObject.SetActive(value: true);
			}
		}
	}

	public IEnumerator InitCoroutine()
	{
		if (World.CanLoadFromUrl())
		{
			Debug.Log("Loading custom map from " + World.Url);
		}
		else
		{
			Debug.Log("Generating procedural map of size " + World.Size + " with seed " + World.Seed);
		}
		World.Config = new WorldConfig();
		if (!string.IsNullOrEmpty(ConVar.World.configString))
		{
			Debug.Log("Loading custom world config from world.configstring convar");
			World.Config.LoadFromJsonString(ConVar.World.configString);
		}
		else if (!string.IsNullOrEmpty(ConVar.World.configFile))
		{
			string text = ConVar.Server.rootFolder + "/" + ConVar.World.configFile;
			Debug.Log("Loading custom world config from world.configfile convar: " + text);
			World.Config.LoadFromJsonFile(text);
		}
		ProceduralComponent[] components = GetComponentsInChildren<ProceduralComponent>(includeInactive: true);
		Timing downloadTimer = Timing.Start("Downloading World");
		if (World.Procedural && !World.CanLoadFromDisk() && World.CanLoadFromUrl())
		{
			LoadingScreen.Update("DOWNLOADING WORLD");
			yield return CoroutineEx.waitForEndOfFrame;
			yield return CoroutineEx.waitForEndOfFrame;
			yield return CoroutineEx.waitForEndOfFrame;
			UnityWebRequest request = UnityWebRequest.Get(World.Url);
			request.downloadHandler = new DownloadHandlerBuffer();
			request.Send();
			while (!request.isDone)
			{
				LoadingScreen.Update("DOWNLOADING WORLD " + (request.downloadProgress * 100f).ToString("0.0") + "%");
				yield return CoroutineEx.waitForEndOfFrame;
			}
			if (!request.isHttpError && !request.isNetworkError)
			{
				File.WriteAllBytes(World.MapFolderName + "/" + World.MapFileName, request.downloadHandler.data);
			}
			else
			{
				CancelSetup("Couldn't Download Level: " + World.Name + " (" + request.error + ")");
			}
		}
		downloadTimer.End();
		Timing loadTimer = Timing.Start("Loading World");
		if (World.Procedural && World.CanLoadFromDisk())
		{
			LoadingScreen.Update("LOADING WORLD");
			yield return CoroutineEx.waitForEndOfFrame;
			yield return CoroutineEx.waitForEndOfFrame;
			yield return CoroutineEx.waitForEndOfFrame;
			World.Serialization.Load(World.MapFolderName + "/" + World.MapFileName);
			World.Cached = true;
		}
		loadTimer.End();
		if (World.Cached && 9 != World.Serialization.Version)
		{
			Debug.LogWarning("World cache version mismatch: " + 9u + " != " + World.Serialization.Version);
			World.Serialization.Clear();
			World.Cached = false;
			if (World.CanLoadFromUrl())
			{
				CancelSetup("World File Outdated: " + World.Name);
			}
		}
		if (World.Cached && string.IsNullOrEmpty(World.Checksum))
		{
			World.Checksum = World.Serialization.Checksum;
		}
		if (World.Cached)
		{
			World.InitSize(World.Serialization.world.size);
		}
		if ((bool)terrain)
		{
			TerrainGenerator component2 = terrain.GetComponent<TerrainGenerator>();
			if ((bool)component2)
			{
				if (World.Cached)
				{
					int cachedHeightMapResolution = World.GetCachedHeightMapResolution();
					int cachedSplatMapResolution = World.GetCachedSplatMapResolution();
					terrain = component2.CreateTerrain(cachedHeightMapResolution, cachedSplatMapResolution);
				}
				else
				{
					terrain = component2.CreateTerrain();
				}
				terrainMeta = terrain.GetComponent<TerrainMeta>();
				terrainMeta.Init();
				terrainMeta.SetupComponents();
				CreateObject(decorPrefab);
				CreateObject(grassPrefab);
				CreateObject(spawnPrefab);
			}
		}
		Timing spawnTimer = Timing.Start("Spawning World");
		if (World.Cached)
		{
			LoadingScreen.Update("SPAWNING WORLD");
			yield return CoroutineEx.waitForEndOfFrame;
			yield return CoroutineEx.waitForEndOfFrame;
			yield return CoroutineEx.waitForEndOfFrame;
			TerrainMeta.HeightMap.FromByteArray(World.GetMap("terrain"));
			TerrainMeta.SplatMap.FromByteArray(World.GetMap("splat"));
			TerrainMeta.BiomeMap.FromByteArray(World.GetMap("biome"));
			TerrainMeta.TopologyMap.FromByteArray(World.GetMap("topology"));
			TerrainMeta.AlphaMap.FromByteArray(World.GetMap("alpha"));
			TerrainMeta.WaterMap.FromByteArray(World.GetMap("water"));
			IEnumerator worldSpawn = ((ConVar.Global.preloadConcurrency > 1) ? World.SpawnAsync(0.2f, delegate(string str)
			{
				LoadingScreen.Update(str);
			}) : World.Spawn(0.2f, delegate(string str)
			{
				LoadingScreen.Update(str);
			}));
			while (worldSpawn.MoveNext())
			{
				yield return worldSpawn.Current;
			}
			TerrainMeta.Path.Clear();
			TerrainMeta.Path.Roads.AddRange(World.GetPaths("Road"));
			TerrainMeta.Path.Rivers.AddRange(World.GetPaths("River"));
			TerrainMeta.Path.Powerlines.AddRange(World.GetPaths("Powerline"));
			TerrainMeta.Path.Rails.AddRange(World.GetPaths("Rail"));
		}
		if (TerrainMeta.Path != null)
		{
			foreach (DungeonBaseLink dungeonBaseLink in TerrainMeta.Path.DungeonBaseLinks)
			{
				if (dungeonBaseLink != null)
				{
					dungeonBaseLink.Initialize();
				}
			}
		}
		spawnTimer.End();
		Timing procgenTimer = Timing.Start("Processing World");
		if (components.Length != 0)
		{
			for (int i = 0; i < components.Length; i++)
			{
				ProceduralComponent component = components[i];
				if ((bool)component && component.ShouldRun())
				{
					uint seed = (uint)(World.Seed + i);
					LoadingScreen.Update(component.Description.ToUpper());
					yield return CoroutineEx.waitForEndOfFrame;
					yield return CoroutineEx.waitForEndOfFrame;
					yield return CoroutineEx.waitForEndOfFrame;
					Timing timing = Timing.Start(component.Description);
					if ((bool)component)
					{
						component.Process(seed);
					}
					timing.End();
				}
			}
		}
		procgenTimer.End();
		Timing saveTimer = Timing.Start("Saving World");
		if (ConVar.World.cache && World.Procedural && !World.Cached)
		{
			LoadingScreen.Update("SAVING WORLD");
			yield return CoroutineEx.waitForEndOfFrame;
			yield return CoroutineEx.waitForEndOfFrame;
			yield return CoroutineEx.waitForEndOfFrame;
			World.Serialization.world.size = World.Size;
			World.AddPaths(TerrainMeta.Path.Roads);
			World.AddPaths(TerrainMeta.Path.Rivers);
			World.AddPaths(TerrainMeta.Path.Powerlines);
			World.AddPaths(TerrainMeta.Path.Rails);
			World.Serialization.Save(World.MapFolderName + "/" + World.MapFileName);
		}
		saveTimer.End();
		Timing checksumTimer = Timing.Start("Calculating Checksum");
		if (string.IsNullOrEmpty(World.Serialization.Checksum))
		{
			LoadingScreen.Update("CALCULATING CHECKSUM");
			yield return CoroutineEx.waitForEndOfFrame;
			yield return CoroutineEx.waitForEndOfFrame;
			yield return CoroutineEx.waitForEndOfFrame;
			World.Serialization.CalculateChecksum();
		}
		checksumTimer.End();
		if (string.IsNullOrEmpty(World.Checksum))
		{
			World.Checksum = World.Serialization.Checksum;
		}
		Timing oceanTimer = Timing.Start("Ocean Patrol Paths");
		LoadingScreen.Update("OCEAN PATROL PATHS");
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		if (BaseBoat.generate_paths && TerrainMeta.Path != null)
		{
			TerrainMeta.Path.OceanPatrolFar = BaseBoat.GenerateOceanPatrolPath(200f);
		}
		else
		{
			Debug.Log("Skipping ocean patrol paths, baseboat.generate_paths == false");
		}
		oceanTimer.End();
		Timing finalizeTimer = Timing.Start("Finalizing World");
		LoadingScreen.Update("FINALIZING WORLD");
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		if ((bool)terrainMeta)
		{
			terrainMeta.BindShaderProperties();
			terrainMeta.PostSetupComponents();
			TerrainMargin.Create();
		}
		finalizeTimer.End();
		Timing cleaningTimer = Timing.Start("Cleaning Up");
		LoadingScreen.Update("CLEANING UP");
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		foreach (string item in FileSystem.Backend.UnloadBundles("monuments"))
		{
			GameManager.server.preProcessed.Invalidate(item);
			GameManifest.Invalidate(item);
			PrefabAttribute.server.Invalidate(StringPool.Get(item));
		}
		Resources.UnloadUnusedAssets();
		cleaningTimer.End();
		LoadingScreen.Update("DONE");
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		if ((bool)this)
		{
			GameManager.Destroy(base.gameObject);
		}
	}

	private void CancelSetup(string msg)
	{
		Debug.LogError(msg);
		Rust.Application.Quit();
	}
}
