using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CompanionServer;
using ConVar;
using Facepunch;
using Facepunch.Math;
using Facepunch.Network;
using Ionic.Crc;
using Network;
using Network.Visibility;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class ServerMgr : SingletonComponent<ServerMgr>, IServerCallback
{
	public ConnectionQueue connectionQueue = new ConnectionQueue();

	public TimeAverageValueLookup<Message.Type> packetHistory = new TimeAverageValueLookup<Message.Type>();

	public TimeAverageValueLookup<uint> rpcHistory = new TimeAverageValueLookup<uint>();

	private Stopwatch queryTimer = Stopwatch.StartNew();

	private Dictionary<uint, int> unconnectedQueries = new Dictionary<uint, int>();

	private Stopwatch queriesPerSeconTimer = Stopwatch.StartNew();

	private int NumQueriesLastSecond;

	private MemoryStream queryBuffer = new MemoryStream();

	public const string BYPASS_PROCEDURAL_SPAWN_PREF = "bypassProceduralSpawn";

	private ConnectionAuth auth;

	private bool useQueryPort;

	public UserPersistance persistance;

	public PlayerStateManager playerStateManager;

	private AIThinkManager.QueueType aiTick;

	private List<ulong> bannedPlayerNotices = new List<ulong>();

	private string _AssemblyHash;

	public IEnumerator restartCoroutine;

	public bool runFrameUpdate { get; private set; }

	public static int AvailableSlots => ConVar.Server.maxplayers - BasePlayer.activePlayerList.Count;

	private string AssemblyHash
	{
		get
		{
			if (_AssemblyHash == null)
			{
				string location = typeof(ServerMgr).Assembly.Location;
				if (!string.IsNullOrEmpty(location))
				{
					byte[] array = File.ReadAllBytes(location);
					CRC32 cRC = new CRC32();
					cRC.SlurpBlock(array, 0, array.Length);
					_AssemblyHash = cRC.Crc32Result.ToString("x");
				}
				else
				{
					_AssemblyHash = "il2cpp";
				}
			}
			return _AssemblyHash;
		}
	}

	public bool Restarting => restartCoroutine != null;

	private void Log(Exception e)
	{
		if (ConVar.Global.developer > 0)
		{
			UnityEngine.Debug.LogException(e);
		}
	}

	public void OnNetworkMessage(Message packet)
	{
		if (ConVar.Server.packetlog_enabled)
		{
			packetHistory.Increment(packet.type);
		}
		switch (packet.type)
		{
		case Message.Type.GiveUserInformation:
			if (packet.connection.GetPacketsPerSecond(packet.type) >= 1)
			{
				Network.Net.sv.Kick(packet.connection, "Packet Flooding: User Information", packet.connection.connected);
				break;
			}
			using (TimeWarning.New("GiveUserInformation", 20))
			{
				try
				{
					OnGiveUserInformation(packet);
				}
				catch (Exception e7)
				{
					Log(e7);
					Network.Net.sv.Kick(packet.connection, "Invalid Packet: User Information");
				}
			}
			packet.connection.AddPacketsPerSecond(packet.type);
			break;
		case Message.Type.Ready:
			if (!packet.connection.connected)
			{
				break;
			}
			if (packet.connection.GetPacketsPerSecond(packet.type) >= 1)
			{
				Network.Net.sv.Kick(packet.connection, "Packet Flooding: Client Ready", packet.connection.connected);
				break;
			}
			using (TimeWarning.New("ClientReady", 20))
			{
				try
				{
					ClientReady(packet);
				}
				catch (Exception e9)
				{
					Log(e9);
					Network.Net.sv.Kick(packet.connection, "Invalid Packet: Client Ready");
				}
			}
			packet.connection.AddPacketsPerSecond(packet.type);
			break;
		case Message.Type.RPCMessage:
			if (!packet.connection.connected)
			{
				break;
			}
			if (packet.connection.GetPacketsPerSecond(packet.type) >= (ulong)ConVar.Server.maxpacketspersecond_rpc)
			{
				Network.Net.sv.Kick(packet.connection, "Packet Flooding: RPC Message");
				break;
			}
			using (TimeWarning.New("OnRPCMessage", 20))
			{
				try
				{
					OnRPCMessage(packet);
				}
				catch (Exception e8)
				{
					Log(e8);
					Network.Net.sv.Kick(packet.connection, "Invalid Packet: RPC Message");
				}
			}
			packet.connection.AddPacketsPerSecond(packet.type);
			break;
		case Message.Type.ConsoleCommand:
			if (!packet.connection.connected)
			{
				break;
			}
			if (packet.connection.GetPacketsPerSecond(packet.type) >= (ulong)ConVar.Server.maxpacketspersecond_command)
			{
				Network.Net.sv.Kick(packet.connection, "Packet Flooding: Client Command", packet.connection.connected);
				break;
			}
			using (TimeWarning.New("OnClientCommand", 20))
			{
				try
				{
					ConsoleNetwork.OnClientCommand(packet);
				}
				catch (Exception e5)
				{
					Log(e5);
					Network.Net.sv.Kick(packet.connection, "Invalid Packet: Client Command");
				}
			}
			packet.connection.AddPacketsPerSecond(packet.type);
			break;
		case Message.Type.DisconnectReason:
			if (!packet.connection.connected)
			{
				break;
			}
			if (packet.connection.GetPacketsPerSecond(packet.type) >= 1)
			{
				Network.Net.sv.Kick(packet.connection, "Packet Flooding: Disconnect Reason", packet.connection.connected);
				break;
			}
			using (TimeWarning.New("ReadDisconnectReason", 20))
			{
				try
				{
					ReadDisconnectReason(packet);
					Network.Net.sv.Disconnect(packet.connection);
				}
				catch (Exception e2)
				{
					Log(e2);
					Network.Net.sv.Kick(packet.connection, "Invalid Packet: Disconnect Reason");
				}
			}
			packet.connection.AddPacketsPerSecond(packet.type);
			break;
		case Message.Type.Tick:
			if (!packet.connection.connected)
			{
				break;
			}
			if (packet.connection.GetPacketsPerSecond(packet.type) >= (ulong)ConVar.Server.maxpacketspersecond_tick)
			{
				Network.Net.sv.Kick(packet.connection, "Packet Flooding: Player Tick", packet.connection.connected);
				break;
			}
			using (TimeWarning.New("OnPlayerTick", 20))
			{
				try
				{
					OnPlayerTick(packet);
				}
				catch (Exception e4)
				{
					Log(e4);
					Network.Net.sv.Kick(packet.connection, "Invalid Packet: Player Tick");
				}
			}
			packet.connection.AddPacketsPerSecond(packet.type);
			break;
		case Message.Type.EAC:
			using (TimeWarning.New("OnEACMessage", 20))
			{
				try
				{
					EACServer.OnMessageReceived(packet);
					break;
				}
				catch (Exception e3)
				{
					Log(e3);
					Network.Net.sv.Kick(packet.connection, "Invalid Packet: EAC");
					break;
				}
			}
		case Message.Type.World:
			if (!World.Transfer || !packet.connection.connected)
			{
				break;
			}
			if (packet.connection.GetPacketsPerSecond(packet.type) >= (ulong)ConVar.Server.maxpacketspersecond_world)
			{
				Network.Net.sv.Kick(packet.connection, "Packet Flooding: World", packet.connection.connected);
				break;
			}
			using (TimeWarning.New("OnWorldMessage", 20))
			{
				try
				{
					WorldNetworking.OnMessageReceived(packet);
					break;
				}
				catch (Exception e6)
				{
					Log(e6);
					Network.Net.sv.Kick(packet.connection, "Invalid Packet: World");
					break;
				}
			}
		case Message.Type.VoiceData:
			if (!packet.connection.connected)
			{
				break;
			}
			if (packet.connection.GetPacketsPerSecond(packet.type) >= (ulong)ConVar.Server.maxpacketspersecond_voice)
			{
				Network.Net.sv.Kick(packet.connection, "Packet Flooding: Disconnect Reason", packet.connection.connected);
				break;
			}
			using (TimeWarning.New("OnPlayerVoice", 20))
			{
				try
				{
					OnPlayerVoice(packet);
				}
				catch (Exception e)
				{
					Log(e);
					Network.Net.sv.Kick(packet.connection, "Invalid Packet: Player Voice");
				}
			}
			packet.connection.AddPacketsPerSecond(packet.type);
			break;
		default:
			ProcessUnhandledPacket(packet);
			break;
		}
	}

	public void ProcessUnhandledPacket(Message packet)
	{
		if (ConVar.Global.developer > 0)
		{
			UnityEngine.Debug.LogWarning("[SERVER][UNHANDLED] " + packet.type);
		}
		Network.Net.sv.Kick(packet.connection, "Sent Unhandled Message");
	}

	public void ReadDisconnectReason(Message packet)
	{
		string text = packet.read.String(4096);
		string text2 = packet.connection.ToString();
		if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2))
		{
			Interface.CallHook("OnClientDisconnect", packet.connection, text);
			DebugEx.Log(text2 + " disconnecting: " + text);
		}
	}

	private BasePlayer SpawnPlayerSleeping(Network.Connection connection)
	{
		BasePlayer basePlayer = BasePlayer.FindSleeping(connection.userid);
		if (basePlayer == null)
		{
			return null;
		}
		if (!basePlayer.IsSleeping())
		{
			UnityEngine.Debug.LogWarning("Player spawning into sleeper that isn't sleeping!");
			basePlayer.Kill();
			return null;
		}
		basePlayer.PlayerInit(connection);
		basePlayer.inventory.SendSnapshot();
		DebugEx.Log(basePlayer.net.connection.ToString() + " joined [" + basePlayer.net.connection.os + "/" + basePlayer.net.connection.ownerid + "]");
		return basePlayer;
	}

	private BasePlayer SpawnNewPlayer(Network.Connection connection)
	{
		BasePlayer.SpawnPoint spawnPoint = FindSpawnPoint();
		BasePlayer basePlayer = GameManager.server.CreateEntity("assets/prefabs/player/player.prefab", spawnPoint.pos, spawnPoint.rot).ToPlayer();
		if (Interface.CallHook("OnPlayerSpawn", basePlayer, connection) != null)
		{
			return null;
		}
		basePlayer.health = 0f;
		basePlayer.lifestate = BaseCombatEntity.LifeState.Dead;
		basePlayer.ResetLifeStateOnSpawn = false;
		basePlayer.limitNetworking = true;
		basePlayer.Spawn();
		basePlayer.limitNetworking = false;
		basePlayer.PlayerInit(connection);
		if ((bool)BaseGameMode.GetActiveGameMode(serverside: true))
		{
			BaseGameMode.GetActiveGameMode(serverside: true).OnNewPlayer(basePlayer);
		}
		else if (UnityEngine.Application.isEditor || (SleepingBag.FindForPlayer(basePlayer.userID, ignoreTimers: true).Length == 0 && !basePlayer.hasPreviousLife))
		{
			basePlayer.Respawn();
		}
		else
		{
			basePlayer.SendRespawnOptions();
		}
		DebugEx.Log($"{basePlayer.displayName} with steamid {basePlayer.userID} joined from ip {basePlayer.net.connection.ipaddress}");
		DebugEx.Log($"\tNetworkId {basePlayer.userID} is {basePlayer.net.ID} ({basePlayer.displayName})");
		if (basePlayer.net.connection.ownerid != basePlayer.net.connection.userid)
		{
			DebugEx.Log($"\t{basePlayer} is sharing the account {basePlayer.net.connection.ownerid}");
		}
		return basePlayer;
	}

	private void ClientReady(Message packet)
	{
		if (packet.connection.state != Network.Connection.State.Welcoming)
		{
			Network.Net.sv.Kick(packet.connection, "Invalid connection state");
			return;
		}
		packet.connection.decryptIncoming = true;
		using (ClientReady clientReady = ProtoBuf.ClientReady.Deserialize(packet.read))
		{
			foreach (ClientReady.ClientInfo item in clientReady.clientInfo)
			{
				Interface.CallHook("OnPlayerSetInfo", packet.connection, item.name, item.value);
				packet.connection.info.Set(item.name, item.value);
			}
			connectionQueue.JoinedGame(packet.connection);
			using (TimeWarning.New("ClientReady"))
			{
				BasePlayer basePlayer;
				using (TimeWarning.New("SpawnPlayerSleeping"))
				{
					basePlayer = SpawnPlayerSleeping(packet.connection);
				}
				if (basePlayer == null)
				{
					using (TimeWarning.New("SpawnNewPlayer"))
					{
						basePlayer = SpawnNewPlayer(packet.connection);
					}
				}
				if (basePlayer != null)
				{
					Util.SendSignedInNotification(basePlayer);
				}
			}
		}
		SendReplicatedVars(packet.connection);
	}

	private void OnRPCMessage(Message packet)
	{
		uint uid = packet.read.UInt32();
		uint num = packet.read.UInt32();
		if (ConVar.Server.rpclog_enabled)
		{
			rpcHistory.Increment(num);
		}
		BaseEntity baseEntity = BaseNetworkable.serverEntities.Find(uid) as BaseEntity;
		if (!(baseEntity == null))
		{
			baseEntity.SV_RPCMessage(num, packet);
		}
	}

	private void OnPlayerTick(Message packet)
	{
		BasePlayer basePlayer = NetworkPacketEx.Player(packet);
		if (!(basePlayer == null))
		{
			basePlayer.OnReceivedTick(packet.read);
		}
	}

	private void OnPlayerVoice(Message packet)
	{
		BasePlayer basePlayer = NetworkPacketEx.Player(packet);
		if (!(basePlayer == null))
		{
			basePlayer.OnReceivedVoice(packet.read.BytesWithSize());
		}
	}

	private void OnGiveUserInformation(Message packet)
	{
		if (packet.connection.state != 0)
		{
			Network.Net.sv.Kick(packet.connection, "Invalid connection state");
			return;
		}
		packet.connection.state = Network.Connection.State.Connecting;
		if (packet.read.UInt8() != 228)
		{
			Network.Net.sv.Kick(packet.connection, "Invalid Connection Protocol");
			return;
		}
		packet.connection.userid = packet.read.UInt64();
		packet.connection.protocol = packet.read.UInt32();
		packet.connection.os = packet.read.String(128);
		packet.connection.username = packet.read.String();
		if (string.IsNullOrEmpty(packet.connection.os))
		{
			throw new Exception("Invalid OS");
		}
		if (string.IsNullOrEmpty(packet.connection.username))
		{
			Network.Net.sv.Kick(packet.connection, "Invalid Username");
			return;
		}
		packet.connection.username = packet.connection.username.Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ')
			.Trim();
		if (string.IsNullOrEmpty(packet.connection.username))
		{
			Network.Net.sv.Kick(packet.connection, "Invalid Username");
			return;
		}
		string text = string.Empty;
		string branch = ConVar.Server.branch;
		if (packet.read.Unread >= 4)
		{
			text = packet.read.String(128);
		}
		Interface.CallHook("OnClientAuth", packet.connection);
		if (branch != string.Empty && branch != text)
		{
			DebugEx.Log(string.Concat("Kicking ", packet.connection, " - their branch is '", text, "' not '", branch, "'"));
			Network.Net.sv.Kick(packet.connection, "Wrong Steam Beta: Requires '" + branch + "' branch!");
		}
		else if (packet.connection.protocol > 2367)
		{
			DebugEx.Log(string.Concat("Kicking ", packet.connection, " - their protocol is ", packet.connection.protocol, " not ", 2367));
			Network.Net.sv.Kick(packet.connection, "Wrong Connection Protocol: Server update required!");
		}
		else if (packet.connection.protocol < 2367)
		{
			DebugEx.Log(string.Concat("Kicking ", packet.connection, " - their protocol is ", packet.connection.protocol, " not ", 2367));
			Network.Net.sv.Kick(packet.connection, "Wrong Connection Protocol: Client update required!");
		}
		else
		{
			packet.connection.token = packet.read.BytesWithSize(512u);
			if (packet.connection.token == null || packet.connection.token.Length < 1)
			{
				Network.Net.sv.Kick(packet.connection, "Invalid Token");
			}
			else
			{
				auth.OnNewConnection(packet.connection);
			}
		}
	}

	public bool OnUnconnectedMessage(int type, NetRead read, uint ip, int port)
	{
		if (useQueryPort)
		{
			return false;
		}
		if (type == 255)
		{
			if (queriesPerSeconTimer.Elapsed.TotalSeconds > 1.0)
			{
				queriesPerSeconTimer.Reset();
				queriesPerSeconTimer.Start();
				NumQueriesLastSecond = 0;
			}
			if (NumQueriesLastSecond > ConVar.Server.queriesPerSecond)
			{
				return false;
			}
			if (read.UInt8() != byte.MaxValue)
			{
				return false;
			}
			if (read.UInt8() != byte.MaxValue)
			{
				return false;
			}
			if (read.UInt8() != byte.MaxValue)
			{
				return false;
			}
			if (queryTimer.Elapsed.TotalSeconds > 60.0)
			{
				queryTimer.Reset();
				queryTimer.Start();
				unconnectedQueries.Clear();
			}
			if (!unconnectedQueries.ContainsKey(ip))
			{
				unconnectedQueries.Add(ip, 0);
			}
			int num = unconnectedQueries[ip] + 1;
			unconnectedQueries[ip] = num;
			if (num > ConVar.Server.ipQueriesPerMin)
			{
				return true;
			}
			NumQueriesLastSecond++;
			read.Position = 0L;
			int unread = read.Unread;
			if (unread > 4096)
			{
				return true;
			}
			if (queryBuffer.Capacity < unread)
			{
				queryBuffer.Capacity = unread;
			}
			int size = read.Read(queryBuffer.GetBuffer(), 0, unread);
			SteamServer.HandleIncomingPacket(queryBuffer.GetBuffer(), size, ip, (ushort)port);
			return true;
		}
		return false;
	}

	public void Initialize(bool loadSave = true, string saveFile = "", bool allowOutOfDateSaves = false, bool skipInitialSpawn = false)
	{
		Interface.CallHook("OnServerInitialize");
		persistance = new UserPersistance(ConVar.Server.rootFolder);
		playerStateManager = new PlayerStateManager(persistance);
		SpawnMapEntities();
		if ((bool)SingletonComponent<SpawnHandler>.Instance)
		{
			using (TimeWarning.New("SpawnHandler.UpdateDistributions"))
			{
				SingletonComponent<SpawnHandler>.Instance.UpdateDistributions();
			}
		}
		if (loadSave)
		{
			World.LoadedFromSave = true;
			World.LoadedFromSave = (skipInitialSpawn = SaveRestore.Load(saveFile, allowOutOfDateSaves));
		}
		else
		{
			SaveRestore.SaveCreatedTime = DateTime.UtcNow;
			World.LoadedFromSave = false;
		}
		if ((bool)SingletonComponent<SpawnHandler>.Instance)
		{
			if (!skipInitialSpawn)
			{
				using (TimeWarning.New("SpawnHandler.InitialSpawn", 200))
				{
					SingletonComponent<SpawnHandler>.Instance.InitialSpawn();
				}
			}
			using (TimeWarning.New("SpawnHandler.StartSpawnTick", 200))
			{
				SingletonComponent<SpawnHandler>.Instance.StartSpawnTick();
			}
		}
		CreateImportantEntities();
		auth = GetComponent<ConnectionAuth>();
	}

	public void OpenConnection()
	{
		useQueryPort = ConVar.Server.queryport > 0 && ConVar.Server.queryport != ConVar.Server.port;
		if (!useQueryPort && !Network.Net.sv.AllowPassthroughMessages)
		{
			ConVar.Server.queryport = Math.Max(ConVar.Server.port, RCon.Port) + 1;
			useQueryPort = true;
		}
		Network.Net.sv.ip = ConVar.Server.ip;
		Network.Net.sv.port = ConVar.Server.port;
		StartSteamServer();
		if (!Network.Net.sv.Start())
		{
			UnityEngine.Debug.LogWarning("Couldn't Start Server.");
			CloseConnection();
			return;
		}
		Network.Net.sv.callbackHandler = this;
		Network.Net.sv.cryptography = new NetworkCryptographyServer();
		EACServer.DoStartup();
		InvokeRepeating("DoTick", 1f, 1f / (float)ConVar.Server.tickrate);
		InvokeRepeating("DoHeartbeat", 1f, 1f);
		runFrameUpdate = true;
		ConsoleSystem.OnReplicatedVarChanged += OnReplicatedVarChanged;
		Interface.CallHook("IOnServerInitialized");
	}

	private void CloseConnection()
	{
		if (persistance != null)
		{
			persistance.Dispose();
			persistance = null;
		}
		EACServer.DoShutdown();
		Network.Net.sv.callbackHandler = null;
		using (TimeWarning.New("sv.Stop"))
		{
			Network.Net.sv.Stop("Shutting Down");
		}
		using (TimeWarning.New("RCon.Shutdown"))
		{
			RCon.Shutdown();
		}
		using (TimeWarning.New("PlatformService.Shutdown"))
		{
			PlatformService.Instance?.Shutdown();
		}
		using (TimeWarning.New("CompanionServer.Shutdown"))
		{
			CompanionServer.Server.Shutdown();
		}
		ConsoleSystem.OnReplicatedVarChanged -= OnReplicatedVarChanged;
	}

	private void OnDisable()
	{
		if (!Rust.Application.isQuitting)
		{
			CloseConnection();
		}
	}

	private void OnApplicationQuit()
	{
		Rust.Application.isQuitting = true;
		CloseConnection();
	}

	private void CreateImportantEntities()
	{
		CreateImportantEntity<EnvSync>("assets/bundled/prefabs/system/net_env.prefab");
		CreateImportantEntity<CommunityEntity>("assets/bundled/prefabs/system/server/community.prefab");
		CreateImportantEntity<ResourceDepositManager>("assets/bundled/prefabs/system/server/resourcedepositmanager.prefab");
		CreateImportantEntity<RelationshipManager>("assets/bundled/prefabs/system/server/relationship_manager.prefab");
		CreateImportantEntity<TreeManager>("assets/bundled/prefabs/system/tree_manager.prefab");
	}

	public void CreateImportantEntity<T>(string prefabName) where T : BaseEntity
	{
		if (!BaseNetworkable.serverEntities.Any((BaseNetworkable x) => x is T))
		{
			UnityEngine.Debug.LogWarning("Missing " + typeof(T).Name + " - creating");
			BaseEntity baseEntity = GameManager.server.CreateEntity(prefabName);
			if (baseEntity == null)
			{
				UnityEngine.Debug.LogWarning("Couldn't create");
			}
			else
			{
				baseEntity.Spawn();
			}
		}
	}

	private void StartSteamServer()
	{
		PlatformService.Instance.Initialize(RustPlatformHooks.Instance);
		InvokeRepeating("UpdateServerInformation", 2f, 30f);
		InvokeRepeating("UpdateItemDefinitions", 10f, 3600f);
		DebugEx.Log("SteamServer Initialized");
	}

	private void UpdateItemDefinitions()
	{
		UnityEngine.Debug.Log("Checking for new Steam Item Definitions..");
		PlatformService.Instance.RefreshItemDefinitions();
	}

	internal void OnValidateAuthTicketResponse(ulong SteamId, ulong OwnerId, AuthResponse Status)
	{
		if (Auth_Steam.ValidateConnecting(SteamId, OwnerId, Status))
		{
			return;
		}
		Network.Connection connection = Network.Net.sv.connections.FirstOrDefault((Network.Connection x) => x.userid == SteamId);
		if (connection == null)
		{
			UnityEngine.Debug.LogWarning($"Steam gave us a {Status} ticket response for unconnected id {SteamId}");
			return;
		}
		switch (Status)
		{
		case AuthResponse.OK:
			UnityEngine.Debug.LogWarning($"Steam gave us a 'ok' ticket response for already connected id {SteamId}");
			return;
		case AuthResponse.TimedOut:
			return;
		case AuthResponse.VACBanned:
		case AuthResponse.PublisherBanned:
			if (!bannedPlayerNotices.Contains(SteamId))
			{
				Interface.CallHook("IOnPlayerBanned", connection, Status);
				ConsoleNetwork.BroadcastToAllClients("chat.add", 2, 0, "<color=#fff>SERVER</color> Kicking " + connection.username.EscapeRichText() + " (banned by anticheat)");
				bannedPlayerNotices.Add(SteamId);
			}
			break;
		}
		UnityEngine.Debug.Log($"Kicking {connection.ipaddress}/{connection.userid}/{connection.username} (Steam Status \"{Status.ToString()}\")");
		connection.authStatus = Status.ToString();
		Network.Net.sv.Kick(connection, "Steam: " + Status);
	}

	private void Update()
	{
		if (!runFrameUpdate)
		{
			return;
		}
		using (TimeWarning.New("ServerMgr.Update", 500))
		{
			try
			{
				using (TimeWarning.New("EACServer.DoUpdate", 100))
				{
					EACServer.DoUpdate();
				}
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogWarning("Server Exception: EACServer.DoUpdate");
				UnityEngine.Debug.LogException(exception, this);
			}
			try
			{
				using (TimeWarning.New("PlatformService.Update", 100))
				{
					PlatformService.Instance.Update();
				}
			}
			catch (Exception exception2)
			{
				UnityEngine.Debug.LogWarning("Server Exception: Platform Service Update");
				UnityEngine.Debug.LogException(exception2, this);
			}
			try
			{
				using (TimeWarning.New("Net.sv.Cycle", 100))
				{
					Network.Net.sv.Cycle();
				}
			}
			catch (Exception exception3)
			{
				UnityEngine.Debug.LogWarning("Server Exception: Network Cycle");
				UnityEngine.Debug.LogException(exception3, this);
			}
			try
			{
				using (TimeWarning.New("ServerBuildingManager.Cycle"))
				{
					BuildingManager.server.Cycle();
				}
			}
			catch (Exception exception4)
			{
				UnityEngine.Debug.LogWarning("Server Exception: Building Manager");
				UnityEngine.Debug.LogException(exception4, this);
			}
			try
			{
				using (TimeWarning.New("BasePlayer.ServerCycle"))
				{
					bool batchsynctransforms = ConVar.Physics.batchsynctransforms;
					bool autosynctransforms = ConVar.Physics.autosynctransforms;
					if (batchsynctransforms && autosynctransforms)
					{
						UnityEngine.Physics.autoSyncTransforms = false;
					}
					if (!UnityEngine.Physics.autoSyncTransforms)
					{
						UnityEngine.Physics.SyncTransforms();
					}
					BasePlayer.ServerCycle(UnityEngine.Time.deltaTime);
					try
					{
						using (TimeWarning.New("FlameTurret.BudgetedUpdate"))
						{
							FlameTurret.updateFlameTurretQueueServer.RunQueue(0.25);
						}
					}
					catch (Exception exception5)
					{
						UnityEngine.Debug.LogWarning("Server Exception: FlameTurret.BudgetedUpdate");
						UnityEngine.Debug.LogException(exception5, this);
					}
					try
					{
						using (TimeWarning.New("AutoTurret.BudgetedUpdate"))
						{
							AutoTurret.updateAutoTurretScanQueue.RunQueue(0.5);
						}
					}
					catch (Exception exception6)
					{
						UnityEngine.Debug.LogWarning("Server Exception: AutoTurret.BudgetedUpdate");
						UnityEngine.Debug.LogException(exception6, this);
					}
					try
					{
						using (TimeWarning.New("BaseFishingRod.BudgetedUpdate"))
						{
							BaseFishingRod.updateFishingRodQueue.RunQueue(1.0);
						}
					}
					catch (Exception exception7)
					{
						UnityEngine.Debug.LogWarning("Server Exception: BaseFishingRod.BudgetedUpdate");
						UnityEngine.Debug.LogException(exception7, this);
					}
					if (batchsynctransforms && autosynctransforms)
					{
						UnityEngine.Physics.autoSyncTransforms = true;
					}
				}
			}
			catch (Exception exception8)
			{
				UnityEngine.Debug.LogWarning("Server Exception: Player Update");
				UnityEngine.Debug.LogException(exception8, this);
			}
			try
			{
				using (TimeWarning.New("SteamQueryResponse"))
				{
					SteamQueryResponse();
				}
			}
			catch (Exception exception9)
			{
				UnityEngine.Debug.LogWarning("Server Exception: Steam Query");
				UnityEngine.Debug.LogException(exception9, this);
			}
			try
			{
				using (TimeWarning.New("connectionQueue.Cycle"))
				{
					connectionQueue.Cycle(AvailableSlots);
				}
			}
			catch (Exception exception10)
			{
				UnityEngine.Debug.LogWarning("Server Exception: Connection Queue");
				UnityEngine.Debug.LogException(exception10, this);
			}
			try
			{
				using (TimeWarning.New("IOEntity.ProcessQueue"))
				{
					IOEntity.ProcessQueue();
				}
			}
			catch (Exception exception11)
			{
				UnityEngine.Debug.LogWarning("Server Exception: IOEntity.ProcessQueue");
				UnityEngine.Debug.LogException(exception11, this);
			}
			if (!AI.spliceupdates)
			{
				aiTick = AIThinkManager.QueueType.Human;
			}
			else
			{
				aiTick = ((aiTick == AIThinkManager.QueueType.Human) ? AIThinkManager.QueueType.Animal : AIThinkManager.QueueType.Human);
			}
			if (aiTick == AIThinkManager.QueueType.Human)
			{
				try
				{
					using (TimeWarning.New("AIThinkManager.ProcessQueue"))
					{
						AIThinkManager.ProcessQueue(AIThinkManager.QueueType.Human);
					}
				}
				catch (Exception exception12)
				{
					UnityEngine.Debug.LogWarning("Server Exception: AIThinkManager.ProcessQueue");
					UnityEngine.Debug.LogException(exception12, this);
				}
				if (!AI.spliceupdates)
				{
					aiTick = AIThinkManager.QueueType.Animal;
				}
			}
			if (aiTick == AIThinkManager.QueueType.Animal)
			{
				try
				{
					using (TimeWarning.New("AIThinkManager.ProcessAnimalQueue"))
					{
						AIThinkManager.ProcessQueue(AIThinkManager.QueueType.Animal);
					}
				}
				catch (Exception exception13)
				{
					UnityEngine.Debug.LogWarning("Server Exception: AIThinkManager.ProcessAnimalQueue");
					UnityEngine.Debug.LogException(exception13, this);
				}
			}
			try
			{
				using (TimeWarning.New("AIThinkManager.ProcessPetQueue"))
				{
					AIThinkManager.ProcessQueue(AIThinkManager.QueueType.Pets);
				}
			}
			catch (Exception exception14)
			{
				UnityEngine.Debug.LogWarning("Server Exception: AIThinkManager.ProcessPetQueue");
				UnityEngine.Debug.LogException(exception14, this);
			}
			try
			{
				using (TimeWarning.New("AIThinkManager.ProcessPetMovementQueue"))
				{
					BasePet.ProcessMovementQueue();
				}
			}
			catch (Exception exception15)
			{
				UnityEngine.Debug.LogWarning("Server Exception: AIThinkManager.ProcessPetMovementQueue");
				UnityEngine.Debug.LogException(exception15, this);
			}
			try
			{
				using (TimeWarning.New("BaseRidableAnimal.ProcessQueue"))
				{
					BaseRidableAnimal.ProcessQueue();
				}
			}
			catch (Exception exception16)
			{
				UnityEngine.Debug.LogWarning("Server Exception: BaseRidableAnimal.ProcessQueue");
				UnityEngine.Debug.LogException(exception16, this);
			}
			try
			{
				using (TimeWarning.New("GrowableEntity.BudgetedUpdate"))
				{
					GrowableEntity.growableEntityUpdateQueue.RunQueue(GrowableEntity.framebudgetms);
				}
			}
			catch (Exception exception17)
			{
				UnityEngine.Debug.LogWarning("Server Exception: GrowableEntity.BudgetedUpdate");
				UnityEngine.Debug.LogException(exception17, this);
			}
			try
			{
				using (TimeWarning.New("BasePlayer.BudgetedLifeStoryUpdate"))
				{
					BasePlayer.lifeStoryQueue.RunQueue(BasePlayer.lifeStoryFramebudgetms);
				}
			}
			catch (Exception exception18)
			{
				UnityEngine.Debug.LogWarning("Server Exception: BasePlayer.BudgetedLifeStoryUpdate");
				UnityEngine.Debug.LogException(exception18, this);
			}
			try
			{
				using (TimeWarning.New("JunkPileWater.UpdateNearbyPlayers"))
				{
					JunkPileWater.junkpileWaterWorkQueue.RunQueue(JunkPileWater.framebudgetms);
				}
			}
			catch (Exception exception19)
			{
				UnityEngine.Debug.LogWarning("Server Exception: JunkPileWater.UpdateNearbyPlayers");
				UnityEngine.Debug.LogException(exception19, this);
			}
		}
	}

	private void LateUpdate()
	{
		if (!runFrameUpdate)
		{
			return;
		}
		using (TimeWarning.New("ServerMgr.LateUpdate", 500))
		{
			if (!Facepunch.Network.SteamNetworking.steamnagleflush)
			{
				return;
			}
			try
			{
				using (TimeWarning.New("Connection.Flush"))
				{
					for (int i = 0; i < Network.Net.sv.connections.Count; i++)
					{
						Network.Net.sv.Flush(Network.Net.sv.connections[i]);
					}
				}
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogWarning("Server Exception: Connection.Flush");
				UnityEngine.Debug.LogException(exception, this);
			}
		}
	}

	private void FixedUpdate()
	{
		using (TimeWarning.New("ServerMgr.FixedUpdate"))
		{
			try
			{
				using (TimeWarning.New("BaseMountable.FixedUpdateCycle"))
				{
					BaseMountable.FixedUpdateCycle();
				}
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogWarning("Server Exception: Mountable Cycle");
				UnityEngine.Debug.LogException(exception, this);
			}
			try
			{
				using (TimeWarning.New("Buoyancy.Cycle"))
				{
					Buoyancy.Cycle();
				}
			}
			catch (Exception exception2)
			{
				UnityEngine.Debug.LogWarning("Server Exception: Buoyancy Cycle");
				UnityEngine.Debug.LogException(exception2, this);
			}
		}
	}

	private void SteamQueryResponse()
	{
		if (!SteamServer.IsValid || !Network.Net.sv.AllowPassthroughMessages)
		{
			return;
		}
		using (TimeWarning.New("SteamGameServer.GetNextOutgoingPacket"))
		{
			OutgoingPacket packet;
			while (SteamServer.GetOutgoingPacket(out packet))
			{
				Network.Net.sv.SendUnconnected(packet.Address, packet.Port, packet.Data, packet.Size);
			}
		}
	}

	private void DoTick()
	{
		Interface.CallHook("OnTick");
		RCon.Update();
		CompanionServer.Server.Update();
		for (int i = 0; i < Network.Net.sv.connections.Count; i++)
		{
			Network.Connection connection = Network.Net.sv.connections[i];
			if (!connection.isAuthenticated && !(connection.GetSecondsConnected() < (float)ConVar.Server.authtimeout))
			{
				Network.Net.sv.Kick(connection, "Authentication Timed Out");
			}
		}
	}

	private void DoHeartbeat()
	{
		ItemManager.Heartbeat();
	}

	private static BaseGameMode Gamemode()
	{
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if (!(activeGameMode != null))
		{
			return null;
		}
		return activeGameMode;
	}

	public static string GamemodeName()
	{
		return Gamemode()?.shortname ?? "rust";
	}

	public static string GamemodeTitle()
	{
		return Gamemode()?.gamemodeTitle ?? "Survival";
	}

	private void UpdateServerInformation()
	{
		if (!SteamServer.IsValid)
		{
			return;
		}
		using (TimeWarning.New("UpdateServerInformation"))
		{
			SteamServer.ServerName = ConVar.Server.hostname;
			SteamServer.MaxPlayers = ConVar.Server.maxplayers;
			SteamServer.Passworded = false;
			SteamServer.MapName = World.GetServerBrowserMapName();
			string text = "stok";
			if (Restarting)
			{
				text = "strst";
			}
			string text2 = $"born{Epoch.FromDateTime(SaveRestore.SaveCreatedTime)}";
			string text3 = $"gm{GamemodeName()}";
			string text4 = (ConVar.Server.pve ? ",pve" : string.Empty);
			string text5 = ConVar.Server.tags?.Trim(',') ?? "";
			string text6 = ((!string.IsNullOrWhiteSpace(text5)) ? ("," + text5) : "");
			SteamServer.GameTags = $"mp{ConVar.Server.maxplayers},cp{BasePlayer.activePlayerList.Count},pt{Network.Net.sv.ProtocolId},qp{SingletonComponent<ServerMgr>.Instance.connectionQueue.Queued},v{2367}{text4}{text6},h{AssemblyHash},{text},{text2},{text3}";
			if (ConVar.Server.description != null && ConVar.Server.description.Length > 100)
			{
				string[] array = ConVar.Server.description.SplitToChunks(100).ToArray();
				for (int i = 0; i < 16; i++)
				{
					if (i < array.Length)
					{
						SteamServer.SetKey($"description_{i:00}", array[i]);
					}
					else
					{
						SteamServer.SetKey($"description_{i:00}", string.Empty);
					}
				}
			}
			else
			{
				SteamServer.SetKey("description_0", ConVar.Server.description);
				for (int j = 1; j < 16; j++)
				{
					SteamServer.SetKey($"description_{j:00}", string.Empty);
				}
			}
			SteamServer.SetKey("hash", AssemblyHash);
			string value = World.Seed.ToString();
			BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
			if (activeGameMode != null && !activeGameMode.ingameMap)
			{
				value = "0";
			}
			SteamServer.SetKey("world.seed", value);
			SteamServer.SetKey("world.size", World.Size.ToString());
			SteamServer.SetKey("pve", ConVar.Server.pve.ToString());
			SteamServer.SetKey("headerimage", ConVar.Server.headerimage);
			SteamServer.SetKey("logoimage", ConVar.Server.logoimage);
			SteamServer.SetKey("url", ConVar.Server.url);
			SteamServer.SetKey("gmn", GamemodeName());
			SteamServer.SetKey("gmt", GamemodeTitle());
			SteamServer.SetKey("uptime", ((int)UnityEngine.Time.realtimeSinceStartup).ToString());
			SteamServer.SetKey("gc_mb", Performance.report.memoryAllocations.ToString());
			SteamServer.SetKey("gc_cl", Performance.report.memoryCollections.ToString());
			SteamServer.SetKey("fps", Performance.report.frameRate.ToString());
			SteamServer.SetKey("fps_avg", Performance.report.frameRateAverage.ToString("0.00"));
			SteamServer.SetKey("ent_cnt", BaseNetworkable.serverEntities.Count.ToString());
			SteamServer.SetKey("build", BuildInfo.Current.Scm.ChangeId);
		}
		Interface.CallHook("OnServerInformationUpdated");
	}

	public void OnDisconnected(string strReason, Network.Connection connection)
	{
		connectionQueue.RemoveConnection(connection);
		ConnectionAuth.OnDisconnect(connection);
		PlatformService.Instance.EndPlayerSession(connection.userid);
		EACServer.OnLeaveGame(connection);
		BasePlayer basePlayer = connection.player as BasePlayer;
		if ((bool)basePlayer)
		{
			Interface.CallHook("OnPlayerDisconnected", basePlayer, strReason);
			basePlayer.OnDisconnected();
		}
	}

	public static void OnEnterVisibility(Network.Connection connection, Group group)
	{
		if (Network.Net.sv.IsConnected() && Network.Net.sv.write.Start())
		{
			Network.Net.sv.write.PacketID(Message.Type.GroupEnter);
			Network.Net.sv.write.GroupID(group.ID);
			Network.Net.sv.write.Send(new SendInfo(connection));
		}
	}

	public static void OnLeaveVisibility(Network.Connection connection, Group group)
	{
		if (Network.Net.sv.IsConnected())
		{
			if (Network.Net.sv.write.Start())
			{
				Network.Net.sv.write.PacketID(Message.Type.GroupLeave);
				Network.Net.sv.write.GroupID(group.ID);
				Network.Net.sv.write.Send(new SendInfo(connection));
			}
			if (Network.Net.sv.write.Start())
			{
				Network.Net.sv.write.PacketID(Message.Type.GroupDestroy);
				Network.Net.sv.write.GroupID(group.ID);
				Network.Net.sv.write.Send(new SendInfo(connection));
			}
		}
	}

	public void SpawnMapEntities()
	{
		new PrefabPreProcess(clientside: false, serverside: true);
		BaseEntity[] array = UnityEngine.Object.FindObjectsOfType<BaseEntity>();
		BaseEntity[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].SpawnAsMapEntity();
		}
		DebugEx.Log($"Map Spawned {array.Length} entities");
		array2 = array;
		foreach (BaseEntity baseEntity in array2)
		{
			if (baseEntity != null)
			{
				baseEntity.PostMapEntitySpawn();
			}
		}
	}

	public static BasePlayer.SpawnPoint FindSpawnPoint(BasePlayer forPlayer = null)
	{
		object obj = Interface.CallHook("OnFindSpawnPoint", forPlayer);
		if (obj is BasePlayer.SpawnPoint)
		{
			return (BasePlayer.SpawnPoint)obj;
		}
		bool flag = false;
		BaseGameMode baseGameMode = Gamemode();
		if ((bool)baseGameMode && baseGameMode.useCustomSpawns)
		{
			BasePlayer.SpawnPoint playerSpawn = baseGameMode.GetPlayerSpawn(forPlayer);
			if (playerSpawn != null)
			{
				return playerSpawn;
			}
		}
		if (SingletonComponent<SpawnHandler>.Instance != null && !flag)
		{
			BasePlayer.SpawnPoint spawnPoint = SpawnHandler.GetSpawnPoint();
			if (spawnPoint != null)
			{
				return spawnPoint;
			}
		}
		BasePlayer.SpawnPoint spawnPoint2 = new BasePlayer.SpawnPoint();
		GameObject[] array = GameObject.FindGameObjectsWithTag("spawnpoint");
		if (array.Length != 0)
		{
			GameObject gameObject = array[UnityEngine.Random.Range(0, array.Length)];
			spawnPoint2.pos = gameObject.transform.position;
			spawnPoint2.rot = gameObject.transform.rotation;
		}
		else
		{
			UnityEngine.Debug.Log("Couldn't find an appropriate spawnpoint for the player - so spawning at camera");
			if (MainCamera.mainCamera != null)
			{
				spawnPoint2.pos = MainCamera.position;
				spawnPoint2.rot = MainCamera.rotation;
			}
		}
		if (UnityEngine.Physics.Raycast(new Ray(spawnPoint2.pos, Vector3.down), out var hitInfo, 32f, 1537286401))
		{
			spawnPoint2.pos = hitInfo.point;
		}
		return spawnPoint2;
	}

	public void JoinGame(Network.Connection connection)
	{
		using (Approval approval = Facepunch.Pool.Get<Approval>())
		{
			uint num = (uint)ConVar.Server.encryption;
			if (num > 1 && connection.os == "editor" && DeveloperList.Contains(connection.ownerid))
			{
				num = 1u;
			}
			if (num > 1 && !ConVar.Server.secure)
			{
				num = 1u;
			}
			approval.level = UnityEngine.Application.loadedLevelName;
			approval.levelTransfer = World.Transfer;
			approval.levelUrl = World.Url;
			approval.levelSeed = World.Seed;
			approval.levelSize = World.Size;
			approval.checksum = World.Checksum;
			approval.hostname = ConVar.Server.hostname;
			approval.official = ConVar.Server.official;
			approval.encryption = num;
			if (Network.Net.sv.write.Start())
			{
				Network.Net.sv.write.PacketID(Message.Type.Approved);
				approval.WriteToStream(Network.Net.sv.write);
				Network.Net.sv.write.Send(new SendInfo(connection));
			}
			connection.encryptionLevel = num;
			connection.encryptOutgoing = true;
		}
		connection.connected = true;
	}

	internal void Shutdown()
	{
		Interface.CallHook("IOnServerShutdown");
		BasePlayer[] array = BasePlayer.activePlayerList.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Kick("Server Shutting Down");
		}
		ConsoleSystem.Run(ConsoleSystem.Option.Server, "server.save");
		ConsoleSystem.Run(ConsoleSystem.Option.Server, "server.writecfg");
	}

	private IEnumerator ServerRestartWarning(string info, int iSeconds)
	{
		if (iSeconds < 0)
		{
			yield break;
		}
		if (!string.IsNullOrEmpty(info))
		{
			ConsoleNetwork.BroadcastToAllClients("chat.add", 2, 0, "<color=#fff>SERVER</color> Restarting: " + info);
		}
		for (int i = iSeconds; i > 0; i--)
		{
			if (i == iSeconds || i % 60 == 0 || (i < 300 && i % 30 == 0) || (i < 60 && i % 10 == 0) || i < 10)
			{
				ConsoleNetwork.BroadcastToAllClients("chat.add", 2, 0, $"<color=#fff>SERVER</color> Restarting in {i} seconds ({info})!");
				UnityEngine.Debug.Log($"Restarting in {i} seconds");
			}
			yield return CoroutineEx.waitForSeconds(1f);
		}
		ConsoleNetwork.BroadcastToAllClients("chat.add", 2, 0, "<color=#fff>SERVER</color> Restarting (" + info + ")");
		yield return CoroutineEx.waitForSeconds(2f);
		BasePlayer[] array = BasePlayer.activePlayerList.ToArray();
		for (int j = 0; j < array.Length; j++)
		{
			array[j].Kick("Server Restarting");
		}
		yield return CoroutineEx.waitForSeconds(1f);
		ConsoleSystem.Run(ConsoleSystem.Option.Server, "quit");
	}

	public static void RestartServer(string strNotice, int iSeconds)
	{
		if (SingletonComponent<ServerMgr>.Instance == null)
		{
			return;
		}
		if (SingletonComponent<ServerMgr>.Instance.restartCoroutine != null)
		{
			if (Interface.CallHook("OnServerRestartInterrupt") != null)
			{
				return;
			}
			ConsoleNetwork.BroadcastToAllClients("chat.add", 2, 0, "<color=#fff>SERVER</color> Restart interrupted!");
			SingletonComponent<ServerMgr>.Instance.StopCoroutine(SingletonComponent<ServerMgr>.Instance.restartCoroutine);
			SingletonComponent<ServerMgr>.Instance.restartCoroutine = null;
		}
		if (Interface.CallHook("OnServerRestart", strNotice, iSeconds) == null)
		{
			SingletonComponent<ServerMgr>.Instance.restartCoroutine = SingletonComponent<ServerMgr>.Instance.ServerRestartWarning(strNotice, iSeconds);
			SingletonComponent<ServerMgr>.Instance.StartCoroutine(SingletonComponent<ServerMgr>.Instance.restartCoroutine);
			SingletonComponent<ServerMgr>.Instance.UpdateServerInformation();
		}
	}

	public static void SendReplicatedVars(string filter)
	{
		if (!Network.Net.sv.write.Start())
		{
			return;
		}
		List<Network.Connection> obj = Facepunch.Pool.GetList<Network.Connection>();
		foreach (Network.Connection connection in Network.Net.sv.connections)
		{
			if (connection.connected)
			{
				obj.Add(connection);
			}
		}
		List<ConsoleSystem.Command> obj2 = Facepunch.Pool.GetList<ConsoleSystem.Command>();
		foreach (ConsoleSystem.Command item in ConsoleSystem.Index.Server.Replicated)
		{
			if (item.FullName.StartsWith(filter))
			{
				obj2.Add(item);
			}
		}
		Network.Net.sv.write.PacketID(Message.Type.ConsoleReplicatedVars);
		Network.Net.sv.write.Int32(obj2.Count);
		foreach (ConsoleSystem.Command item2 in obj2)
		{
			Network.Net.sv.write.String(item2.FullName);
			Network.Net.sv.write.String(item2.String);
		}
		Network.Net.sv.write.Send(new SendInfo(obj));
		Facepunch.Pool.FreeList(ref obj2);
		Facepunch.Pool.FreeList(ref obj);
	}

	public static void SendReplicatedVars(Network.Connection connection)
	{
		if (!Network.Net.sv.write.Start())
		{
			return;
		}
		List<ConsoleSystem.Command> replicated = ConsoleSystem.Index.Server.Replicated;
		Network.Net.sv.write.PacketID(Message.Type.ConsoleReplicatedVars);
		Network.Net.sv.write.Int32(replicated.Count);
		foreach (ConsoleSystem.Command item in replicated)
		{
			Network.Net.sv.write.String(item.FullName);
			Network.Net.sv.write.String(item.String);
		}
		Network.Net.sv.write.Send(new SendInfo(connection));
	}

	private static void OnReplicatedVarChanged(string fullName, string value)
	{
		if (!Network.Net.sv.write.Start())
		{
			return;
		}
		List<Network.Connection> obj = Facepunch.Pool.GetList<Network.Connection>();
		foreach (Network.Connection connection in Network.Net.sv.connections)
		{
			if (connection.connected)
			{
				obj.Add(connection);
			}
		}
		Network.Net.sv.write.PacketID(Message.Type.ConsoleReplicatedVars);
		Network.Net.sv.write.Int32(1);
		Network.Net.sv.write.String(fullName);
		Network.Net.sv.write.String(value);
		Network.Net.sv.write.Send(new SendInfo(obj));
		Facepunch.Pool.FreeList(ref obj);
	}
}
