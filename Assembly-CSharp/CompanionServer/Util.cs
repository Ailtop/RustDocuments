using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CompanionServer.Handlers;
using ConVar;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Math;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer;

public static class Util
{
	public const int OceanMargin = 500;

	public static readonly Translate.Phrase NotificationEmpty = new Translate.Phrase("app.error.empty", "Notification was not sent because it was missing some content.");

	public static readonly Translate.Phrase NotificationDisabled = new Translate.Phrase("app.error.disabled", "Rust+ features are disabled on this server.");

	public static readonly Translate.Phrase NotificationRateLimit = new Translate.Phrase("app.error.ratelimit", "You are sending too many notifications at a time. Please wait and then try again.");

	public static readonly Translate.Phrase NotificationServerError = new Translate.Phrase("app.error.servererror", "The companion server failed to send the notification.");

	public static readonly Translate.Phrase NotificationNoTargets = new Translate.Phrase("app.error.notargets", "Open the Rust+ menu in-game to pair your phone with this server.");

	public static readonly Translate.Phrase NotificationTooManySubscribers = new Translate.Phrase("app.error.toomanysubs", "There are too many players subscribed to these notifications.");

	public static readonly Translate.Phrase NotificationUnknown = new Translate.Phrase("app.error.unknown", "An unknown error occurred sending the notification.");

	public static Vector2 WorldToMap(Vector3 worldPos)
	{
		return new Vector2(worldPos.x - TerrainMeta.Position.x, worldPos.z - TerrainMeta.Position.z);
	}

	public static void SendSignedInNotification(BasePlayer player)
	{
		if (!(player == null) && player.currentTeam != 0L)
		{
			RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindTeam(player.currentTeam);
			Dictionary<string, string> serverPairingData = GetServerPairingData();
			serverPairingData.Add("type", "login");
			serverPairingData.Add("targetId", player.UserIDString);
			serverPairingData.Add("targetName", player.displayName.Truncate(128));
			if (playerTeam != null)
			{
				SendNotification(playerTeam, NotificationChannel.PlayerLoggedIn, player.displayName + " is now online", ConVar.Server.hostname, serverPairingData, player.userID);
			}
		}
	}

	public static void SendDeathNotification(BasePlayer player, BaseEntity killer)
	{
		string value;
		string text;
		if (killer is BasePlayer basePlayer && basePlayer.GetType() == typeof(BasePlayer))
		{
			value = basePlayer.UserIDString;
			text = basePlayer.displayName;
		}
		else
		{
			value = "";
			text = killer.ShortPrefabName;
		}
		if (!(player == null) && !string.IsNullOrEmpty(text))
		{
			Dictionary<string, string> serverPairingData = GetServerPairingData();
			serverPairingData.Add("type", "death");
			serverPairingData.Add("targetId", value);
			serverPairingData.Add("targetName", text.Truncate(128));
			NotificationList.SendNotificationTo(player.userID, NotificationChannel.PlayerDied, "You were killed by " + text, ConVar.Server.hostname, serverPairingData);
		}
	}

	public static Task<NotificationSendResult> SendPairNotification(string type, BasePlayer player, string title, string message, Dictionary<string, string> data)
	{
		if (!Server.IsEnabled)
		{
			return Task.FromResult(NotificationSendResult.Disabled);
		}
		if (!Server.CanSendPairingNotification(player.userID))
		{
			return Task.FromResult(NotificationSendResult.RateLimited);
		}
		if (data == null)
		{
			data = GetPlayerPairingData(player);
		}
		data.Add("type", type);
		return NotificationList.SendNotificationTo(player.userID, NotificationChannel.Pairing, title, message, data);
	}

	public static Dictionary<string, string> GetServerPairingData()
	{
		Dictionary<string, string> dictionary = Facepunch.Pool.Get<Dictionary<string, string>>();
		dictionary.Clear();
		dictionary.Add("id", App.serverid);
		dictionary.Add("name", ConVar.Server.hostname.Truncate(128));
		dictionary.Add("desc", ConVar.Server.description.Truncate(512));
		dictionary.Add("img", ConVar.Server.headerimage.Truncate(128));
		dictionary.Add("logo", ConVar.Server.logoimage.Truncate(128));
		dictionary.Add("url", ConVar.Server.url.Truncate(128));
		dictionary.Add("ip", App.GetPublicIP());
		dictionary.Add("port", App.port.ToString("G", CultureInfo.InvariantCulture));
		if (NexusServer.Started)
		{
			int? nexusId = NexusServer.NexusId;
			string zoneKey = NexusServer.ZoneKey;
			if (nexusId.HasValue && zoneKey != null)
			{
				dictionary.Add("nexus", Nexus.endpoint);
				dictionary.Add("nexusId", nexusId.Value.ToString("G"));
				dictionary.Add("nexusZone", zoneKey);
			}
		}
		return dictionary;
	}

