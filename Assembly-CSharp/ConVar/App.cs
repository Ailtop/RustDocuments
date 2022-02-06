using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using CompanionServer;
using Facepunch.Extend;
using Steamworks;
using UnityEngine;

namespace ConVar
{
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

		public static IPAddress GetListenIP()
		{
			if (!string.IsNullOrWhiteSpace(listenip))
			{
				IPAddress address;
				if (!IPAddress.TryParse(listenip, out address) || address.AddressFamily != AddressFamily.InterNetwork)
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
			IPAddress address;
			if (!string.IsNullOrWhiteSpace(publicip) && IPAddress.TryParse(publicip, out address) && address.AddressFamily == AddressFamily.InterNetwork)
			{
				return publicip;
			}
			return SteamServer.PublicIp.ToString();
		}
	}
}
