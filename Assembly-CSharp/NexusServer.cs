using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CompanionServer;
using ConVar;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Nexus;
using Facepunch.Nexus.Models;
using Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProtoBuf;
using ProtoBuf.Nexus;
using Rust.Nexus.Handlers;
using UnityEngine;

public static class NexusServer
{
	private struct ZonePlayerManifest
	{
		public RealTimeSince Received;

		public List<ulong> UserIds;
	}

	private struct PendingCall
	{
		public bool IsBroadcast;

		public RealTimeUntil TimeUntilTimeout;

		public TaskCompletionSource<bool> Completion;

		public NexusRpcResult Result;
	}

	private static bool _isRefreshingCompanion;

	private static RealTimeSince _lastCompanionRefresh;

	private static readonly Memoized<string, ulong> SteamIdToString = new Memoized<string, ulong>((ulong i) => i.ToString("G"));

	private static readonly MemoryStream WriterStream = new MemoryStream();

	private static readonly ByteArrayStream ReaderStream = new ByteArrayStream();

	private static NexusDB _database;

	private static readonly Dictionary<string, List<(string Zone, FerryStatus Status)>> FerryEntries = new Dictionary<string, List<(string, FerryStatus)>>(StringComparer.InvariantCultureIgnoreCase);

	private static bool _updatingFerries;

	private static int _cyclesWithoutFerry;

	private static float _zoneContactRadius;

	private static Dictionary<string, NexusIsland> _existingIslands;

	private const int MapRenderVersion = 5;

	private static readonly HashSet<ulong> PlayerManifest = new HashSet<ulong>();

	private static readonly Dictionary<string, ZonePlayerManifest> ZonePlayerManifests = new Dictionary<string, ZonePlayerManifest>(StringComparer.InvariantCultureIgnoreCase);

	private static RealTimeSince _lastPlayerManifestBroadcast;

	private static bool _playerManifestDirty;

	private static RealTimeSince _lastPlayerManifestRebuild;

	private static readonly Dictionary<Uuid, PendingCall> PendingCalls = new Dictionary<Uuid, PendingCall>();

	private static RealTimeSince _sinceLastRpcTimeoutCheck = 0f;

	private static readonly Dictionary<string, ServerStatus> ZoneStatuses = new Dictionary<string, ServerStatus>(StringComparer.InvariantCultureIgnoreCase);

	private static bool _isRefreshingZoneStatus;

	private static RealTimeSince _lastZoneStatusRefresh;

	private static DateTimeOffset? _lastUnsavedTransfer;

	private const string CopyFromKey = "$copyFrom";

	public static NexusZoneClient ZoneClient { get; private set; }

	public static bool Started { get; private set; }

	public static bool FailedToStart { get; private set; }

	public static int? NexusId => ZoneClient?.Zone?.NexusId;

	public static string ZoneKey => ZoneClient?.Zone?.Key;

	public static long? LastReset => ZoneClient?.Nexus?.LastReset;

	public static List<NexusZoneDetails> Zones => ZoneClient?.Nexus?.Zones;

	public static bool NeedsJournalFlush
	{
		get
		{
			if (Started && _database.OldestJournal.HasValue)
			{
				return (DateTimeOffset.UtcNow - _database.OldestJournal.Value).TotalSeconds >= (double)ConVar.Nexus.transferFlushTime;
			}
			return false;
		}
	}

	private static int RpcResponseTtl => ConVar.Nexus.messageLockDuration * 4;

	public static bool NeedTransferFlush
	{
		get
		{
			if (Started && _lastUnsavedTransfer.HasValue)
			{
				return (DateTimeOffset.UtcNow - _lastUnsavedTransfer.Value).TotalSeconds >= (double)ConVar.Nexus.transferFlushTime;
			}
			return false;
		}
	}

	private static void RefreshCompanionVariables()
	{
		if (!_isRefreshingCompanion && !((float)_lastCompanionRefresh < 60f))
		{
			RefreshCompanionVariablesImpl();
		}
		static async void RefreshCompanionVariablesImpl()
		{
			_ = 3;
			try
			{
				_isRefreshingCompanion = true;
				_lastCompanionRefresh = 0f;
				await ZoneClient.SetZoneVariable("protocol", Network.Net.sv.ProtocolId, isTransient: false, isSecret: false);
				if (CompanionServer.Server.IsEnabled)
				{
					string value = await App.GetPublicIPAsync();
					string appPort = App.port.ToString("G", CultureInfo.InvariantCulture);
					await ZoneClient.SetZoneVariable("appIp", value, isTransient: false, isSecret: false);
					await ZoneClient.SetZoneVariable("appPort", appPort, isTransient: false, isSecret: false);
				}
			}
			catch (Exception exception)
			{
				Debug.LogError("Failed to set up Rust companion nexus zone variables");
				Debug.LogException(exception);
			}
			finally
			{
				_isRefreshingCompanion = false;
			}
		}
	}

