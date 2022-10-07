using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CircularBuffer;
using CompanionServer;
using Facepunch;
using Facepunch.Math;
using Network;
using Oxide.Core;
using UnityEngine;

namespace ConVar;

[Factory("chat")]
public class Chat : ConsoleSystem
{
	public enum ChatChannel
	{
		Global = 0,
		Team = 1,
		Server = 2,
		Cards = 3,
		Local = 4
	}

	public struct ChatEntry
	{
		public ChatChannel Channel { get; set; }

		public string Message { get; set; }

		public string UserId { get; set; }

		public string Username { get; set; }

		public string Color { get; set; }

		public int Time { get; set; }
	}

	[ServerVar]
	public static float localChatRange = 100f;

	[ReplicatedVar]
	public static bool globalchat = true;

	[ReplicatedVar]
	public static bool localchat = false;

	private const float textVolumeBoost = 0.2f;

	[ServerVar]
	[ClientVar]
	public static bool enabled = true;

	[ServerVar(Help = "Number of messages to keep in memory for chat history")]
	public static int historysize = 1000;

	public static CircularBuffer<ChatEntry> History = new CircularBuffer<ChatEntry>(historysize);

	[ServerVar]
	public static bool serverlog = true;

	public static void Broadcast(string message, string username = "SERVER", string color = "#eee", ulong userid = 0uL)
	{
		if (Interface.CallHook("OnServerMessage", message, username, color, userid) == null)
		{
			string text = username.EscapeRichText();
			ConsoleNetwork.BroadcastToAllClients("chat.add", 2, 0, "<color=" + color + ">" + text + "</color> " + message);
			ChatEntry ce = default(ChatEntry);
			ce.Channel = ChatChannel.Server;
			ce.Message = message;
			ce.UserId = userid.ToString();
			ce.Username = username;
			ce.Color = color;
			ce.Time = Epoch.Current;
			Record(ce);
		}
	}

	[ServerUserVar]
	public static void say(Arg arg)
	{
		if (globalchat)
		{
			sayImpl(ChatChannel.Global, arg);
		}
	}

	[ServerUserVar]
	public static void localsay(Arg arg)
	{
		if (localchat)
		{
			sayImpl(ChatChannel.Local, arg);
		}
	}

	[ServerUserVar]
	public static void teamsay(Arg arg)
	{
		sayImpl(ChatChannel.Team, arg);
	}

	[ServerUserVar]
	public static void cardgamesay(Arg arg)
	{
		sayImpl(ChatChannel.Cards, arg);
	}

