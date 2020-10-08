using EasyAntiCheat.Server.Scout;
using Facepunch.Network.Raknet;
using Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ConVar
{
	[Factory("server")]
	public class Server : ConsoleSystem
	{
		[ServerVar]
		public static string ip = "";

		[ServerVar]
		public static int port = 28015;

		[ServerVar]
		public static int queryport = 0;

		[ServerVar]
		public static int maxplayers = 500;

		[ServerVar]
		public static string hostname = "My Untitled Rust Server";

		[ServerVar]
		public static string identity = "my_server_identity";

		[ServerVar]
		public static string level = "Procedural Map";

		[ServerVar]
		public static string levelurl = "";

		[ServerVar]
		public static bool leveltransfer = true;

		[ServerVar]
		public static int seed = 1337;

		[ServerVar]
		public static int salt = 1;

		[ServerVar]
		public static int worldsize = 4500;

		[ServerVar]
		public static int saveinterval = 600;

		[ServerVar]
		public static bool secure = true;

		[ServerVar]
		public static int encryption = 2;

		[ServerVar]
		public static int tickrate = 10;

		[ServerVar]
		public static int entityrate = 16;

		[ServerVar]
		public static float schematime = 1800f;

		[ServerVar]
		public static float cycletime = 500f;

		[ServerVar]
		public static bool official = false;

		[ServerVar]
		public static bool stats = false;

		[ServerVar]
		public static bool globalchat = true;

		[ServerVar]
		public static bool stability = true;

		[ServerVar]
		public static bool radiation = true;

		[ServerVar]
		public static float itemdespawn = 300f;

		[ServerVar]
		public static float corpsedespawn = 300f;

		[ServerVar]
		public static float debrisdespawn = 30f;

		[ServerVar]
		public static bool pve = false;

		[ServerVar]
		public static bool cinematic = false;

		[ServerVar]
		public static string description = "No server description has been provided.";

		[ServerVar]
		public static string headerimage = "";

		[ServerVar]
		public static string url = "";

		[ServerVar]
		public static string branch = "";

		[ServerVar]
		public static int queriesPerSecond = 2000;

		[ServerVar]
		public static int ipQueriesPerMin = 30;

		[ReplicatedVar(Saved = true)]
		public static string motd = "";

		[ServerVar(Saved = true)]
		public static float meleedamage = 1f;

		[ServerVar(Saved = true)]
		public static float arrowdamage = 1f;

		[ServerVar(Saved = true)]
		public static float bulletdamage = 1f;

		[ServerVar(Saved = true)]
		public static float bleedingdamage = 1f;

		[ReplicatedVar(Saved = true)]
		public static float funWaterDamageThreshold = 0.8f;

		[ReplicatedVar(Saved = true)]
		public static float funWaterWetnessGain = 0.05f;

		[ServerVar(Saved = true)]
		public static float meleearmor = 1f;

		[ServerVar(Saved = true)]
		public static float arrowarmor = 1f;

		[ServerVar(Saved = true)]
		public static float bulletarmor = 1f;

		[ServerVar(Saved = true)]
		public static float bleedingarmor = 1f;

		[ServerVar]
		public static int updatebatch = 512;

		[ServerVar]
		public static int updatebatchspawn = 1024;

		[ServerVar]
		public static int entitybatchsize = 100;

		[ServerVar]
		public static float entitybatchtime = 1f;

		[ServerVar]
		public static float composterUpdateInterval = 300f;

		[ReplicatedVar]
		public static float planttick = 60f;

		[ServerVar]
		public static float planttickscale = 1f;

		[ServerVar]
		public static bool useMinimumPlantCondition = true;

		[ServerVar(Saved = true)]
		public static float nonPlanterDeathChancePerTick = 0.005f;

		[ServerVar(Saved = true)]
		public static float ceilingLightGrowableRange = 3f;

		[ServerVar(Saved = true)]
		public static float artificialTemperatureGrowableRange = 4f;

		[ServerVar(Saved = true)]
		public static float ceilingLightHeightOffset = 3f;

		[ServerVar(Saved = true)]
		public static float sprinklerRadius = 3f;

		[ServerVar(Saved = true)]
		public static float sprinklerEyeHeightOffset = 3f;

		[ServerVar(Saved = true)]
		public static float optimalPlanterQualitySaturation = 0.6f;

		[ServerVar]
		public static float metabolismtick = 1f;

		[ServerVar]
		public static float modifierTickRate = 1f;

		[ServerVar(Saved = true)]
		public static bool woundingenabled = true;

		[ServerVar(Saved = true)]
		public static bool playerserverfall = true;

		[ServerVar]
		public static bool plantlightdetection = true;

		[ServerVar]
		public static float respawnresetrange = 50f;

		[ServerVar]
		public static int maxunack = 4;

		[ServerVar]
		public static bool netcache = true;

		[ServerVar]
		public static bool corpses = true;

		[ServerVar]
		public static bool events = true;

		[ServerVar]
		public static bool dropitems = true;

		[ServerVar]
		public static int netcachesize = 0;

		[ServerVar]
		public static int savecachesize = 0;

		[ServerVar]
		public static int combatlogsize = 30;

		[ServerVar]
		public static int combatlogdelay = 10;

		[ServerVar]
		public static int authtimeout = 60;

		[ServerVar]
		public static int playertimeout = 60;

		[ServerVar]
		public static int idlekick = 30;

		[ServerVar]
		public static int idlekickmode = 1;

		[ServerVar]
		public static int idlekickadmins = 0;

		[ServerVar(Help = "Censors the Steam player list to make player tracking more difficult")]
		public static bool censorplayerlist = false;

		[ServerVar(Saved = true)]
		public static bool showHolsteredItems = true;

		[ServerVar]
		public static int maxpacketspersecond_world = 1;

		[ServerVar]
		public static int maxpacketspersecond_rpc = 200;

		[ServerVar]
		public static int maxpacketspersecond_command = 100;

		[ServerVar]
		public static int maxpacketsize_command = 100000;

		[ServerVar]
		public static int maxpacketspersecond_tick = 300;

		[ServerVar]
		public static int maxpacketspersecond_voice = 100;

		[ServerVar]
		public static bool packetlog_enabled = false;

		[ServerVar]
		public static bool rpclog_enabled = false;

		[ServerVar]
		public static float maxreceivetime
		{
			get
			{
				return Facepunch.Network.Raknet.Server.MaxReceiveTime;
			}
			set
			{
				Facepunch.Network.Raknet.Server.MaxReceiveTime = Mathf.Clamp(value, 1f, 1000f);
			}
		}

		[ServerVar]
		public static int maxpacketspersecond
		{
			get
			{
				return (int)Network.Server.MaxPacketsPerSecond;
			}
			set
			{
				Network.Server.MaxPacketsPerSecond = (ulong)Mathf.Clamp(value, 1, 1000000);
			}
		}

		[ServerVar]
		public static int maxpacketsize
		{
			get
			{
				return Network.Server.MaxPacketSize;
			}
			set
			{
				Network.Server.MaxPacketSize = Mathf.Clamp(value, 1, 1000000000);
			}
		}

		public static string rootFolder => "server/" + identity;

		public static string backupFolder => "backup/0/" + identity;

		public static string backupFolder1 => "backup/1/" + identity;

		public static string backupFolder2 => "backup/2/" + identity;

		public static string backupFolder3 => "backup/3/" + identity;

		[ServerVar]
		public static bool compression
		{
			get
			{
				if (Network.Net.sv == null)
				{
					return false;
				}
				return Network.Net.sv.compressionEnabled;
			}
			set
			{
				Network.Net.sv.compressionEnabled = value;
			}
		}

		[ServerVar]
		public static bool netlog
		{
			get
			{
				if (Network.Net.sv == null)
				{
					return false;
				}
				return Network.Net.sv.logging;
			}
			set
			{
				Network.Net.sv.logging = value;
			}
		}

		public static float TickDelta()
		{
			return 1f / (float)tickrate;
		}

		public static float TickTime(uint tick)
		{
			return (float)((double)TickDelta() * (double)tick);
		}

		[ServerVar(Help = "Show holstered items on player bodies")]
		public static void setshowholstereditems(Arg arg)
		{
			showHolsteredItems = arg.GetBool(0, showHolsteredItems);
			foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
			{
				activePlayer.inventory.UpdatedVisibleHolsteredItems();
			}
			foreach (BasePlayer sleepingPlayer in BasePlayer.sleepingPlayerList)
			{
				sleepingPlayer.inventory.UpdatedVisibleHolsteredItems();
			}
		}

		[ServerVar]
		public static string packetlog(Arg arg)
		{
			if (!packetlog_enabled)
			{
				return "Packet log is not enabled.";
			}
			List<Tuple<Message.Type, ulong>> list = new List<Tuple<Message.Type, ulong>>();
			foreach (KeyValuePair<Message.Type, TimeAverageValue> item in SingletonComponent<ServerMgr>.Instance.packetHistory.dict)
			{
				list.Add(new Tuple<Message.Type, ulong>(item.Key, item.Value.Calculate()));
			}
			TextTable textTable = new TextTable();
			textTable.AddColumn("type");
			textTable.AddColumn("calls");
			foreach (Tuple<Message.Type, ulong> item2 in list.OrderByDescending((Tuple<Message.Type, ulong> entry) => entry.Item2))
			{
				if (item2.Item2 == 0L)
				{
					break;
				}
				string text = item2.Item1.ToString();
				string text2 = item2.Item2.ToString();
				textTable.AddRow(text, text2);
			}
			return textTable.ToString();
		}

		[ServerVar]
		public static string rpclog(Arg arg)
		{
			if (!rpclog_enabled)
			{
				return "RPC log is not enabled.";
			}
			List<Tuple<uint, ulong>> list = new List<Tuple<uint, ulong>>();
			foreach (KeyValuePair<uint, TimeAverageValue> item in SingletonComponent<ServerMgr>.Instance.rpcHistory.dict)
			{
				list.Add(new Tuple<uint, ulong>(item.Key, item.Value.Calculate()));
			}
			TextTable textTable = new TextTable();
			textTable.AddColumn("id");
			textTable.AddColumn("name");
			textTable.AddColumn("calls");
			foreach (Tuple<uint, ulong> item2 in list.OrderByDescending((Tuple<uint, ulong> entry) => entry.Item2))
			{
				if (item2.Item2 == 0L)
				{
					break;
				}
				string text = item2.Item1.ToString();
				string text2 = StringPool.Get(item2.Item1);
				string text3 = item2.Item2.ToString();
				textTable.AddRow(text, text2, text3);
			}
			return textTable.ToString();
		}

		[ServerVar(Help = "Starts a server")]
		public static void start(Arg arg)
		{
			if (Network.Net.sv.IsConnected())
			{
				arg.ReplyWith("There is already a server running!");
				return;
			}
			string @string = arg.GetString(0, level);
			if (!LevelManager.IsValid(@string))
			{
				arg.ReplyWith("Level '" + @string + "' isn't valid!");
				return;
			}
			if ((bool)UnityEngine.Object.FindObjectOfType<ServerMgr>())
			{
				arg.ReplyWith("There is already a server running!");
				return;
			}
			UnityEngine.Object.DontDestroyOnLoad(GameManager.server.CreatePrefab("assets/bundled/prefabs/system/server.prefab"));
			LevelManager.LoadLevel(@string);
		}

		[ServerVar(Help = "Stops a server")]
		public static void stop(Arg arg)
		{
			if (!Network.Net.sv.IsConnected())
			{
				arg.ReplyWith("There isn't a server running!");
			}
			else
			{
				Network.Net.sv.Stop(arg.GetString(0, "Stopping Server"));
			}
		}

		[ServerVar(Help = "Backup server folder")]
		public static void backup()
		{
			DirectoryEx.Backup(backupFolder, backupFolder1, backupFolder2, backupFolder3);
			DirectoryEx.CopyAll(rootFolder, backupFolder);
		}

		public static string GetServerFolder(string folder)
		{
			string text = rootFolder + "/" + folder;
			if (Directory.Exists(text))
			{
				return text;
			}
			Directory.CreateDirectory(text);
			return text;
		}

		[ServerVar(Help = "Writes config files")]
		public static void writecfg(Arg arg)
		{
			string contents = ConsoleSystem.SaveToConfigString(true);
			File.WriteAllText(GetServerFolder("cfg") + "/serverauto.cfg", contents);
			ServerUsers.Save();
			arg.ReplyWith("Config Saved");
		}

		[ServerVar]
		public static void fps(Arg arg)
		{
			arg.ReplyWith(Performance.report.frameRate.ToString() + " FPS");
		}

		[ServerVar(Help = "Force save the current game")]
		public static void save(Arg arg)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			foreach (BaseEntity save in BaseEntity.saveList)
			{
				save.InvalidateNetworkCache();
			}
			UnityEngine.Debug.Log("Invalidate Network Cache took " + stopwatch.Elapsed.TotalSeconds.ToString("0.00") + " seconds");
			SaveRestore.Save(true);
		}

		[ServerVar]
		public static string readcfg(Arg arg)
		{
			string serverFolder = GetServerFolder("cfg");
			if (File.Exists(serverFolder + "/serverauto.cfg"))
			{
				string strFile = File.ReadAllText(serverFolder + "/serverauto.cfg");
				ConsoleSystem.RunFile(Option.Server.Quiet(), strFile);
			}
			if (File.Exists(serverFolder + "/server.cfg"))
			{
				string strFile2 = File.ReadAllText(serverFolder + "/server.cfg");
				ConsoleSystem.RunFile(Option.Server.Quiet(), strFile2);
			}
			return "Server Config Loaded";
		}

		[ServerUserVar]
		public static void cheatreport(Arg arg)
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (!(basePlayer == null))
			{
				ulong uInt = arg.GetUInt64(0, 0uL);
				string @string = arg.GetString(1);
				UnityEngine.Debug.LogWarning(basePlayer + " reported " + uInt + ": " + @string.ToPrintable(140));
				if (EACServer.eacScout != null)
				{
					EACServer.eacScout.SendPlayerReport(uInt.ToString(), basePlayer.net.connection.userid.ToString(), PlayerReportCategory.PlayerReportCheating, @string);
				}
			}
		}

		[ServerAllVar(Help = "Get the player combat log")]
		public static string combatlog(Arg arg)
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (arg.HasArgs() && arg.IsAdmin)
			{
				basePlayer = ArgEx.GetPlayerOrSleeper(arg, 0);
			}
			if (basePlayer == null)
			{
				return "invalid player";
			}
			return basePlayer.stats.combat.Get(combatlogsize);
		}

		[ServerVar(Help = "Print the current player position.")]
		public static string printpos(Arg arg)
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (arg.HasArgs())
			{
				basePlayer = ArgEx.GetPlayerOrSleeper(arg, 0);
			}
			if (!(basePlayer == null))
			{
				return basePlayer.transform.position.ToString();
			}
			return "invalid player";
		}

		[ServerVar(Help = "Print the current player rotation.")]
		public static string printrot(Arg arg)
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (arg.HasArgs())
			{
				basePlayer = ArgEx.GetPlayerOrSleeper(arg, 0);
			}
			if (!(basePlayer == null))
			{
				return basePlayer.transform.rotation.eulerAngles.ToString();
			}
			return "invalid player";
		}

		[ServerVar(Help = "Print the current player eyes.")]
		public static string printeyes(Arg arg)
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (arg.HasArgs())
			{
				basePlayer = ArgEx.GetPlayerOrSleeper(arg, 0);
			}
			if (!(basePlayer == null))
			{
				return basePlayer.eyes.rotation.eulerAngles.ToString();
			}
			return "invalid player";
		}

		[ServerVar(ServerAdmin = false, Help = "This sends a snapshot of all the entities in the client's pvs. This is mostly redundant, but we request this when the client starts recording a demo.. so they get all the information.")]
		public static void snapshot(Arg arg)
		{
			if (!(ArgEx.Player(arg) == null))
			{
				UnityEngine.Debug.Log("Sending full snapshot to " + ArgEx.Player(arg));
				ArgEx.Player(arg).SendNetworkUpdateImmediate();
				ArgEx.Player(arg).SendGlobalSnapshot();
				ArgEx.Player(arg).SendFullSnapshot();
			}
		}

		[ServerVar(Help = "Send network update for all players")]
		public static void sendnetworkupdate(Arg arg)
		{
			foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
			{
				activePlayer.SendNetworkUpdate();
			}
		}
	}
}
