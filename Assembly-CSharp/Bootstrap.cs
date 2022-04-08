using System.Collections;
using System.Globalization;
using System.Linq;
using System.Threading;
using CompanionServer;
using ConVar;
using Facepunch;
using Facepunch.Network;
using Facepunch.Network.Raknet;
using Facepunch.Rust;
using Facepunch.Utility;
using Network;
using Oxide.Core;
using Rust;
using Rust.Ai;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Bootstrap : SingletonComponent<Bootstrap>
{
	internal static bool bootstrapInitRun;

	public static bool isErrored;

	public string messageString = "Loading...";

	public CanvasGroup BootstrapUiCanvas;

	public GameObject errorPanel;

	public TextMeshProUGUI errorText;

	public TextMeshProUGUI statusText;

	private static string lastWrittenValue;

	public static bool needsSetup => !bootstrapInitRun;

	public static bool isPresent
	{
		get
		{
			if (bootstrapInitRun)
			{
				return true;
			}
			if (Object.FindObjectsOfType<GameSetup>().Count() > 0)
			{
				return true;
			}
			return false;
		}
	}

	public static void RunDefaults()
	{
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
		UnityEngine.Application.targetFrameRate = 256;
		UnityEngine.Time.fixedDeltaTime = 0.0625f;
		UnityEngine.Time.maximumDeltaTime = 0.125f;
	}

	public static void Init_Tier0()
	{
		RunDefaults();
		GameSetup.RunOnce = true;
		bootstrapInitRun = true;
		ConsoleSystem.Index.Initialize(ConsoleGen.All);
		UnityButtons.Register();
		Output.Install();
		Facepunch.Pool.ResizeBuffer<Networkable>(65536);
		Facepunch.Pool.ResizeBuffer<EntityLink>(65536);
		Facepunch.Pool.FillBuffer<Networkable>();
		Facepunch.Pool.FillBuffer<EntityLink>();
		SteamNetworking.SetDebugFunction();
		if (Facepunch.CommandLine.HasSwitch("-swnet"))
		{
			NetworkInitSteamworks(enableSteamDatagramRelay: false);
		}
		else if (Facepunch.CommandLine.HasSwitch("-sdrnet"))
		{
			NetworkInitSteamworks(enableSteamDatagramRelay: true);
		}
		else
		{
			NetworkInitRaknet();
		}
		if (!UnityEngine.Application.isEditor)
		{
			string text = Facepunch.CommandLine.Full.Replace(Facepunch.CommandLine.GetSwitch("-rcon.password", Facepunch.CommandLine.GetSwitch("+rcon.password", "RCONPASSWORD")), "******");
			WriteToLog("Command Line: " + text);
		}
		Interface.Initialize();
	}

	public static void Init_Systems()
	{
		Rust.Global.Init();
		if (Rust.GameInfo.IsOfficialServer && ConVar.Server.stats)
		{
			GA.Logging = false;
			GA.Build = BuildInfo.Current.Scm.ChangeId;
			GA.Initialize("218faecaf1ad400a4e15c53392ebeebc", "0c9803ce52c38671278899538b9c54c8d4e19849");
			Facepunch.Rust.Analytics.Server.Enabled = true;
		}
		Facepunch.Application.Initialize(new Integration());
		Facepunch.Performance.GetMemoryUsage = () => SystemInfoEx.systemMemoryUsed;
	}

	public static void Init_Config()
	{
		ConsoleNetwork.Init();
		ConsoleSystem.UpdateValuesFromCommandLine();
		ConsoleSystem.Run(ConsoleSystem.Option.Server, "server.readcfg");
		ServerUsers.Load();
	}

	public static void NetworkInitRaknet()
	{
		Network.Net.sv = new Facepunch.Network.Raknet.Server();
	}

	public static void NetworkInitSteamworks(bool enableSteamDatagramRelay)
	{
		Network.Net.sv = new SteamNetworking.Server(enableSteamDatagramRelay);
	}

	private IEnumerator Start()
	{
		WriteToLog("Bootstrap Startup");
		BenchmarkTimer.Enabled = Facepunch.Utility.CommandLine.Full.Contains("+autobench");
		BenchmarkTimer timer = BenchmarkTimer.New("bootstrap");
		if (!UnityEngine.Application.isEditor)
		{
			ExceptionReporter.InitializeFromUrl("https://83df169465e84da091c1a3cd2fbffeee:3671b903f9a840ecb68411cf946ab9b6@sentry.io/51080");
			ExceptionReporter.Disabled = !Facepunch.Utility.CommandLine.Full.Contains("-official") && !Facepunch.Utility.CommandLine.Full.Contains("-server.official") && !Facepunch.Utility.CommandLine.Full.Contains("+official") && !Facepunch.Utility.CommandLine.Full.Contains("+server.official");
			BuildInfo current = BuildInfo.Current;
			if (current.Scm.Branch != null && current.Scm.Branch.StartsWith("main"))
			{
				ExceptionReporter.InitializeFromUrl("https://0654eb77d1e04d6babad83201b6b6b95:d2098f1d15834cae90501548bd5dbd0d@sentry.io/1836389");
				ExceptionReporter.Disabled = false;
			}
		}
		if (AssetBundleBackend.Enabled)
		{
			AssetBundleBackend newBackend = new AssetBundleBackend();
			using (BenchmarkTimer.New("bootstrap;bundles"))
			{
				yield return StartCoroutine(LoadingUpdate("Opening Bundles"));
				newBackend.Load("Bundles/Bundles");
				FileSystem.Backend = newBackend;
			}
			if (FileSystem.Backend.isError)
			{
				ThrowError(FileSystem.Backend.loadingError);
				yield break;
			}
			using (BenchmarkTimer.New("bootstrap;bundlesindex"))
			{
				newBackend.BuildFileIndex();
			}
		}
		if (FileSystem.Backend.isError)
		{
			ThrowError(FileSystem.Backend.loadingError);
			yield break;
		}
		if (!UnityEngine.Application.isEditor)
		{
			WriteToLog(SystemInfoGeneralText.currentInfo);
		}
		UnityEngine.Texture.SetGlobalAnisotropicFilteringLimits(1, 16);
		if (isErrored)
		{
			yield break;
		}
		using (BenchmarkTimer.New("bootstrap;gamemanifest"))
		{
			yield return StartCoroutine(LoadingUpdate("Loading Game Manifest"));
			GameManifest.Load();
			yield return StartCoroutine(LoadingUpdate("DONE!"));
		}
		using (BenchmarkTimer.New("bootstrap;selfcheck"))
		{
			yield return StartCoroutine(LoadingUpdate("Running Self Check"));
			SelfCheck.Run();
		}
		if (isErrored)
		{
			yield break;
		}
		yield return StartCoroutine(LoadingUpdate("Bootstrap Tier0"));
		using (BenchmarkTimer.New("bootstrap;tier0"))
		{
			Init_Tier0();
		}
		using (BenchmarkTimer.New("bootstrap;commandlinevalues"))
		{
			ConsoleSystem.UpdateValuesFromCommandLine();
		}
		yield return StartCoroutine(LoadingUpdate("Bootstrap Systems"));
		using (BenchmarkTimer.New("bootstrap;init_systems"))
		{
			Init_Systems();
		}
		yield return StartCoroutine(LoadingUpdate("Bootstrap Config"));
		using (BenchmarkTimer.New("bootstrap;init_config"))
		{
			Init_Config();
		}
		if (!isErrored)
		{
			yield return StartCoroutine(LoadingUpdate("Loading Items"));
			using (BenchmarkTimer.New("bootstrap;itemmanager"))
			{
				ItemManager.Initialize();
			}
			if (!isErrored)
			{
				yield return StartCoroutine(DedicatedServerStartup());
				timer?.Dispose();
				GameManager.Destroy(base.gameObject);
			}
		}
	}

	private IEnumerator DedicatedServerStartup()
	{
		Rust.Application.isLoading = true;
		WriteToLog("Skinnable Warmup");
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		GameManifest.LoadAssets();
		WriteToLog("Loading Scene");
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		UnityEngine.Physics.solverIterationCount = 3;
		int @int = PlayerPrefs.GetInt("UnityGraphicsQuality");
		QualitySettings.SetQualityLevel(0);
		PlayerPrefs.SetInt("UnityGraphicsQuality", @int);
		Object.DontDestroyOnLoad(base.gameObject);
		Object.DontDestroyOnLoad(GameManager.server.CreatePrefab("assets/bundled/prefabs/system/server_console.prefab"));
		StartupShared();
		World.InitSize(ConVar.Server.worldsize);
		World.InitSeed(ConVar.Server.seed);
		World.InitSalt(ConVar.Server.salt);
		World.Url = ConVar.Server.levelurl;
		World.Transfer = ConVar.Server.leveltransfer;
		LevelManager.LoadLevel(ConVar.Server.level);
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		yield return StartCoroutine(FileSystem_Warmup.Run(2f, WriteToLog, "Asset Warmup ({0}/{1})"));
		yield return StartCoroutine(StartServer(!Facepunch.CommandLine.HasSwitch("-skipload"), "", allowOutOfDateSaves: false));
		if (!Object.FindObjectOfType<Performance>())
		{
			Object.DontDestroyOnLoad(GameManager.server.CreatePrefab("assets/bundled/prefabs/system/performance.prefab"));
		}
		Facepunch.Pool.Clear();
		Rust.GC.Collect();
		Rust.Application.isLoading = false;
	}

	public static IEnumerator StartServer(bool doLoad, string saveFileOverride, bool allowOutOfDateSaves)
	{
		float timeScale = UnityEngine.Time.timeScale;
		if (ConVar.Time.pausewhileloading)
		{
			UnityEngine.Time.timeScale = 0f;
		}
		RCon.Initialize();
		BaseEntity.Query.Server = new BaseEntity.Query.EntityTree(8096f);
		if ((bool)SingletonComponent<WorldSetup>.Instance)
		{
			yield return SingletonComponent<WorldSetup>.Instance.StartCoroutine(SingletonComponent<WorldSetup>.Instance.InitCoroutine());
		}
		if ((bool)SingletonComponent<DynamicNavMesh>.Instance && SingletonComponent<DynamicNavMesh>.Instance.enabled && !AiManager.nav_disable)
		{
			yield return SingletonComponent<DynamicNavMesh>.Instance.StartCoroutine(SingletonComponent<DynamicNavMesh>.Instance.UpdateNavMeshAndWait());
		}
		if ((bool)SingletonComponent<AiManager>.Instance && SingletonComponent<AiManager>.Instance.enabled)
		{
			SingletonComponent<AiManager>.Instance.Initialize();
			if (!AiManager.nav_disable && AI.npc_enable && TerrainMeta.Path != null)
			{
				foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
				{
					if (monument.HasNavmesh)
					{
						yield return monument.StartCoroutine(monument.GetMonumentNavMesh().UpdateNavMeshAndWait());
					}
				}
				if ((bool)TerrainMeta.Path && (bool)TerrainMeta.Path.DungeonGridRoot)
				{
					DungeonNavmesh dungeonNavmesh = TerrainMeta.Path.DungeonGridRoot.AddComponent<DungeonNavmesh>();
					dungeonNavmesh.NavMeshCollectGeometry = NavMeshCollectGeometry.PhysicsColliders;
					dungeonNavmesh.LayerMask = 65537;
					yield return dungeonNavmesh.StartCoroutine(dungeonNavmesh.UpdateNavMeshAndWait());
				}
				else
				{
					Debug.LogError("Failed to find DungeonGridRoot, NOT generating Dungeon navmesh");
				}
				if ((bool)TerrainMeta.Path && (bool)TerrainMeta.Path.DungeonBaseRoot)
				{
					DungeonNavmesh dungeonNavmesh2 = TerrainMeta.Path.DungeonBaseRoot.AddComponent<DungeonNavmesh>();
					dungeonNavmesh2.NavmeshResolutionModifier = 0.3f;
					dungeonNavmesh2.NavMeshCollectGeometry = NavMeshCollectGeometry.PhysicsColliders;
					dungeonNavmesh2.LayerMask = 65537;
					yield return dungeonNavmesh2.StartCoroutine(dungeonNavmesh2.UpdateNavMeshAndWait());
				}
				else
				{
					Debug.LogError("Failed to find DungeonBaseRoot , NOT generating Dungeon navmesh");
				}
				GenerateDungeonBase.SetupAI();
			}
		}
		GameObject gameObject = GameManager.server.CreatePrefab("assets/bundled/prefabs/system/server.prefab");
		Object.DontDestroyOnLoad(gameObject);
		ServerMgr serverMgr = gameObject.GetComponent<ServerMgr>();
		serverMgr.Initialize(doLoad, saveFileOverride, allowOutOfDateSaves);
		yield return CoroutineEx.waitForSecondsRealtime(0.1f);
		SaveRestore.InitializeEntityLinks();
		yield return CoroutineEx.waitForSecondsRealtime(0.1f);
		SaveRestore.InitializeEntitySupports();
		yield return CoroutineEx.waitForSecondsRealtime(0.1f);
		SaveRestore.InitializeEntityConditionals();
		yield return CoroutineEx.waitForSecondsRealtime(0.1f);
		SaveRestore.GetSaveCache();
		yield return CoroutineEx.waitForSecondsRealtime(0.1f);
		BaseGameMode.CreateGameMode();
		yield return CoroutineEx.waitForSecondsRealtime(0.1f);
		serverMgr.OpenConnection();
		CompanionServer.Server.Initialize();
		using (BenchmarkTimer.New("Boombox.LoadStations"))
		{
			BoomBox.LoadStations();
		}
		if (ConVar.Time.pausewhileloading)
		{
			UnityEngine.Time.timeScale = timeScale;
		}
		WriteToLog("Server startup complete");
	}

	private void StartupShared()
	{
		Interface.CallHook("InitLogging");
		ItemManager.Initialize();
	}

	public void ThrowError(string error)
	{
		Debug.Log("ThrowError: " + error);
		errorPanel.SetActive(value: true);
		errorText.text = error;
		isErrored = true;
	}

	public void ExitGame()
	{
		Debug.Log("Exiting due to Exit Game button on bootstrap error panel");
		Rust.Application.Quit();
	}

	public static IEnumerator LoadingUpdate(string str)
	{
		if ((bool)SingletonComponent<Bootstrap>.Instance)
		{
			SingletonComponent<Bootstrap>.Instance.messageString = str;
			yield return CoroutineEx.waitForEndOfFrame;
			yield return CoroutineEx.waitForEndOfFrame;
		}
	}

	public static void WriteToLog(string str)
	{
		if (!(lastWrittenValue == str))
		{
			DebugEx.Log(str);
		}
	}
}