	public static IEnumerator Initialize()
	{
		if (Started)
		{
			Debug.LogError("NexusServer was already started");
			yield break;
		}
		ZoneClient?.Dispose();
		ZoneClient = null;
		_database?.Close();
		_database = null;
		ZoneController.Instance = null;
		Started = false;
		FailedToStart = true;
		if (string.IsNullOrWhiteSpace(ConVar.Nexus.endpoint) || !ConVar.Nexus.endpoint.StartsWith("http") || string.IsNullOrWhiteSpace(ConVar.Nexus.secretKey))
		{
			Debug.Log("Nexus endpoint and/or secret key is not set, not starting nexus connection");
			FailedToStart = false;
			yield break;
		}
		GameObject gameObject = new GameObject("NexusCleanupOnShutdown");
		gameObject.AddComponent<NexusCleanupOnShutdown>();
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		try
		{
			_database = new NexusDB();
			_database.Open($"{ConVar.Server.rootFolder}/nexus.{243}.db", fastMode: true);
			_database.Initialize();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			yield break;
		}
		ZoneClient = new NexusZoneClient(NexusServerLogger.Instance, ConVar.Nexus.endpoint, ConVar.Nexus.secretKey, ConVar.Nexus.messageLockDuration);
		ZoneClient.OnError += delegate(BaseNexusClient _, Exception ex)
		{
			Debug.LogException(ex);
		};
		Task startTask = ZoneClient.Start();
		yield return new WaitUntil(() => startTask.IsCompleted);
		if (startTask.Exception != null)
		{
			Debug.LogException(startTask.Exception);
			yield break;
		}
		if (string.IsNullOrWhiteSpace(ZoneKey))
		{
			Debug.LogError("Zone name is not available after nexus initialization");
			yield break;
		}
		Debug.Log($"Connected as zone '{ZoneKey}' in Nexus {ZoneClient.Zone.NexusName} (id={ZoneClient.Zone.NexusId})");
		ZoneController.Instance = BuildZoneController(ConVar.Nexus.zoneController);
		if (ZoneController.Instance == null)
		{
			Debug.LogError(string.IsNullOrWhiteSpace(ConVar.Nexus.zoneController) ? "Zone controller was not specified (nexus.zoneController convar)" : ("Zone controller is not supported: " + ConVar.Nexus.zoneController));
			yield break;
		}
		if (ZoneClient.TryGetNexusVariable("server.cfg", out var variable))
		{
			Debug.Log("Running server.cfg from nexus variable");
			RunConsoleConfig(variable);
		}
		if (ZoneClient.TryGetZoneVariable("server.cfg", out var variable2))
		{
			Debug.Log("Running server.cfg from zone variable");
			RunConsoleConfig(variable2);
		}
		if (string.IsNullOrWhiteSpace(ConVar.World.configString) && string.IsNullOrWhiteSpace(ConVar.World.configFile))
		{
			Debug.Log("Attempting to pull world config from the nexus");
			string worldConfigString;
			try
			{
				worldConfigString = GetWorldConfigString();
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
				yield break;
			}
			Debug.Log("Will use world config from nexus: " + worldConfigString);
			ConVar.World.configString = worldConfigString;
		}
		else
		{
			Debug.LogWarning("World config convar(s) are already set, will not pull world config from nexus");
		}
		Started = true;
		FailedToStart = false;
		static void RunConsoleConfig(Variable cfgVariable)
		{
			if ((object)cfgVariable != null && cfgVariable.Type == VariableType.String)
			{
				string asString = cfgVariable.GetAsString();
				if (!string.IsNullOrWhiteSpace(asString))
				{
					ConsoleSystem.RunFile(ConsoleSystem.Option.Server, asString);
				}
			}
		}
	}

	public static void Shutdown()
	{
		Started = false;
		FailedToStart = false;
		_existingIslands?.Clear();
		ZoneClient?.Dispose();
		ZoneClient = null;
		_database?.Close();
		_database = null;
	}

	public static void Update()
	{
		if (Started)
		{
			ReadIncomingMessages();
			CheckForRpcTimeouts();
			RefreshZoneStatus();
			UpdatePlayerManifest();
			RefreshCompanionVariables();
		}
	}

	public static NexusZoneDetails FindZone(string zoneKey)
	{
		return ZoneClient?.Nexus?.Zones?.FindWith((NexusZoneDetails z) => z.Key, zoneKey, StringComparer.InvariantCultureIgnoreCase);
	}

	public static Task<NexusLoginResult> Login(ulong steamId)
	{
		return ZoneClient.PlayerLogin(SteamIdToString.Get(steamId));
	}

	public static void Logout(ulong steamId)
	{
		ZoneClient?.PlayerLogout(SteamIdToString.Get(steamId));
	}

	public static bool TryGetPlayer(ulong steamId, out NexusPlayer player)
	{
		if (!Started)
		{
			player = null;
			return false;
		}
		return ZoneClient.TryGetPlayer(SteamIdToString.Get(steamId), out player);
	}

	public static Task AssignInitialZone(ulong steamId, string zoneKey)
	{
		return ZoneClient.Assign(steamId.ToString("G"), zoneKey);
	}

	private static ZoneController BuildZoneController(string name)
	{
		if (name.ToLowerInvariant() == "basic")
		{
			return new BasicZoneController(ZoneClient);
		}
		return null;
	}

	public static void PostGameSaved()
	{
		_database?.ClearJournal();
		_database?.ClearTransferred();
		_lastUnsavedTransfer = null;
	}

