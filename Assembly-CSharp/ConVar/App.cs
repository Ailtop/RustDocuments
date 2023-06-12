using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using CompanionServer;
using Facepunch.Extend;
using Steamworks;
using UnityEngine;

namespace ConVar;

[Factory("app")]
public class App : ConsoleSystem
{
	[ServerVar]
	public static string listenip = "";

	[ServerVar]
	public static int port;

	[ServerVar]
	public static string publicip = "";

	[ServerVar(Help = "Disables updating entirely - emergency use only")]
	public static bool update = true;

	[ServerVar(Help = "Enables sending push notifications")]
	public static bool notifications = true;

	[ServerVar(Help = "Max number of queued messages - set to 0 to disable message processing")]
	public static int queuelimit = 100;

	[ReplicatedVar(Default = "")]
	public static string serverid = "";

	[ServerVar(Help = "Cooldown time before alarms can send another notification (in seconds)")]
	public static float alarmcooldown = 30f;

	[ServerVar]
	public static int maxconnections = 500;

	[ServerVar]
	public static int maxconnectionsperip = 5;

	[ServerVar]
	public static int maxmessagesize = 1048576;

	[ServerUserVar]
	public static async void pair(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!(basePlayer == null))
		{
			Dictionary<string, string> playerPairingData = Util.GetPlayerPairingData(basePlayer);
			NotificationSendResult notificationSendResult = await Util.SendPairNotification("server", basePlayer, Server.hostname.Truncate(128), "Tap to pair with this server.", playerPairingData);
			arg.ReplyWith((notificationSendResult == NotificationSendResult.Sent) ? "Sent pairing notification." : Util.ToErrorMessage(notificationSendResult));
		}
	}

	[ServerUserVar]
	public static void regeneratetoken(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!(basePlayer == null))
		{
			SingletonComponent<ServerMgr>.Instance.persistance.RegenerateAppToken(basePlayer.userID);
			arg.ReplyWith("Regenerated Rust+ token");
		}
	}

	[ServerVar]
	public static void info(Arg arg)
	{
		if (!CompanionServer.Server.IsEnabled)
		{
			arg.ReplyWith("Companion server is not enabled");
			return;
		}
		Listener listener = CompanionServer.Server.Listener;
		arg.ReplyWith($"Server ID: {serverid}\nListening on: {listener.Address}:{listener.Port}\nApp connects to: {GetPublicIP()}:{port}");
	}

	[ServerVar]
	public static void resetlimiter(Arg arg)
	{
		CompanionServer.Server.Listener?.Limiter?.Clear();
	}

	[ServerVar]
	public static void connections(Arg arg)
	{
		string strValue = CompanionServer.Server.Listener?.Limiter?.ToString() ?? "Not available";
		arg.ReplyWith(strValue);
	}

	[ServerVar]
	public static void appban(Arg arg)
	{
		ulong uLong = arg.GetULong(0, 0uL);
		if (uLong == 0L)
		{
			arg.ReplyWith("Usage: app.appban <steamID64>");
			return;
		}
		string strValue = (SingletonComponent<ServerMgr>.Instance.persistance.SetAppTokenLocked(uLong, locked: true) ? $"Banned {uLong} from using the companion app" : $"{uLong} is already banned from using the companion app");
		arg.ReplyWith(strValue);
	}

	[ServerVar]
	public static void appunban(Arg arg)
	{
		ulong uLong = arg.GetULong(0, 0uL);
		if (uLong == 0L)
		{
			arg.ReplyWith("Usage: app.appunban <steamID64>");
			return;
		}
		string strValue = (SingletonComponent<ServerMgr>.Instance.persistance.SetAppTokenLocked(uLong, locked: false) ? $"Unbanned {uLong}, they can use the companion app again" : $"{uLong} is not banned from using the companion app");
		arg.ReplyWith(strValue);
	}

	public static IPAddress GetListenIP()
	{
		if (!string.IsNullOrWhiteSpace(listenip))
		{
			if (!IPAddress.TryParse(listenip, out var address) || address.AddressFamily != AddressFamily.InterNetwork)
			{
				Debug.LogError("Invalid app.listenip: " + listenip);
				return IPAddress.Any;
			}
			return address;
		}
		return IPAddress.Any;
	}

	public static string GetPublicIP()
	{
		if (!string.IsNullOrWhiteSpace(publicip) && IPAddress.TryParse(publicip, out var address) && address.AddressFamily == AddressFamily.InterNetwork)
		{
			return publicip;
		}
		return SteamServer.PublicIp.ToString();
	}
}