	private static void sayImpl(ChatChannel targetChannel, Arg arg)
	{
		if (!enabled)
		{
			arg.ReplyWith("Chat is disabled.");
			return;
		}
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!basePlayer || basePlayer.HasPlayerFlag(BasePlayer.PlayerFlags.ChatMute))
		{
			return;
		}
		if (!basePlayer.IsAdmin && !basePlayer.IsDeveloper)
		{
			if (basePlayer.NextChatTime == 0f)
			{
				basePlayer.NextChatTime = UnityEngine.Time.realtimeSinceStartup - 30f;
			}
			if (basePlayer.NextChatTime > UnityEngine.Time.realtimeSinceStartup)
			{
				basePlayer.NextChatTime += 2f;
				float num = basePlayer.NextChatTime - UnityEngine.Time.realtimeSinceStartup;
				ConsoleNetwork.SendClientCommand(basePlayer.net.connection, "chat.add", 2, 0, "You're chatting too fast - try again in " + (num + 0.5f).ToString("0") + " seconds");
				if (num > 120f)
				{
					basePlayer.Kick("Chatting too fast");
				}
				return;
			}
		}
		string @string = arg.GetString(0, "text");
		if (sayAs(targetChannel, basePlayer.userID, basePlayer.displayName, @string, basePlayer))
		{
			basePlayer.NextChatTime = UnityEngine.Time.realtimeSinceStartup + 1.5f;
		}
	}

	internal static bool sayAs(ChatChannel targetChannel, ulong userId, string username, string message, BasePlayer player = null)
	{
		if (!player)
		{
			player = null;
		}
		if (!enabled)
		{
			return false;
		}
		if (player != null && player.HasPlayerFlag(BasePlayer.PlayerFlags.ChatMute))
		{
			return false;
		}
		ServerUsers.UserGroup userGroup = ServerUsers.Get(userId)?.group ?? ServerUsers.UserGroup.None;
		if (userGroup == ServerUsers.UserGroup.Banned)
		{
			return false;
		}
		string text = message.Replace("\n", "").Replace("\r", "").Trim();
		if (text.Length > 128)
		{
			text = text.Substring(0, 128);
		}
		if (text.Length <= 0)
		{
			return false;
		}
		if (text.StartsWith("/") || text.StartsWith("\\"))
		{
			Interface.CallHook("IOnPlayerCommand", player, message);
			return false;
		}
		text = text.EscapeRichText();
		object obj = Interface.CallHook("IOnPlayerChat", userId, username, text, targetChannel, player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (serverlog)
		{
			ServerConsole.PrintColoured(ConsoleColor.DarkYellow, string.Concat("[", targetChannel, "] ", username, ": "), ConsoleColor.DarkGreen, text);
			string text2 = player?.ToString() ?? $"{username}[{userId}]";
			switch (targetChannel)
			{
			case ChatChannel.Team:
				DebugEx.Log("[TEAM CHAT] " + text2 + " : " + text);
				break;
			case ChatChannel.Cards:
				DebugEx.Log("[CARDS CHAT] " + text2 + " : " + text);
				break;
			default:
				DebugEx.Log("[CHAT] " + text2 + " : " + text);
				break;
			}
		}
		bool flag = userGroup == ServerUsers.UserGroup.Owner || userGroup == ServerUsers.UserGroup.Moderator;
		bool num = ((player != null) ? player.IsDeveloper : DeveloperList.Contains(userId));
		string text3 = "#5af";
		if (flag)
		{
			text3 = "#af5";
		}
		if (num)
		{
			text3 = "#fa5";
		}
		string text4 = username.EscapeRichText();
		ChatEntry ce = default(ChatEntry);
		ce.Channel = targetChannel;
		ce.Message = text;
		ce.UserId = ((player != null) ? player.UserIDString : userId.ToString());
		ce.Username = username;
		ce.Color = text3;
		ce.Time = Epoch.Current;
		Record(ce);
		switch (targetChannel)
		{
		case ChatChannel.Cards:
		{
			if (player == null)
			{
				return false;
			}
			if (!player.isMounted)
			{
				return false;
			}
			CardTable cardTable = player.GetMountedVehicle() as CardTable;
			if (cardTable == null || !cardTable.GameController.PlayerIsInGame(player))
			{
				return false;
			}
			List<Network.Connection> obj2 = Facepunch.Pool.GetList<Network.Connection>();
			cardTable.GameController.GetConnectionsInGame(obj2);
			if (obj2.Count > 0)
			{
				ConsoleNetwork.SendClientCommand(obj2, "chat.add2", 3, userId, text, text4, text3, 1f);
			}
			Facepunch.Pool.FreeList(ref obj2);
			return true;
		}
		case ChatChannel.Global:
			ConsoleNetwork.BroadcastToAllClients("chat.add2", 0, userId, text, text4, text3, 1f);
			return true;
		case ChatChannel.Local:
		{
			if (!(player != null))
			{
				break;
			}
			float num2 = localChatRange * localChatRange;
			foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
			{
				float sqrMagnitude = (activePlayer.transform.position - player.transform.position).sqrMagnitude;
				if (!(sqrMagnitude > num2))
				{
					ConsoleNetwork.SendClientCommand(activePlayer.net.connection, "chat.add2", 4, userId, text, text4, text3, Mathf.Clamp01(sqrMagnitude / num2 + 0.2f));
				}
			}
			return true;
		}
		case ChatChannel.Team:
		{
			RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindPlayersTeam(userId);
			if (playerTeam == null)
			{
				return false;
			}
			List<Network.Connection> onlineMemberConnections = playerTeam.GetOnlineMemberConnections();
			if (onlineMemberConnections != null)
			{
				ConsoleNetwork.SendClientCommand(onlineMemberConnections, "chat.add2", 1, userId, text, text4, text3, 1f);
			}
			Util.BroadcastTeamChat(playerTeam, userId, text4, text, text3);
			return true;
		}
		}
		return false;
	}

	[ServerVar]
	[Help("Return the last x lines of the console. Default is 200")]
	public static IEnumerable<ChatEntry> tail(Arg arg)
	{
		int @int = arg.GetInt(0, 200);
		int num = History.Size - @int;
		if (num < 0)
		{
			num = 0;
		}
		return History.Skip(num);
	}

	[ServerVar]
	[Help("Search the console for a particular string")]
	public static IEnumerable<ChatEntry> search(Arg arg)
	{
		string search = arg.GetString(0, null);
		if (search == null)
		{
			return Enumerable.Empty<ChatEntry>();
		}
		return History.Where((ChatEntry x) => x.Message.Length < 4096 && x.Message.Contains(search, CompareOptions.IgnoreCase));
	}

	public static void Record(ChatEntry ce)
	{
		int num = Mathf.Max(historysize, 10);
		if (History.Capacity != num)
		{
			CircularBuffer<ChatEntry> circularBuffer = new CircularBuffer<ChatEntry>(num);
			foreach (ChatEntry item in History)
			{
				circularBuffer.PushBack(item);
			}
			History = circularBuffer;
		}
		History.PushBack(ce);
		RCon.Broadcast(RCon.LogType.Chat, ce);
	}
}