	public static async void UpdateFerries()
	{
		if (ZoneClient == null || _updatingFerries)
		{
			return;
		}
		try
		{
			_updatingFerries = true;
			await UpdateFerriesImpl();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		finally
		{
			_updatingFerries = false;
		}
	}

	private static async Task UpdateFerriesImpl()
	{
		if (ZoneClient == null)
		{
			return;
		}
		Request request = Facepunch.Pool.Get<Request>();
		request.ferryStatus = Facepunch.Pool.Get<FerryStatusRequest>();
		using (NexusRpcResult statusResponse = await BroadcastRpc(request))
		{
			UpdateFerryStatuses(statusResponse);
		}
		string zone = ZoneKey;
		List<(string, FerryStatus)> value;
		if (ZoneClient.TryGetZoneVariable("ferry", out var variable) && variable.Type == VariableType.String && TryParseFerrySchedule(zone, variable.GetAsString(), out var schedule))
		{
			if (FerryEntries.TryGetValue(zone, out var entries) && entries.Count > 1)
			{
				for (int i = 1; i < entries.Count; i++)
				{
					(string, FerryStatus) tuple = entries[i];
					await RetireFerry(tuple.Item1, tuple.Item2.entityId, tuple.Item2.timestamp);
				}
			}
			if (entries != null && entries.Count > 0)
			{
				_cyclesWithoutFerry = 0;
				(string, FerryStatus) tuple2 = entries[0];
				if (!tuple2.Item2.schedule.SequenceEqual(schedule, StringComparer.InvariantCultureIgnoreCase))
				{
					await UpdateFerrySchedule(tuple2.Item1, tuple2.Item2.entityId, tuple2.Item2.timestamp, schedule);
				}
			}
			else
			{
				if (entries != null && entries.Count != 0)
				{
					return;
				}
				_cyclesWithoutFerry++;
				if (_cyclesWithoutFerry < 5)
				{
					return;
				}
				_cyclesWithoutFerry = 0;
				BaseEntity baseEntity = GameManager.server.CreateEntity("assets/content/nexus/ferry/nexusferry.entity.prefab");
				if (!(baseEntity is NexusFerry nexusFerry))
				{
					Debug.LogError("Failed to spawn nexus ferry!");
					if (baseEntity != null)
					{
						UnityEngine.Object.Destroy(baseEntity);
					}
				}
				else
				{
					nexusFerry.Initialize(zone, schedule);
					nexusFerry.Spawn();
				}
			}
		}
		else if (FerryEntries.TryGetValue(zone, out value) && value.Count > 0)
		{
			_cyclesWithoutFerry = 0;
			foreach (var item in value)
			{
				await RetireFerry(item.Item1, item.Item2.entityId, item.Item2.timestamp);
			}
		}
		else
		{
			_cyclesWithoutFerry = 0;
		}
	}

	public static bool TryGetFerryStatus(string ownerZone, out string currentZone, out FerryStatus status)
	{
		if (!FerryEntries.TryGetValue(ownerZone, out List<(string, FerryStatus)> value) || value.Count < 1)
		{
			currentZone = null;
			status = null;
			return false;
		}
		(currentZone, status) = value[0];
		return true;
	}

	private static Task RetireFerry(string zone, NetworkableId entityId, long timestamp)
	{
		Request request = Facepunch.Pool.Get<Request>();
		request.ferryRetire = Facepunch.Pool.Get<FerryRetireRequest>();
		request.ferryRetire.entityId = entityId;
		request.ferryRetire.timestamp = timestamp;
		return ZoneRpc(zone, request);
	}

	private static Task UpdateFerrySchedule(string zone, NetworkableId entityId, long timestamp, List<string> schedule)
	{
		Request request = Facepunch.Pool.Get<Request>();
		request.ferryUpdateSchedule = Facepunch.Pool.Get<FerryUpdateScheduleRequest>();
		request.ferryUpdateSchedule.entityId = entityId;
		request.ferryUpdateSchedule.timestamp = timestamp;
		request.ferryUpdateSchedule.schedule = schedule.ShallowClonePooled();
		return ZoneRpc(zone, request);
	}

	private static bool TryParseFerrySchedule(string zone, string scheduleString, out List<string> entries)
	{
		if (!NexusUtil.TryParseFerrySchedule(zone, scheduleString, out var entries2))
		{
			entries = null;
			return false;
		}
		List<string> list = entries2.ToList();
		foreach (string item in list)
		{
			if (FindZone(item) == null)
			{
				Debug.LogError("Ferry schedule for '" + zone + "' lists an invalid zone '" + item + "': " + scheduleString);
				entries = null;
				return false;
			}
		}
		entries = list;
		return true;
	}

	private static void UpdateFerryStatuses(NexusRpcResult statusResponse)
	{
		foreach (KeyValuePair<string, List<(string, FerryStatus)>> ferryEntry in FerryEntries)
		{
			List<(string, FerryStatus)> obj = ferryEntry.Value;
			foreach (var item in obj)
			{
				item.Item2.Dispose();
			}
			Facepunch.Pool.FreeList(ref obj);
		}
		FerryEntries.Clear();
		foreach (KeyValuePair<string, Response> response in statusResponse.Responses)
		{
			FerryStatusResponse ferryStatus = response.Value.ferryStatus;
			if (ferryStatus?.statuses == null)
			{
				continue;
			}
			foreach (FerryStatus status in ferryStatus.statuses)
			{
				AddFerryStatus(response.Key, status);
			}
		}
		string zoneKey = ZoneKey;
		foreach (NexusFerry item2 in NexusFerry.All)
		{
			AddFerryStatus(zoneKey, item2.GetStatus());
		}
		foreach (List<(string, FerryStatus)> value2 in FerryEntries.Values)
		{
			if (value2.Count > 1)
			{
				value2.Sort(((string Zone, FerryStatus Status) a, (string Zone, FerryStatus Status) b) => a.Status.timestamp.CompareTo(b.Status.timestamp));
			}
		}
		static void AddFerryStatus(string currentZone, FerryStatus status)
		{
			if (!FerryEntries.TryGetValue(status.ownerZone, out List<(string, FerryStatus)> value))
			{
				value = Facepunch.Pool.GetList<(string, FerryStatus)>();
				FerryEntries.Add(status.ownerZone, value);
			}
			value.Add((currentZone, status.Copy()));
		}
	}

	public static void UpdateIslands()
	{
		if (ZoneClient == null)
		{
			return;
		}
		if (ZoneClient.TryGetNexusVariable("map.contactRadius", out var variable) && variable.Type == VariableType.String && float.TryParse(variable.GetAsString(), out var result))
		{
			_zoneContactRadius = result;
		}
		else
		{
			_zoneContactRadius = ConVar.Nexus.defaultZoneContactRadius;
		}
		if (_existingIslands == null)
		{
			_existingIslands = new Dictionary<string, NexusIsland>();
		}
		HashSet<NexusIsland> obj = Facepunch.Pool.Get<HashSet<NexusIsland>>();
		obj.Clear();
		if (_existingIslands.Count == 0)
		{
			foreach (BaseNetworkable serverEntity in BaseNetworkable.serverEntities)
			{
				if (serverEntity is NexusIsland nexusIsland)
				{
					if (string.IsNullOrEmpty(nexusIsland.ZoneKey) || _existingIslands.ContainsKey(nexusIsland.ZoneKey))
					{
						obj.Add(nexusIsland);
					}
					else
					{
						_existingIslands.Add(nexusIsland.ZoneKey, nexusIsland);
					}
				}
			}
		}
		Dictionary<string, NexusZoneDetails> obj2 = Facepunch.Pool.Get<Dictionary<string, NexusZoneDetails>>();
		obj2.Clear();
		foreach (NexusZoneDetails zone in ZoneClient.Nexus.Zones)
		{
			if (TryGetZoneStatus(zone.Key, out var status) && status.IsOnline)
			{
				obj2.Add(zone.Key, zone);
			}
		}
		foreach (KeyValuePair<string, NexusZoneDetails> item in obj2)
		{
			if (item.Key == ZoneKey)
			{
				continue;
			}
			if (!IsCloseTo(item.Value))
			{
				if (_existingIslands.TryGetValue(item.Key, out var value))
				{
					obj.Add(value);
				}
				continue;
			}
			var (vector, quaternion) = CalculateIslandTransform(item.Value);
			if (_existingIslands.TryGetValue(item.Key, out var value2) && value2 != null)
			{
				value2.transform.SetPositionAndRotation(vector, quaternion);
			}
			else
			{
				BaseEntity baseEntity = GameManager.server.CreateEntity("assets/content/nexus/island/nexusisland.prefab", vector, quaternion);
				if (!(baseEntity is NexusIsland nexusIsland2))
				{
					baseEntity.Kill();
					Debug.LogError("Failed to spawn nexus island entity!");
					continue;
				}
				nexusIsland2.ZoneKey = item.Key;
				nexusIsland2.Spawn();
				_existingIslands[item.Key] = nexusIsland2;
				value2 = nexusIsland2;
			}
			value2.SetFlag(BaseEntity.Flags.Reserved1, TryGetZoneStatus(item.Key, out var status2) && status2.IsFull);
		}
		foreach (KeyValuePair<string, NexusIsland> existingIsland in _existingIslands)
		{
			if (!obj2.ContainsKey(existingIsland.Key))
			{
				obj.Add(existingIsland.Value);
			}
		}
		foreach (NexusIsland item2 in obj)
		{
			if ((object)item2 != null)
			{
				if (item2.ZoneKey != null)
				{
					_existingIslands.Remove(item2.ZoneKey);
				}
				item2.Kill();
			}
		}
		obj.Clear();
		Facepunch.Pool.Free(ref obj);
		obj2.Clear();
		Facepunch.Pool.Free(ref obj2);
	}

	public static bool TryGetIsland(string zoneKey, out NexusIsland island)
	{
		if (_existingIslands == null)
		{
			island = null;
			return false;
		}
		if (_existingIslands.TryGetValue(zoneKey, out island))
		{
			return island != null;
		}
		return false;
	}

	public static bool TryGetIslandPosition(string zoneKey, out Vector3 position)
	{
		NexusZoneDetails nexusZoneDetails = Zones.FindWith((NexusZoneDetails z) => z.Key, zoneKey, StringComparer.InvariantCultureIgnoreCase);
		if (nexusZoneDetails == null)
		{
			position = Vector3.zero;
			return false;
		}
		(position, _) = CalculateIslandTransform(nexusZoneDetails);
		return true;
	}

	private static (Vector3, Quaternion) CalculateIslandTransform(NexusZoneDetails otherZone)
	{
		Bounds worldBounds = GetWorldBounds();
		float num = Mathf.Max(worldBounds.extents.x, worldBounds.extents.z) * 1.5f;
		float y = Vector2Ex.AngleFromTo(NexusExtensions.Position(ZoneClient.Zone), NexusExtensions.Position(otherZone));
		Vector3 point = TerrainMeta.Center + Quaternion.Euler(0f, y, 0f) * Vector3.right * num;
		Vector3 vector = worldBounds.ClosestPoint(point).WithY(TerrainMeta.Center.y);
		Quaternion item = Quaternion.LookRotation((TerrainMeta.Center - vector).normalized);
		return (vector.WithY(WaterSystem.OceanLevel), item);
	}

	public static Bounds GetWorldBounds()
	{
		Vector3 vector = ((SingletonComponent<ValidBounds>.Instance != null) ? SingletonComponent<ValidBounds>.Instance.worldBounds.extents : (Vector3.one * float.MaxValue));
		vector.x = Mathf.Min(vector.x, (float)World.Size * 1.5f);
		vector.y = 0.01f;
		vector.z = Mathf.Min(vector.z, (float)World.Size * 1.5f);
		vector.x = Mathf.Min((float)World.Size * ConVar.Nexus.islandSpawnDistance, vector.x * 0.9f);
		vector.z = Mathf.Min((float)World.Size * ConVar.Nexus.islandSpawnDistance, vector.z * 0.9f);
		return new Bounds(Vector3.zero, vector * 2f);
	}

	private static bool IsCloseTo(NexusZoneDetails otherZone)
	{
		return Vector2.Distance(NexusExtensions.Position(ZoneClient.Zone), NexusExtensions.Position(otherZone)) <= _zoneContactRadius;
	}

	private static void ReadIncomingMessages()
	{
		NexusMessage message;
		while (ZoneClient.TryReceiveMessage(out message))
		{
			if (!message.IsBinary)
			{
				Debug.LogWarning("Received a nexus message that's not binary, ignoring");
				ZoneClient.AcknowledgeMessage(in message);
				continue;
			}
			byte[] asBinary;
			Packet packet;
			try
			{
				asBinary = message.AsBinary;
				packet = ReadPacket(asBinary);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				ZoneClient.AcknowledgeMessage(in message);
				continue;
			}
			bool num = !RequiresJournaling(packet) || _database.SeenJournaled(message.Id, asBinary);
			ZoneClient.AcknowledgeMessage(in message);
			if (!num)
			{
				Debug.LogWarning("Already saw this nexus message, ignoring");
				packet.Dispose();
			}
			else
			{
				HandleMessage(message.Id, packet);
			}
		}
	}

	public static void RestoreUnsavedState()
	{
		if (Started)
		{
			ReplayJournaledMessages();
			DeleteTransferredEntities();
			ConsoleSystem.Run(ConsoleSystem.Option.Server, "server.save");
		}
	}

	private static void ReplayJournaledMessages()
	{
		List<(Guid, long, byte[])> list = _database.ReadJournal();
		if (list.Count == 0)
		{
			Debug.Log("No messages found in the nexus message journal");
			return;
		}
		Debug.Log($"Replaying {list.Count} nexus messages from the journal");
		foreach (var (guid, seconds, data) in list)
		{
			try
			{
				Debug.Log($"Replaying message ID {guid}, received {DateTimeOffset.FromUnixTimeSeconds(seconds):R}");
				Packet packet = ReadPacket(data);
				HandleMessage(guid, packet);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		Debug.Log($"Finished replaying {list.Count} nexus messages from the journal");
	}

	private static void DeleteTransferredEntities()
	{
		List<NetworkableId> list = _database.ReadTransferred();
		if (list.Count == 0)
		{
			Debug.Log("No entities found in the transferred list");
			return;
		}
		foreach (NetworkableId item in list)
		{
			try
			{
				BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(item);
				if (!(baseNetworkable == null))
				{
					Debug.Log($"Found {baseNetworkable}, killing it because it was transferred away");
					baseNetworkable.Kill();
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		Debug.Log($"Finished making sure {list.Count} entities do not exist");
	}

	private static bool RequiresJournaling(Packet packet)
	{
		if (packet.request == null || !packet.request.isFireAndForget)
		{
			return false;
		}
		return packet.request.transfer != null;
	}

	public static async void UploadMapImage(bool force = false)
	{
		_ = 1;
		try
		{
			int valueOrDefault = (World.Config?.JsonString?.GetHashCode()).GetValueOrDefault();
			string key = $"{2511}##{243}##{World.Name}##{World.Size}##{World.Seed}##{World.Salt}##{ConVar.Nexus.mapImageScale}##{valueOrDefault}##{5}";
			if (!force && (await ZoneClient.CheckUploadedMap()).Key == key)
			{
				Debug.Log("Nexus already has this map's image uploaded, will not render and upload again");
				return;
			}
			Debug.Log("Rendering map image to upload to nexus...");
			int oceanMargin = 0;
			int imageWidth;
			int imageHeight;
			Color background;
			byte[] pngMapImage = MapImageRenderer.Render(out imageWidth, out imageHeight, out background, ConVar.Nexus.mapImageScale, lossy: false, transparent: true, oceanMargin);
			Debug.Log("Uploading map image to nexus...");
			await ZoneClient.UploadMap(key, pngMapImage);
			Debug.Log("Map image was updated in the nexus");
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private static void HandleMessage(Uuid id, Packet packet)
	{
		try
		{
			if (packet.protocol != 243)
			{
				Debug.LogWarning("Received a nexus message with wrong protocol, ignoring");
				return;
			}
			NexusZoneDetails nexusZoneDetails = ZoneClient.Nexus.Zones.FindWith((NexusZoneDetails z) => z.Id, packet.sourceZone);
			if (nexusZoneDetails == null)
			{
				Debug.LogWarning($"Received a nexus message from unknown zone ID {packet.sourceZone}, ignoring");
			}
			else if (packet.request != null)
			{
				HandleRpcInvocation(nexusZoneDetails, id, packet.request);
			}
			else if (packet.response != null)
			{
				HandleRpcResponse(nexusZoneDetails, id, packet.response);
			}
			else
			{
				Debug.LogWarning("Received a nexus message without the request or request sections set, ignoring");
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		finally
		{
			packet?.Dispose();
		}
	}

	private static Packet ReadPacket(byte[] data)
	{
		ReaderStream.SetData(data, 0, data.Length);
		return Packet.Deserialize(ReaderStream);
	}

	private static Task SendRequestImpl(Uuid id, Request request, string toZoneKey, int? ttl = null)
	{
		Packet packet = Facepunch.Pool.Get<Packet>();
		packet.protocol = 243u;
		packet.sourceZone = ZoneClient.Zone.ZoneId;
		packet.request = request;
		return SendPacket(id, packet, toZoneKey, ttl);
	}

	private static async void SendResponseImpl(Response response, string toZoneKey, int? ttl = null)
	{
		try
		{
			Packet packet = Facepunch.Pool.Get<Packet>();
			packet.protocol = 243u;
			packet.sourceZone = ZoneClient.Zone.ZoneId;
			packet.response = response;
			await SendPacket(Uuid.Generate(), packet, toZoneKey, ttl);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private static Task SendPacket(Uuid id, Packet packet, string toZoneKey, int? ttl = null)
	{
		WriterStream.SetLength(0L);
		WriterStream.Position = 0L;
		packet.WriteToStream(WriterStream);
		Memory<byte> message = new Memory<byte>(WriterStream.GetBuffer(), 0, (int)WriterStream.Length);
		packet.Dispose();
		return ZoneClient.SendMessage(toZoneKey, id, message, ttl);
	}

	public static bool IsOnline(ulong userId)
	{
		RebuildPlayerManifestIfDirty();
		if (!PlayerManifest.Contains(userId))
		{
			return ServerPlayers.IsOnline(userId);
		}
		return true;
	}

	public static void AddZonePlayerManifest(string zoneKey, List<ulong> userIds)
	{
		if (ZonePlayerManifests.TryGetValue(zoneKey, out var value))
		{
			if (value.UserIds != null)
			{
				Facepunch.Pool.FreeList(ref value.UserIds);
			}
			ZonePlayerManifests.Remove(zoneKey);
		}
		ZonePlayerManifests.Add(zoneKey, new ZonePlayerManifest
		{
			Received = 0f,
			UserIds = userIds.ShallowClonePooled()
		});
	}

	private static void UpdatePlayerManifest()
	{
		if ((float)_lastPlayerManifestBroadcast >= ConVar.Nexus.playerManifestInterval)
		{
			_lastPlayerManifestBroadcast = 0f;
			BroadcastPlayerManifest();
		}
		if ((float)_lastPlayerManifestRebuild > ConVar.Nexus.playerManifestInterval)
		{
			_playerManifestDirty = true;
		}
		RebuildPlayerManifestIfDirty();
	}

	private static async void BroadcastPlayerManifest()
	{
		try
		{
			Request request = Facepunch.Pool.Get<Request>();
			request.isFireAndForget = true;
			request.playerManifest = Facepunch.Pool.Get<PlayerManifestRequest>();
			request.playerManifest.userIds = Facepunch.Pool.GetList<ulong>();
			ServerPlayers.GetAll(request.playerManifest.userIds);
			await BroadcastRpc(request);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private static void RebuildPlayerManifestIfDirty()
	{
		if (!_playerManifestDirty)
		{
			return;
		}
		_playerManifestDirty = false;
		_lastPlayerManifestRebuild = 0f;
		RemoveInvalidPlayerManifests();
		PlayerManifest.Clear();
		foreach (ZonePlayerManifest value in ZonePlayerManifests.Values)
		{
			foreach (ulong userId in value.UserIds)
			{
				PlayerManifest.Add(userId);
			}
		}
	}

	private static void RemoveInvalidPlayerManifests()
	{
		List<string> obj = Facepunch.Pool.GetList<string>();
		foreach (KeyValuePair<string, ZonePlayerManifest> zonePlayerManifest in ZonePlayerManifests)
		{
			if (FindZone(zonePlayerManifest.Key) == null || (float)zonePlayerManifest.Value.Received > ConVar.Nexus.playerManifestInterval * 3f)
			{
				obj.Add(zonePlayerManifest.Key);
			}
		}
		foreach (string item in obj)
		{
			if (ZonePlayerManifests.TryGetValue(item, out var value))
			{
				ZonePlayerManifests.Remove(item);
				if (value.UserIds != null)
				{
					Facepunch.Pool.FreeList(ref value.UserIds);
				}
			}
		}
		Facepunch.Pool.FreeList(ref obj);
	}

	public static async Task<Response> ZoneRpc(string zone, Request request, float timeoutAfter = 30f)
	{
		if (string.IsNullOrEmpty(zone))
		{
			throw new ArgumentNullException("zone");
		}
		if (string.Equals(zone, ZoneKey, StringComparison.InvariantCultureIgnoreCase))
		{
			return HandleRpcInvocationImpl(Zones.FindWith((NexusZoneDetails z) => z.Key, ZoneKey), Uuid.Empty, request);
		}
		using NexusRpcResult nexusRpcResult = await CallRpcImpl(zone, request, timeoutAfter, throwOnTimeout: true);
		Response response = nexusRpcResult.Responses[zone];
		if (!string.IsNullOrWhiteSpace(response.status?.errorMessage))
		{
			throw new Exception(response.status.errorMessage);
		}
		return response.Copy();
	}

	public static Task<NexusRpcResult> BroadcastRpc(Request request, float timeoutAfter = 30f)
	{
		return CallRpcImpl(null, request, timeoutAfter, throwOnTimeout: false);
	}

	private static async Task<NexusRpcResult> CallRpcImpl(string zone, Request request, float timeoutAfter, bool throwOnTimeout)
	{
		Uuid id = Uuid.Generate();
		TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
		NexusRpcResult result = Facepunch.Pool.Get<NexusRpcResult>();
		try
		{
			float actualTimeout = timeoutAfter * ConVar.Nexus.rpcTimeoutMultiplier;
			await SendRequestImpl(id, request, zone, (int)actualTimeout + RpcResponseTtl);
			PendingCalls.Add(id, new PendingCall
			{
				IsBroadcast = string.IsNullOrWhiteSpace(zone),
				TimeUntilTimeout = actualTimeout,
				Completion = tcs,
				Result = result
			});
			bool flag = await tcs.Task;
			if (throwOnTimeout && !flag)
			{
				throw new TimeoutException("Nexus RPC invocation timed out");
			}
		}
		catch
		{
			Facepunch.Pool.Free(ref result);
			throw;
		}
		return result;
	}

	private static void HandleRpcInvocation(NexusZoneDetails from, Uuid id, Request request)
	{
		Response response = HandleRpcInvocationImpl(from, id, request);
		if (response != null)
		{
			SendResponseImpl(response, from.Key, RpcResponseTtl);
		}
	}

	private static Response HandleRpcInvocationImpl(NexusZoneDetails from, Uuid id, Request request)
	{
		if (Handle<TransferRequest, TransferHandler>((Request r) => r.transfer, out var requestHandler2) || Handle<PingRequest, PingHandler>((Request r) => r.ping, out requestHandler2) || Handle<SpawnOptionsRequest, SpawnOptionsHandler>((Request r) => r.spawnOptions, out requestHandler2) || Handle<SleepingBagRespawnRequest, RespawnAtBagHandler>((Request r) => r.respawnAtBag, out requestHandler2) || Handle<SleepingBagDestroyRequest, DestroyBagHandler>((Request r) => r.destroyBag, out requestHandler2) || Handle<FerryStatusRequest, FerryStatusHandler>((Request r) => r.ferryStatus, out requestHandler2) || Handle<FerryRetireRequest, FerryRetireHandler>((Request r) => r.ferryRetire, out requestHandler2) || Handle<FerryUpdateScheduleRequest, FerryUpdateScheduleHandler>((Request r) => r.ferryUpdateSchedule, out requestHandler2) || Handle<ClanChatBatchRequest, ClanChatBatchHandler>((Request r) => r.clanChatBatch, out requestHandler2) || Handle<PlayerManifestRequest, PlayerManifestHandler>((Request r) => r.playerManifest, out requestHandler2))
		{
			requestHandler2.Execute();
			Response response = requestHandler2.Response;
			Facepunch.Pool.FreeDynamic(ref requestHandler2);
			return response;
		}
		Debug.LogError("Received a nexus RPC invocation with a missing or unsupported request, ignoring");
		return null;
		bool Handle<TProto, THandler>(Func<Request, TProto> protoSelector, out INexusRequestHandler requestHandler) where TProto : class where THandler : BaseNexusRequestHandler<TProto>, new()
		{
			TProto val = protoSelector(request);
			if (val == null)
			{
				requestHandler = null;
				return false;
			}
			THandler val2 = Facepunch.Pool.Get<THandler>();
			val2.Initialize(from, id, request.isFireAndForget, val);
			requestHandler = val2;
			return true;
		}
	}

	private static void HandleRpcResponse(NexusZoneDetails from, Uuid id, Response response)
	{
		if (!PendingCalls.TryGetValue(response.id, out var value))
		{
			Debug.LogWarning("Received an unexpected nexus RPC response (likely timed out), ignoring");
			return;
		}
		if (!value.Result.Responses.ContainsKey(from.Key))
		{
			value.Result.Responses.Add(from.Key, response.Copy());
		}
		int num = ((!value.IsBroadcast) ? 1 : ((ZoneClient?.Nexus?.Zones?.Count).GetValueOrDefault() - 1));
		if (value.Result.Responses.Count >= num)
		{
			PendingCalls.Remove(id);
			value.Completion.TrySetResult(result: true);
		}
	}

	private static void CheckForRpcTimeouts()
	{
		if ((float)_sinceLastRpcTimeoutCheck < 1f)
		{
			return;
		}
		_sinceLastRpcTimeoutCheck = 0f;
		List<(Uuid, PendingCall)> obj = Facepunch.Pool.GetList<(Uuid, PendingCall)>();
		foreach (KeyValuePair<Uuid, PendingCall> pendingCall in PendingCalls)
		{
			Uuid key = pendingCall.Key;
			PendingCall value = pendingCall.Value;
			if ((float)value.TimeUntilTimeout <= 0f)
			{
				obj.Add((key, value));
			}
		}
		foreach (var item3 in obj)
		{
			Uuid item = item3.Item1;
			PendingCall item2 = item3.Item2;
			PendingCalls.Remove(item);
			item2.Completion.TrySetResult(result: false);
		}
		Facepunch.Pool.FreeList(ref obj);
	}

	private static void RefreshZoneStatus()
	{
		if (!_isRefreshingZoneStatus && !((float)_lastZoneStatusRefresh < ConVar.Nexus.pingInterval))
		{
			RefreshZoneStatusImpl();
		}
		static async void RefreshZoneStatusImpl()
		{
			try
			{
				_isRefreshingZoneStatus = true;
				_lastZoneStatusRefresh = 0f;
				Request request = Facepunch.Pool.Get<Request>();
				request.ping = Facepunch.Pool.Get<PingRequest>();
				using (NexusRpcResult nexusRpcResult = await BroadcastRpc(request))
				{
					List<string> obj = Facepunch.Pool.GetList<string>();
					foreach (string key in ZoneStatuses.Keys)
					{
						if (Zones.FindWith((NexusZoneDetails z) => z.Key, key) == null)
						{
							obj.Add(key);
						}
					}
					foreach (string item in obj)
					{
						ZoneStatuses.Remove(item);
					}
					Facepunch.Pool.FreeList(ref obj);
					foreach (KeyValuePair<string, Response> response in nexusRpcResult.Responses)
					{
						if (string.IsNullOrWhiteSpace(response.Key))
						{
							Debug.LogWarning("Received a ping response for a zone with a null key");
						}
						else if (response.Value?.ping == null)
						{
							Debug.LogWarning("Received a ping response from '" + response.Key + "' but the data was null");
						}
						else
						{
							ZoneStatuses[response.Key] = new ServerStatus
							{
								IsOnline = true,
								LastSeen = 0f,
								Players = response.Value.ping.players,
								MaxPlayers = response.Value.ping.maxPlayers,
								QueuedPlayers = response.Value.ping.queuedPlayers
							};
						}
					}
					foreach (NexusZoneDetails zone in Zones)
					{
						if (!nexusRpcResult.Responses.ContainsKey(zone.Key))
						{
							if (ZoneStatuses.TryGetValue(zone.Key, out var value))
							{
								ZoneStatuses[zone.Key] = new ServerStatus
								{
									IsOnline = false,
									LastSeen = value.LastSeen,
									Players = value.Players,
									MaxPlayers = value.MaxPlayers,
									QueuedPlayers = value.QueuedPlayers
								};
							}
							else
							{
								ZoneStatuses[zone.Key] = new ServerStatus
								{
									IsOnline = false
								};
							}
						}
					}
				}
				_lastZoneStatusRefresh = 0f;
			}
			finally
			{
				_isRefreshingZoneStatus = false;
			}
			OnZoneStatusesRefreshed();
		}
	}

	public static bool TryGetZoneStatus(string zone, out ServerStatus status)
	{
		if (!Started)
		{
			status = default(ServerStatus);
			return false;
		}
		if (string.Equals(zone, ZoneKey, StringComparison.InvariantCultureIgnoreCase))
		{
			status = new ServerStatus
			{
				IsOnline = true,
				LastSeen = 0f,
				Players = BasePlayer.activePlayerList.Count,
				MaxPlayers = ConVar.Server.maxplayers,
				QueuedPlayers = SingletonComponent<ServerMgr>.Instance.connectionQueue.Queued
			};
			return true;
		}
		return ZoneStatuses.TryGetValue(zone, out status);
	}

	private static void OnZoneStatusesRefreshed()
	{
		UpdateIslands();
		UpdateFerries();
	}

	public static async Task TransferEntity(BaseEntity entity, string toZoneKey, string method)
	{
		try
		{
			await TransferEntityImpl(FindRootEntity(entity), toZoneKey, method, ZoneKey, toZoneKey);
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
		}
	}

	public static async Task TransferEntityImpl(BaseEntity rootEntity, string toZoneKey, string method, string from, string to)
	{
		if (rootEntity == null)
		{
			throw new ArgumentNullException("rootEntity");
		}
		if (string.IsNullOrWhiteSpace(toZoneKey))
		{
			throw new ArgumentNullException("toZoneKey");
		}
		if (string.Equals(toZoneKey, ZoneKey, StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ArgumentException("Attempted to transfer a player to the current server's zone", "toZoneKey");
		}
		NexusZoneDetails toZone = ZoneClient.Nexus.Zones.FindWith((NexusZoneDetails z) => z.Key, toZoneKey, StringComparer.InvariantCultureIgnoreCase);
		if (toZone == null)
		{
			throw new ArgumentException("Target zone (" + toZoneKey + ") was not found in the nexus", "toZoneKey");
		}
		BuildTransferRequest(rootEntity, method, from, to, out var request, out var networkables, out var players, out var playerIds);
		HashSet<NetworkableId> transferEntityIds = Facepunch.Pool.Get<HashSet<NetworkableId>>();
		transferEntityIds.Clear();
		foreach (BaseNetworkable item in networkables)
		{
			if (item.net != null && item.net.ID.IsValid)
			{
				transferEntityIds.Add(item.net.ID);
			}
		}
		foreach (BaseNetworkable item2 in networkables)
		{
			if (item2.net != null && item2.net.ID.IsValid)
			{
				transferEntityIds.Add(item2.net.ID);
			}
			if (item2 is BaseEntity baseEntity)
			{
				baseEntity.SetFlag(BaseEntity.Flags.Transferring, b: true);
			}
		}
		try
		{
			if (playerIds.Count > 0)
			{
				await ZoneClient.RegisterTransfers(toZoneKey, playerIds);
			}
			await SendRequestImpl(Uuid.Generate(), request, toZoneKey);
		}
		catch
		{
			foreach (BaseNetworkable item3 in networkables)
			{
				if (item3 != null && item3 is BaseEntity baseEntity2)
				{
					baseEntity2.SetFlag(BaseEntity.Flags.Transferring, b: false);
				}
			}
			throw;
		}
		foreach (BasePlayer item4 in players)
		{
			if (item4 != null && item4.IsConnected)
			{
				ConsoleNetwork.SendClientCommandImmediate(item4.net.connection, "nexus.redirect", toZone.IpAddress, toZone.GamePort, NexusUtil.ConnectionProtocol(toZone));
				item4.Kick("Redirecting to another zone...");
			}
		}
		for (int num = networkables.Count - 1; num >= 0; num--)
		{
			try
			{
				BaseNetworkable baseNetworkable = networkables[num];
				if (baseNetworkable != null)
				{
					if (baseNetworkable is BaseEntity entity)
					{
						UnparentUnknown(entity, transferEntityIds);
					}
					baseNetworkable.Kill();
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		_database.MarkTransferred(transferEntityIds);
		transferEntityIds.Clear();
		Facepunch.Pool.Free(ref transferEntityIds);
		Facepunch.Pool.FreeList(ref networkables);
		Facepunch.Pool.FreeList(ref players);
		Facepunch.Pool.FreeList(ref playerIds);
		_lastUnsavedTransfer = DateTimeOffset.UtcNow;
	}

	private static void UnparentUnknown(BaseEntity entity, HashSet<NetworkableId> knownEntityIds)
	{
		List<BaseEntity> obj = Facepunch.Pool.GetList<BaseEntity>();
		foreach (BaseEntity child in entity.children)
		{
			if (knownEntityIds.Contains(child.net.ID))
			{
				UnparentUnknown(child, knownEntityIds);
			}
			else
			{
				obj.Add(child);
			}
		}
		foreach (BaseEntity item in obj)
		{
			Debug.Log($"Unparenting {entity}", entity);
			item.SetParent(null, worldPositionStays: true, sendImmediate: true);
		}
		Facepunch.Pool.FreeList(ref obj);
	}

	public static void BuildTransferRequest(BaseEntity rootEntity, string method, string from, string to, out Request request, out List<BaseNetworkable> networkables, out List<BasePlayer> players, out List<string> playerIds)
	{
		List<BaseNetworkable> entitiesList = (networkables = Facepunch.Pool.GetList<BaseNetworkable>());
		List<BasePlayer> playerList = (players = Facepunch.Pool.GetList<BasePlayer>());
		List<string> playerIdsList = (playerIds = Facepunch.Pool.GetList<string>());
		request = Facepunch.Pool.Get<Request>();
		request.isFireAndForget = true;
		request.transfer = Facepunch.Pool.Get<TransferRequest>();
		request.transfer.method = method;
		request.transfer.from = from;
		request.transfer.to = to;
		List<ProtoBuf.Entity> serializedEntities = (request.transfer.entities = Facepunch.Pool.GetList<ProtoBuf.Entity>());
		List<PlayerSecondaryData> secondaryData = (request.transfer.secondaryData = Facepunch.Pool.GetList<PlayerSecondaryData>());
		Queue<BaseNetworkable> pendingEntities = Facepunch.Pool.Get<Queue<BaseNetworkable>>();
		pendingEntities.Clear();
		HashSet<NetworkableId> seenEntityIds = Facepunch.Pool.Get<HashSet<NetworkableId>>();
		seenEntityIds.Clear();
		pendingEntities.Enqueue(rootEntity);
		seenEntityIds.Add(rootEntity.net.ID);
		while (pendingEntities.Count > 0)
		{
			BaseNetworkable baseNetworkable = pendingEntities.Dequeue();
			ProtoBuf.Entity entity2 = null;
			if (CanTransferEntity(baseNetworkable))
			{
				entity2 = AddEntity(baseNetworkable);
			}
			foreach (BaseEntity child in baseNetworkable.children)
			{
				if (child != null && seenEntityIds.Add(child.net.ID))
				{
					pendingEntities.Enqueue(child);
				}
			}
			if (baseNetworkable is BaseMountable baseMountable)
			{
				BasePlayer mounted = baseMountable.GetMounted();
				if (mounted != null && seenEntityIds.Add(mounted.net.ID))
				{
					pendingEntities.Enqueue(mounted);
				}
			}
			entity2?.InspectUids(ScanForAdditionalEntities);
		}
		seenEntityIds.Clear();
		Facepunch.Pool.Free(ref seenEntityIds);
		pendingEntities.Clear();
		Facepunch.Pool.Free(ref pendingEntities);
		ProtoBuf.Entity AddEntity(BaseNetworkable entity)
		{
			BaseNetworkable.SaveInfo saveInfo = default(BaseNetworkable.SaveInfo);
			saveInfo.forDisk = true;
			saveInfo.forTransfer = true;
			saveInfo.msg = Facepunch.Pool.Get<ProtoBuf.Entity>();
			BaseNetworkable.SaveInfo info = saveInfo;
			entity.Save(info);
			serializedEntities.Add(info.msg);
			entitiesList.Add(entity);
			if (entity is BasePlayer basePlayer && basePlayer.GetType() == typeof(BasePlayer) && basePlayer.userID > uint.MaxValue)
			{
				playerList.Add(basePlayer);
				playerIdsList.Add(basePlayer.UserIDString);
				secondaryData.Add(basePlayer.SaveSecondaryData());
			}
			return info.msg;
		}
		void ScanForAdditionalEntities(UidType type, ref ulong uid)
		{
			NetworkableId networkableId = new NetworkableId(uid);
			if (type == UidType.NetworkableId && networkableId.IsValid && seenEntityIds.Add(networkableId))
			{
				BaseNetworkable baseNetworkable2 = BaseNetworkable.serverEntities.Find(networkableId);
				if (baseNetworkable2 != null)
				{
					pendingEntities.Enqueue(baseNetworkable2);
				}
			}
		}
	}

	private static bool CanTransferEntity(BaseNetworkable networkable)
	{
		if (networkable == null)
		{
			return false;
		}
		if (networkable is BaseEntity { enableSaving: false })
		{
			return false;
		}
		return true;
	}

	public static BaseEntity FindRootEntity(BaseEntity startEntity)
	{
		BaseEntity baseEntity = startEntity;
		BaseEntity parent2;
		while (TryGetParent(baseEntity, out parent2))
		{
			baseEntity = parent2;
		}
		return baseEntity;
		static bool TryGetParent(BaseEntity entity, out BaseEntity parent)
		{
			BaseEntity parentEntity = entity.GetParentEntity();
			if (parentEntity != null && !(parentEntity is NexusFerry))
			{
				parent = parentEntity;
				return true;
			}
			if (entity is BasePlayer basePlayer)
			{
				BaseMountable mounted = basePlayer.GetMounted();
				if (mounted != null)
				{
					parent = mounted;
					return true;
				}
			}
			parent = null;
			return false;
		}
	}

	private static string GetWorldConfigString()
	{
		List<string> obj = Facepunch.Pool.GetList<string>();
		JObject worldConfigImpl = GetWorldConfigImpl(ZoneKey, obj);
		Facepunch.Pool.FreeList(ref obj);
		return worldConfigImpl?.ToString(Formatting.None);
	}

	private static JObject GetWorldConfigImpl(string zoneKey, List<string> stack)
	{
		if (stack.Count > 20)
		{
			throw new Exception("Cannot load world config from nexus - there is a cyclic dependency between zones (" + string.Join(" -> ", stack) + ")");
		}
		bool required = stack.Count > 0;
		if (!TryGetWorldConfigObject(zoneKey, required, out var cfg, out var error))
		{
			throw new Exception(error + " (" + string.Join(" -> ", stack) + ")");
		}
		if (!cfg.TryGetValue("$copyFrom", out var value))
		{
			return cfg;
		}
		if (value.Type != JTokenType.String)
		{
			throw new Exception("Cannot get world config from nexus - zone '" + zoneKey + "' has a $copyFrom, but its value is not a string");
		}
		stack.Add(zoneKey);
		JObject jObject = MergeInto(GetWorldConfigImpl(value.ToObject<string>(), stack), cfg);
		jObject.Remove("$copyFrom");
		return jObject;
	}

	private static bool TryGetWorldConfigObject(string zoneKey, bool required, out JObject cfg, out string error)
	{
		cfg = null;
		if (ZoneClient?.Nexus?.Zones == null)
		{
			error = "Cannot get world config from nexus - nexus server isn't started";
			return false;
		}
		NexusZoneDetails nexusZoneDetails = FindZone(zoneKey);
		if (nexusZoneDetails == null)
		{
			error = "Cannot get world config for nexus zone '" + zoneKey + "' - zone was not found";
			return false;
		}
		if (!nexusZoneDetails.Variables.TryGetValue("world.cfg", out var value))
		{
			if (required)
			{
				error = "Cannot get world config for nexus zone '" + zoneKey + "' - world.cfg variable not found but is required by another zone";
				return false;
			}
			cfg = new JObject();
			error = null;
			return true;
		}
		if (value.Type != VariableType.String || string.IsNullOrWhiteSpace(value.Value))
		{
			error = "Cannot get world config for nexus zone '" + zoneKey + "' - world.cfg variable is empty or not a string";
			return false;
		}
		try
		{
			cfg = JObject.Parse(value.Value);
			error = null;
			return true;
		}
		catch (Exception ex)
		{
			error = "Cannot get world config for nexus zone '" + zoneKey + "' - failed to parse: `" + value.Value + "` (" + ex.Message + ")";
			return false;
		}
	}

	private static JObject MergeInto(JObject baseObject, JObject sourceObject)
	{
		JObject jObject = new JObject(baseObject);
		foreach (KeyValuePair<string, JToken> item in sourceObject)
		{
			jObject[item.Key] = item.Value;
		}
		return jObject;
	}
}