	public static Dictionary<string, string> GetPlayerPairingData(BasePlayer player)
	{
		bool locked;
		int orGenerateAppToken = SingletonComponent<ServerMgr>.Instance.persistance.GetOrGenerateAppToken(player.userID, out locked);
		Dictionary<string, string> serverPairingData = GetServerPairingData();
		serverPairingData.Add("playerId", player.UserIDString);
		serverPairingData.Add("playerToken", orGenerateAppToken.ToString("G", CultureInfo.InvariantCulture));
		return serverPairingData;
	}

	public static void BroadcastAppTeamRemoval(this BasePlayer player)
	{
		AppBroadcast appBroadcast = Facepunch.Pool.Get<AppBroadcast>();
		appBroadcast.teamChanged = Facepunch.Pool.Get<AppTeamChanged>();
		appBroadcast.teamChanged.playerId = player.userID;
		appBroadcast.teamChanged.teamInfo = AppPlayerExtensions.GetAppTeamInfo(player, player.userID);
		Server.Broadcast(new PlayerTarget(player.userID), appBroadcast);
	}

	public static void BroadcastAppTeamUpdate(this RelationshipManager.PlayerTeam team)
	{
		AppBroadcast appBroadcast = Facepunch.Pool.Get<AppBroadcast>();
		appBroadcast.teamChanged = Facepunch.Pool.Get<AppTeamChanged>();
		appBroadcast.ShouldPool = false;
		foreach (ulong member in team.members)
		{
			appBroadcast.teamChanged.playerId = member;
			appBroadcast.teamChanged.teamInfo = AppPlayerExtensions.GetAppTeamInfo(team, member);
			Server.Broadcast(new PlayerTarget(member), appBroadcast);
		}
		appBroadcast.ShouldPool = true;
		appBroadcast.Dispose();
	}

	public static void BroadcastTeamChat(this RelationshipManager.PlayerTeam team, ulong steamId, string name, string message, string color)
	{
		uint current = (uint)Epoch.Current;
		Server.TeamChat.Record(team.teamID, steamId, name, message, color, current);
		AppBroadcast appBroadcast = Facepunch.Pool.Get<AppBroadcast>();
		appBroadcast.teamMessage = Facepunch.Pool.Get<AppNewTeamMessage>();
		appBroadcast.teamMessage.message = Facepunch.Pool.Get<AppTeamMessage>();
		appBroadcast.ShouldPool = false;
		AppTeamMessage message2 = appBroadcast.teamMessage.message;
		message2.steamId = steamId;
		message2.name = name;
		message2.message = message;
		message2.color = color;
		message2.time = current;
		foreach (ulong member in team.members)
		{
			Server.Broadcast(new PlayerTarget(member), appBroadcast);
		}
		appBroadcast.ShouldPool = true;
		appBroadcast.Dispose();
	}

	public static async void SendNotification(this RelationshipManager.PlayerTeam team, NotificationChannel channel, string title, string body, Dictionary<string, string> data, ulong ignorePlayer = 0uL)
	{
		List<ulong> steamIds = Facepunch.Pool.GetList<ulong>();
		foreach (ulong member in team.members)
		{
			if (member != ignorePlayer)
			{
				BasePlayer basePlayer = RelationshipManager.FindByID(member);
				if (basePlayer == null || basePlayer.net?.connection == null)
				{
					steamIds.Add(member);
				}
			}
		}
		await NotificationList.SendNotificationTo(steamIds, channel, title, body, data);
		Facepunch.Pool.FreeList(ref steamIds);
	}

	public static string ToErrorCode(this ValidationResult result)
	{
		return result switch
		{
			ValidationResult.NotFound => "not_found", 
			ValidationResult.RateLimit => "rate_limit", 
			ValidationResult.Banned => "banned", 
			_ => "unknown", 
		};
	}

	public static string ToErrorMessage(this NotificationSendResult result)
	{
		return result switch
		{
			NotificationSendResult.Sent => null, 
			NotificationSendResult.Empty => NotificationEmpty.translated, 
			NotificationSendResult.Disabled => NotificationDisabled.translated, 
			NotificationSendResult.RateLimited => NotificationRateLimit.translated, 
			NotificationSendResult.ServerError => NotificationServerError.translated, 
			NotificationSendResult.NoTargetsFound => NotificationNoTargets.translated, 
			NotificationSendResult.TooManySubscribers => NotificationTooManySubscribers.translated, 
			_ => NotificationUnknown.translated, 
		};
	}
}
