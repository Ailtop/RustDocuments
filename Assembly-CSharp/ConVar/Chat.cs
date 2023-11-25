using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CircularBuffer;
using CompanionServer;
using Facepunch;
using Facepunch.Math;
using Facepunch.Rust;
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
		Local = 4,
		Clan = 5,
		MaxValue = 6
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

	[ClientVar]
	[ServerVar]
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

	[ServerUserVar]
	public static void clansay(Arg arg)
	{
		sayImpl(ChatChannel.Clan, arg);
	}

	private static void sayImpl(ChatChannel targetChannel, Arg arg)
	{
		if (!enabled)
		{
			arg.ReplyWith("Chat is disabled.");
			return;
		}
		BasePlayer player = ArgEx.Player(arg);
		if (!player || player.HasPlayerFlag(BasePlayer.PlayerFlags.ChatMute))
		{
			return;
		}
		if (!player.IsAdmin && !player.IsDeveloper)
		{
			if (player.NextChatTime == 0f)
			{
				player.NextChatTime = UnityEngine.Time.realtimeSinceStartup - 30f;
			}
			if (player.NextChatTime > UnityEngine.Time.realtimeSinceStartup)
			{
				player.NextChatTime += 2f;
				float num = player.NextChatTime - UnityEngine.Time.realtimeSinceStartup;
				ConsoleNetwork.SendClientCommand(player.net.connection, "chat.add", 2, 0, "You're chatting too fast - try again in " + (num + 0.5f).ToString("0") + " seconds");
				if (num > 120f)
				{
					player.Kick("Chatting too fast");
				}
				return;
			}
		}
		string @string = arg.GetString(0, "text");
		ValueTask<bool> valueTask = sayAs(targetChannel, player.userID, player.displayName, @string, player);
		Facepunch.Rust.Analytics.Azure.OnChatMessage(player, @string, (int)targetChannel);
		player.NextChatTime = UnityEngine.Time.realtimeSinceStartup + 1.5f;
		if (valueTask.IsCompletedSuccessfully)
		{
			if (!valueTask.Result)
			{
				player.NextChatTime = UnityEngine.Time.realtimeSinceStartup;
			}
			return;
		}
		Task<bool> task = valueTask.AsTask();
		task.GetAwaiter().OnCompleted(delegate
		{
			try
			{
				if (!task.Result)
				{
					player.NextChatTime = UnityEngine.Time.realtimeSinceStartup;
				}
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		});
	}

	internal static string GetNameColor(ulong userId, BasePlayer player = null)
	{
		ServerUsers.UserGroup userGroup = ServerUsers.Get(userId)?.group ?? ServerUsers.UserGroup.None;
		bool flag = userGroup == ServerUsers.UserGroup.Owner || userGroup == ServerUsers.UserGroup.Moderator;
		bool num = ((player != null) ? player.IsDeveloper : DeveloperList.Contains(userId));
		string result = "#5af";
		if (flag)
		{
			result = "#af5";
		}
		if (num)
		{
			result = "#fa5";
		}
		return result;
	}

	internal static async ValueTask<bool> sayAs(ChatChannel targetChannel, ulong userId, string username, string message, BasePlayer player = null)
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
		if ((ServerUsers.Get(userId)?.group ?? ServerUsers.UserGroup.None) == ServerUsers.UserGroup.Banned)
		{
			return false;
		}
		string strChatText = message.Replace("\n", "").Replace("\r", "").Trim();
		if (strChatText.Length > 128)
		{
			strChatText = strChatText.Substring(0, 128);
		}
		if (strChatText.Length <= 0)
		{
			return false;
		}
		if (strChatText.StartsWith("/") || strChatText.StartsWith("\\"))
		{
			Interface.CallHook("IOnPlayerCommand", player, message);
			return false;
		}
		strChatText = strChatText.EscapeRichText();
		object obj = Interface.CallHook("IOnPlayerChat", userId, username, strChatText, targetChannel, player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (Server.emojiOwnershipCheck)
		{
			List<(TmProEmojiRedirector.EmojiSub, int)> obj2 = Facepunch.Pool.GetList<(TmProEmojiRedirector.EmojiSub, int)>();
			TmProEmojiRedirector.FindEmojiSubstitutions(strChatText, RustEmojiLibrary.Instance, obj2, richText: false, isServer: true);
			bool flag = true;
			foreach (var item in obj2)
			{
				if (!item.Item1.targetEmojiResult.CanBeUsedBy(player))
				{
					flag = false;
					break;
				}
			}
			Facepunch.Pool.FreeList(ref obj2);
			if (!flag)
			{
				Debug.Log("player tried to use emoji they don't own, reject!");
				return false;
			}
		}
		if (serverlog)
		{
			ServerConsole.PrintColoured(ConsoleColor.DarkYellow, "[" + targetChannel.ToString() + "] " + username + ": ", ConsoleColor.DarkGreen, strChatText);
			string text = player?.ToString() ?? $"{username}[{userId}]";
			switch (targetChannel)
			{
			case ChatChannel.Team:
				DebugEx.Log("[TEAM CHAT] " + text + " : " + strChatText);
				break;
			case ChatChannel.Cards:
				DebugEx.Log("[CARDS CHAT] " + text + " : " + strChatText);
				break;
			case ChatChannel.Clan:
				DebugEx.Log("[CLAN CHAT] " + text + " : " + strChatText);
				break;
			default:
				DebugEx.Log("[CHAT] " + text + " : " + strChatText);
				break;
			}
		}
		string strName = username.EscapeRichText();
		string nameColor = GetNameColor(userId, player);
		ChatEntry ce = default(ChatEntry);
		ce.Channel = targetChannel;
		ce.Message = strChatText;
		ce.UserId = ((player != null) ? player.UserIDString : userId.ToString());
		ce.Username = username;
		ce.Color = nameColor;
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
			BaseCardGameEntity baseCardGameEntity = player.GetMountedVehicle() as BaseCardGameEntity;
			if (baseCardGameEntity == null || !baseCardGameEntity.GameController.IsAtTable(player))
			{
				return false;
			}
			List<Network.Connection> obj3 = Facepunch.Pool.GetList<Network.Connection>();
			baseCardGameEntity.GameController.GetConnectionsInGame(obj3);
			if (obj3.Count > 0)
			{
				ConsoleNetwork.SendClientCommand(obj3, "chat.add2", 3, userId, strChatText, strName, nameColor, 1f);
			}
			Facepunch.Pool.FreeList(ref obj3);
			return true;
		}
		case ChatChannel.Global:
			ConsoleNetwork.BroadcastToAllClients("chat.add2", 0, userId, strChatText, strName, nameColor, 1f);
			return true;
		case ChatChannel.Local:
		{
			if (!(player != null))
			{
				break;
			}
			float num = localChatRange * localChatRange;
			foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
			{
				float sqrMagnitude = (activePlayer.transform.position - player.transform.position).sqrMagnitude;
				if (!(sqrMagnitude > num))
				{
					ConsoleNetwork.SendClientCommand(activePlayer.net.connection, "chat.add2", 4, userId, strChatText, strName, nameColor, Mathf.Clamp01(sqrMagnitude / num + 0.2f));
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
				ConsoleNetwork.SendClientCommand(onlineMemberConnections, "chat.add2", 1, userId, strChatText, strName, nameColor, 1f);
			}
			Util.BroadcastTeamChat(playerTeam, userId, strName, strChatText, nameColor);
			return true;
		}
		case ChatChannel.Clan:
		{
			ClanManager serverInstance = ClanManager.ServerInstance;
			if (serverInstance == null)
			{
				return false;
			}
			if (player != null && player.clanId == 0L)
			{
				return false;
			}
			try
			{
				ClanValueResult<IClan> clanValueResult = ((!(player != null) || player.clanId == 0L) ? (await serverInstance.Backend.GetByMember(userId)) : (await serverInstance.Backend.Get(player.clanId)));
				ClanValueResult<IClan> clanValueResult2 = clanValueResult;
				if (!clanValueResult2.IsSuccess)
				{
					return false;
				}
				if (await clanValueResult2.Value.SendChatMessage(strName, strChatText, userId) != ClanResult.Success)
				{
					return false;
				}
				return true;
			}
			catch (Exception message2)
			{
				Debug.LogError(message2);
				return false;
			}
		}
		}
		return false;
	}

	[Help("Return the last x lines of the console. Default is 200")]
	[ServerVar]
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

	[Help("Search the console for a particular string")]
	[ServerVar]
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
