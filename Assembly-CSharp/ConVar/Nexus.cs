using System;
using Facepunch;
using ProtoBuf.Nexus;
using UnityEngine;

namespace ConVar;

[Factory("nexus")]
public class Nexus : ConsoleSystem
{
	public static readonly Translate.Phrase RedirectPhrase = new Translate.Phrase("loading.redirect", "Switching servers");

	private const string DefaultEndpoint = "https://api.facepunch.com/api/nexus/";

	[ReplicatedVar(Help = "URL endpoint to use for the Nexus API", Default = "https://api.facepunch.com/api/nexus/")]
	public static string endpoint = "https://api.facepunch.com/api/nexus/";

	[ServerVar(Clientside = true)]
	public static bool logging = true;

	[ServerVar]
	public static string secretKey = "";

	[ServerVar]
	public static string zoneController = "basic";

	[ServerVar(Help = "Time in seconds to allow the server to process nexus messages before re-sending (requires restart)")]
	public static int messageLockDuration = 5;

	[ServerVar(Help = "Maximum amount of time in seconds that transfers should be cached before auto-saving")]
	public static int transferFlushTime = 60;

	[ServerVar(Help = "How far away islands should be spawned, as a factor of the map size")]
	public static float islandSpawnDistance = 1.5f;

	[ServerVar(Help = "Default distance between zones to allow boat travel, if map.contactRadius isn't set in the nexus (uses normalized coordinates)")]
	public static float defaultZoneContactRadius = 0.33f;

	[ServerVar(Help = "Time offset in hours from the nexus clock")]
	public static float timeOffset = 0f;

	[ServerVar(Help = "Multiplier for nexus RPC timeout durations in case we expect different latencies")]
	public static float rpcTimeoutMultiplier = 1f;

	[ServerVar(Help = "Time in seconds to keep players in the loading state before going to sleep")]
	public static float loadingTimeout = 900f;

	[ServerVar(Help = "Time in seconds to wait between server status pings")]
	public static float pingInterval = 30f;

	[ServerVar(Help = "Maximum time in seconds to keep transfer protection enabled on entities")]
	public static float protectionDuration = 300f;

	[ServerVar(Help = "Maximum duration in seconds to batch clan chat messages to send to other servers on the nexus")]
	public static float clanClatBatchDuration = 1f;

	[ServerVar(Help = "Interval in seconds to broadcast the player manifest to other servers on the nexus")]
	public static float playerManifestInterval = 30f;

	[ServerVar(Help = "Scale of the map to render and upload to the nexus")]
	public static float mapImageScale = 0.5f;

	[ServerVar]
	public static void transfer(Arg arg)
	{
		if (!NexusServer.Started)
		{
			arg.ReplyWith("Server is not connected to a nexus");
			return;
		}
		string text = arg.GetString(0)?.Trim();
		if (string.IsNullOrWhiteSpace(text))
		{
			arg.ReplyWith("Usage: nexus.transfer <target_zone>");
			return;
		}
		if (string.Equals(text, NexusServer.ZoneKey, StringComparison.InvariantCultureIgnoreCase))
		{
			arg.ReplyWith("You're already on the target zone");
			return;
		}
		BasePlayer basePlayer = arg.Connection.player as BasePlayer;
		if (basePlayer == null)
		{
			arg.ReplyWith("Must be run as a player");
		}
		else
		{
			NexusServer.TransferEntity(basePlayer, text, "console");
		}
	}

	[ServerVar]
	public static void refreshislands(Arg arg)
	{
		if (!NexusServer.Started)
		{
			arg.ReplyWith("Server is not connected to a nexus");
		}
		else
		{
			NexusServer.UpdateIslands();
		}
	}

	[ServerVar]
	public static void ping(Arg arg)
	{
		if (!NexusServer.Started)
		{
			arg.ReplyWith("Server is not connected to a nexus");
			return;
		}
		string @string = arg.GetString(0);
		if (string.IsNullOrWhiteSpace(@string))
		{
			arg.ReplyWith("Usage: nexus.ping <target_zone>");
		}
		else
		{
			SendPing(ArgEx.Player(arg), @string);
		}
		static async void SendPing(BasePlayer requester, string to)
		{
			Request request = Facepunch.Pool.Get<Request>();
			request.ping = Facepunch.Pool.Get<PingRequest>();
			float startTime = UnityEngine.Time.realtimeSinceStartup;
			try
			{
				await NexusServer.ZoneRpc(to, request);
				float num = UnityEngine.Time.realtimeSinceStartup - startTime;
				requester?.ConsoleMessage($"Ping took {num:F3}s");
			}
			catch (Exception arg2)
			{
				requester?.ConsoleMessage($"Failed to ping zone {to}: {arg2}");
			}
		}
	}

	[ServerVar]
	public static void broadcast_ping(Arg arg)
	{
		if (!NexusServer.Started)
		{
			arg.ReplyWith("Server is not connected to a nexus");
		}
		else
		{
			SendBroadcastPing(ArgEx.Player(arg));
		}
		static async void SendBroadcastPing(BasePlayer requester)
		{
			Request request = Facepunch.Pool.Get<Request>();
			request.ping = Facepunch.Pool.Get<PingRequest>();
			float startTime = UnityEngine.Time.realtimeSinceStartup;
			try
			{
				using NexusRpcResult nexusRpcResult = await NexusServer.BroadcastRpc(request);
				float num = UnityEngine.Time.realtimeSinceStartup - startTime;
				string arg2 = string.Join(", ", nexusRpcResult.Responses.Keys);
				requester?.ConsoleMessage($"Broadcast ping took {num:F3}s, response received from zones: {arg2}");
			}
			catch (Exception arg3)
			{
				requester?.ConsoleMessage($"Failed to broadcast ping: {arg3}");
			}
		}
	}

	[ServerVar]
	public static void playeronline(Arg arg)
	{
		if (!NexusServer.Started)
		{
			arg.ReplyWith("Server is not connected to a nexus");
			return;
		}
		ulong uInt = arg.GetUInt64(0, 0uL);
		if (uInt == 0L)
		{
			arg.ReplyWith("Usage: nexus.playeronline <steamID64>");
			return;
		}
		bool flag = NexusServer.IsOnline(uInt);
		arg.ReplyWith(flag ? "Online" : "Offline");
	}

	[ServerVar(Help = "Reupload the map image to the nexus. Normally happens automatically at server boot. WARNING: This will lag the server!")]
	public static void uploadmap(Arg arg)
	{
		if (!NexusServer.Started)
		{
			arg.ReplyWith("Server is not connected to a nexus");
		}
		else
		{
			NexusServer.UploadMapImage(force: true);
		}
	}
}
