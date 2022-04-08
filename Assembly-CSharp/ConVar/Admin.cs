using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Math;
using Network;
using Newtonsoft.Json;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Scripting;

namespace ConVar
{
	[Factory("global")]
	public class Admin : ConsoleSystem
	{
		[Preserve]
		public struct PlayerInfo
		{
			public string SteamID;

			public string OwnerSteamID;

			public string DisplayName;

			public int Ping;

			public string Address;

			public int ConnectedSeconds;

			public float VoiationLevel;

			public float CurrentLevel;

			public float UnspentXp;

			public float Health;
		}

		[Preserve]
		public struct ServerInfoOutput
		{
			public string Hostname;

			public int MaxPlayers;

			public int Players;

			public int Queued;

			public int Joining;

			public int EntityCount;

			public string GameTime;

			public int Uptime;

			public string Map;

			public float Framerate;

			public int Memory;

			public int Collections;

			public int NetworkIn;

			public int NetworkOut;

			public bool Restarting;

			public string SaveCreatedTime;
		}

		[Preserve]
		public struct ServerConvarInfo
		{
			public string FullName;

			public string Value;

			public string Help;
		}

		[ReplicatedVar(Help = "Controls whether the in-game admin UI is displayed to admins")]
		public static bool allowAdminUI = true;

		[ServerVar(Help = "Print out currently connected clients")]
		public static void status(Arg arg)
		{
			string @string = arg.GetString(0);
			string text = string.Empty;
			if (@string.Length == 0)
			{
				text = text + "hostname: " + Server.hostname + "\n";
				text = text + "version : " + 2332 + " secure (secure mode enabled, connected to Steam3)\n";
				text = text + "map     : " + Server.level + "\n";
				text += $"players : {BasePlayer.activePlayerList.Count()} ({Server.maxplayers} max) ({SingletonComponent<ServerMgr>.Instance.connectionQueue.Queued} queued) ({SingletonComponent<ServerMgr>.Instance.connectionQueue.Joining} joining)\n\n";
			}
			TextTable textTable = new TextTable();
			textTable.AddColumn("id");
			textTable.AddColumn("name");
			textTable.AddColumn("ping");
			textTable.AddColumn("connected");
			textTable.AddColumn("addr");
			textTable.AddColumn("owner");
			textTable.AddColumn("violation");
			textTable.AddColumn("kicks");
			foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
			{
				try
				{
					if (!BaseEntityEx.IsValid(activePlayer))
					{
						continue;
					}
					string userIDString = activePlayer.UserIDString;
					if (activePlayer.net.connection == null)
					{
						textTable.AddRow(userIDString, "NO CONNECTION");
						continue;
					}
					string text2 = activePlayer.net.connection.ownerid.ToString();
					string text3 = activePlayer.displayName.QuoteSafe();
					string text4 = Network.Net.sv.GetAveragePing(activePlayer.net.connection).ToString();
					string text5 = activePlayer.net.connection.ipaddress;
					string text6 = activePlayer.violationLevel.ToString("0.0");
					string text7 = activePlayer.GetAntiHackKicks().ToString();
					if (!arg.IsAdmin && !arg.IsRcon)
					{
						text5 = "xx.xxx.xx.xxx";
					}
					string text8 = activePlayer.net.connection.GetSecondsConnected() + "s";
					if (@string.Length <= 0 || text3.Contains(@string, CompareOptions.IgnoreCase) || userIDString.Contains(@string) || text2.Contains(@string) || text5.Contains(@string))
					{
						textTable.AddRow(userIDString, text3, text4, text8, text5, (text2 == userIDString) ? string.Empty : text2, text6, text7);
					}
				}
				catch (Exception ex)
				{
					textTable.AddRow(activePlayer.UserIDString, ex.Message.QuoteSafe());
				}
			}
			arg.ReplyWith(text + textTable.ToString());
		}

		[ServerVar(Help = "Print out stats of currently connected clients")]
		public static void stats(Arg arg)
		{
			TextTable table = new TextTable();
			table.AddColumn("id");
			table.AddColumn("name");
			table.AddColumn("time");
			table.AddColumn("kills");
			table.AddColumn("deaths");
			table.AddColumn("suicides");
			table.AddColumn("player");
			table.AddColumn("building");
			table.AddColumn("entity");
			Action<ulong, string> action = delegate(ulong id, string name)
			{
				ServerStatistics.Storage storage = ServerStatistics.Get(id);
				string text2 = TimeSpanEx.ToShortString(TimeSpan.FromSeconds(storage.Get("time")));
				string text3 = storage.Get("kill_player").ToString();
				string text4 = (storage.Get("deaths") - storage.Get("death_suicide")).ToString();
				string text5 = storage.Get("death_suicide").ToString();
				string text6 = storage.Get("hit_player_direct_los").ToString();
				string text7 = storage.Get("hit_player_indirect_los").ToString();
				string text8 = storage.Get("hit_building_direct_los").ToString();
				string text9 = storage.Get("hit_building_indirect_los").ToString();
				string text10 = storage.Get("hit_entity_direct_los").ToString();
				string text11 = storage.Get("hit_entity_indirect_los").ToString();
				table.AddRow(id.ToString(), name, text2, text3, text4, text5, text6 + " / " + text7, text8 + " / " + text9, text10 + " / " + text11);
			};
			ulong uInt = arg.GetUInt64(0, 0uL);
			if (uInt == 0L)
			{
				string @string = arg.GetString(0);
				foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
				{
					try
					{
						if (BaseEntityEx.IsValid(activePlayer))
						{
							string text = activePlayer.displayName.QuoteSafe();
							if (@string.Length <= 0 || text.Contains(@string, CompareOptions.IgnoreCase))
							{
								action(activePlayer.userID, text);
							}
						}
					}
					catch (Exception ex)
					{
						table.AddRow(activePlayer.UserIDString, ex.Message.QuoteSafe());
					}
				}
			}
			else
			{
				string arg2 = "N/A";
				BasePlayer basePlayer = BasePlayer.FindByID(uInt);
				if ((bool)basePlayer)
				{
					arg2 = basePlayer.displayName.QuoteSafe();
				}
				action(uInt, arg2);
			}
			arg.ReplyWith(table.ToString());
		}

		[ServerVar]
		public static void killplayer(Arg arg)
		{
			BasePlayer basePlayer = ArgEx.GetPlayerOrSleeper(arg, 0);
			if (!basePlayer)
			{
				basePlayer = BasePlayer.FindBotClosestMatch(arg.GetString(0));
			}
			if (!basePlayer)
			{
				arg.ReplyWith("Player not found");
			}
			else
			{
				basePlayer.Hurt(1000f, DamageType.Suicide, basePlayer, false);
			}
		}

		[ServerVar]
		public static void injureplayer(Arg arg)
		{
			BasePlayer basePlayer = ArgEx.GetPlayerOrSleeper(arg, 0);
			if (!basePlayer)
			{
				basePlayer = BasePlayer.FindBotClosestMatch(arg.GetString(0));
			}
			if (!basePlayer)
			{
				arg.ReplyWith("Player not found");
			}
			else
			{
				Global.InjurePlayer(basePlayer);
			}
		}

		[ServerVar]
		public static void recoverplayer(Arg arg)
		{
			BasePlayer basePlayer = ArgEx.GetPlayerOrSleeper(arg, 0);
			if (!basePlayer)
			{
				basePlayer = BasePlayer.FindBotClosestMatch(arg.GetString(0));
			}
			if (!basePlayer)
			{
				arg.ReplyWith("Player not found");
			}
			else
			{
				Global.RecoverPlayer(basePlayer);
			}
		}

		[ServerVar]
		public static void kick(Arg arg)
		{
			BasePlayer player = ArgEx.GetPlayer(arg, 0);
			if (!player || player.net == null || player.net.connection == null)
			{
				arg.ReplyWith("Player not found");
				return;
			}
			string @string = arg.GetString(1, "no reason given");
			arg.ReplyWith("Kicked: " + player.displayName);
			Chat.Broadcast("Kicking " + player.displayName + " (" + @string + ")", "SERVER", "#eee", 0uL);
			player.Kick("Kicked: " + arg.GetString(1, "No Reason Given"));
		}

		[ServerVar]
		public static void kickall(Arg arg)
		{
			BasePlayer[] array = BasePlayer.activePlayerList.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Kick("Kicked: " + arg.GetString(1, "No Reason Given"));
			}
		}

		[ServerVar(Help = "ban <player> <reason> [optional duration]")]
		public static void ban(Arg arg)
		{
			BasePlayer player = ArgEx.GetPlayer(arg, 0);
			if (!player || player.net == null || player.net.connection == null)
			{
				arg.ReplyWith("Player not found");
				return;
			}
			ServerUsers.User user = ServerUsers.Get(player.userID);
			if (user != null && user.group == ServerUsers.UserGroup.Banned)
			{
				arg.ReplyWith($"User {player.userID} is already banned");
				return;
			}
			string @string = arg.GetString(1, "No Reason Given");
			long expiry;
			string durationSuffix;
			if (TryGetBanExpiry(arg, 2, out expiry, out durationSuffix))
			{
				ServerUsers.Set(player.userID, ServerUsers.UserGroup.Banned, player.displayName, @string, expiry);
				string text = "";
				if (player.IsConnected && player.net.connection.ownerid != 0L && player.net.connection.ownerid != player.net.connection.userid)
				{
					text += $" and also banned ownerid {player.net.connection.ownerid}";
					ServerUsers.Set(player.net.connection.ownerid, ServerUsers.UserGroup.Banned, player.displayName, arg.GetString(1, $"Family share owner of {player.net.connection.userid}"), -1L);
				}
				ServerUsers.Save();
				arg.ReplyWith($"Kickbanned User{durationSuffix}: {player.userID} - {player.displayName}{text}");
				Chat.Broadcast("Kickbanning " + player.displayName + durationSuffix + " (" + @string + ")", "SERVER", "#eee", 0uL);
				Network.Net.sv.Kick(player.net.connection, "Banned" + durationSuffix + ": " + @string);
			}
		}

		[ServerVar]
		public static void moderatorid(Arg arg)
		{
			ulong uInt = arg.GetUInt64(0, 0uL);
			string @string = arg.GetString(1, "unnamed");
			string string2 = arg.GetString(2, "no reason");
			if (uInt < 70000000000000000L)
			{
				arg.ReplyWith("This doesn't appear to be a 64bit steamid: " + uInt);
				return;
			}
			ServerUsers.User user = ServerUsers.Get(uInt);
			if (user != null && user.group == ServerUsers.UserGroup.Moderator)
			{
				arg.ReplyWith("User " + uInt + " is already a Moderator");
				return;
			}
			ServerUsers.Set(uInt, ServerUsers.UserGroup.Moderator, @string, string2, -1L);
			arg.ReplyWith("Added moderator " + @string + ", steamid " + uInt);
		}

		[ServerVar]
		public static void ownerid(Arg arg)
		{
			ulong uInt = arg.GetUInt64(0, 0uL);
			string @string = arg.GetString(1, "unnamed");
			string string2 = arg.GetString(2, "no reason");
			if (uInt < 70000000000000000L)
			{
				arg.ReplyWith("This doesn't appear to be a 64bit steamid: " + uInt);
				return;
			}
			ServerUsers.User user = ServerUsers.Get(uInt);
			if (user != null && user.group == ServerUsers.UserGroup.Owner)
			{
				arg.ReplyWith("User " + uInt + " is already an Owner");
				return;
			}
			ServerUsers.Set(uInt, ServerUsers.UserGroup.Owner, @string, string2, -1L);
			arg.ReplyWith("Added owner " + @string + ", steamid " + uInt);
		}

		[ServerVar]
		public static void removemoderator(Arg arg)
		{
			ulong uInt = arg.GetUInt64(0, 0uL);
			if (uInt < 70000000000000000L)
			{
				arg.ReplyWith("This doesn't appear to be a 64bit steamid: " + uInt);
				return;
			}
			ServerUsers.User user = ServerUsers.Get(uInt);
			if (user == null || user.group != ServerUsers.UserGroup.Moderator)
			{
				arg.ReplyWith("User " + uInt + " isn't a moderator");
				return;
			}
			ServerUsers.Remove(uInt);
			arg.ReplyWith("Removed Moderator: " + uInt);
		}

		[ServerVar]
		public static void removeowner(Arg arg)
		{
			ulong uInt = arg.GetUInt64(0, 0uL);
			if (uInt < 70000000000000000L)
			{
				arg.ReplyWith("This doesn't appear to be a 64bit steamid: " + uInt);
				return;
			}
			ServerUsers.User user = ServerUsers.Get(uInt);
			if (user == null || user.group != ServerUsers.UserGroup.Owner)
			{
				arg.ReplyWith("User " + uInt + " isn't an owner");
				return;
			}
			ServerUsers.Remove(uInt);
			arg.ReplyWith("Removed Owner: " + uInt);
		}

		[ServerVar(Help = "banid <steamid> <username> <reason> [optional duration]")]
		public static void banid(Arg arg)
		{
			ulong uInt = arg.GetUInt64(0, 0uL);
			string text = arg.GetString(1, "unnamed");
			string @string = arg.GetString(2, "no reason");
			if (uInt < 70000000000000000L)
			{
				arg.ReplyWith("This doesn't appear to be a 64bit steamid: " + uInt);
				return;
			}
			ServerUsers.User user = ServerUsers.Get(uInt);
			if (user != null && user.group == ServerUsers.UserGroup.Banned)
			{
				arg.ReplyWith("User " + uInt + " is already banned");
			}
			else
			{
				long expiry;
				string durationSuffix;
				if (!TryGetBanExpiry(arg, 3, out expiry, out durationSuffix))
				{
					return;
				}
				string text2 = "";
				BasePlayer basePlayer = BasePlayer.FindByID(uInt);
				if (basePlayer != null && basePlayer.IsConnected)
				{
					text = basePlayer.displayName;
					if (basePlayer.IsConnected && basePlayer.net.connection.ownerid != 0L && basePlayer.net.connection.ownerid != basePlayer.net.connection.userid)
					{
						text2 += $" and also banned ownerid {basePlayer.net.connection.ownerid}";
						ServerUsers.Set(basePlayer.net.connection.ownerid, ServerUsers.UserGroup.Banned, basePlayer.displayName, arg.GetString(1, $"Family share owner of {basePlayer.net.connection.userid}"), expiry);
					}
					Chat.Broadcast("Kickbanning " + basePlayer.displayName + durationSuffix + " (" + @string + ")", "SERVER", "#eee", 0uL);
					Network.Net.sv.Kick(basePlayer.net.connection, "Banned" + durationSuffix + ": " + @string);
				}
				ServerUsers.Set(uInt, ServerUsers.UserGroup.Banned, text, @string, expiry);
				arg.ReplyWith($"Banned User{durationSuffix}: {uInt} - \"{text}\" for \"{@string}\"{text2}");
			}
		}

		private static bool TryGetBanExpiry(Arg arg, int n, out long expiry, out string durationSuffix)
		{
			expiry = arg.GetTimestamp(n, -1L);
			durationSuffix = null;
			int current = Epoch.Current;
			if (expiry > 0 && expiry <= current)
			{
				arg.ReplyWith("Expiry time is in the past");
				return false;
			}
			durationSuffix = ((expiry > 0) ? (" for " + (expiry - current).FormatSecondsLong()) : "");
			return true;
		}

		[ServerVar]
		public static void unban(Arg arg)
		{
			ulong uInt = arg.GetUInt64(0, 0uL);
			if (uInt < 70000000000000000L)
			{
				arg.ReplyWith($"This doesn't appear to be a 64bit steamid: {uInt}");
				return;
			}
			ServerUsers.User user = ServerUsers.Get(uInt);
			if (user == null || user.group != ServerUsers.UserGroup.Banned)
			{
				arg.ReplyWith($"User {uInt} isn't banned");
				return;
			}
			ServerUsers.Remove(uInt);
			arg.ReplyWith("Unbanned User: " + uInt);
		}

		[ServerVar]
		public static void skipqueue(Arg arg)
		{
			ulong uInt = arg.GetUInt64(0, 0uL);
			if (uInt < 70000000000000000L)
			{
				arg.ReplyWith("This doesn't appear to be a 64bit steamid: " + uInt);
			}
			else
			{
				SingletonComponent<ServerMgr>.Instance.connectionQueue.SkipQueue(uInt);
			}
		}

		[ServerVar(Help = "Print out currently connected clients etc")]
		public static void players(Arg arg)
		{
			TextTable textTable = new TextTable();
			textTable.AddColumn("id");
			textTable.AddColumn("name");
			textTable.AddColumn("ping");
			textTable.AddColumn("snap");
			textTable.AddColumn("updt");
			textTable.AddColumn("posi");
			textTable.AddColumn("dist");
			foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
			{
				string userIDString = activePlayer.UserIDString;
				string text = activePlayer.displayName.ToString();
				if (text.Length >= 14)
				{
					text = text.Substring(0, 14) + "..";
				}
				string text2 = text;
				string text3 = Network.Net.sv.GetAveragePing(activePlayer.net.connection).ToString();
				string text4 = activePlayer.GetQueuedUpdateCount(BasePlayer.NetworkQueue.Update).ToString();
				string text5 = activePlayer.GetQueuedUpdateCount(BasePlayer.NetworkQueue.UpdateDistance).ToString();
				textTable.AddRow(userIDString, text2, text3, string.Empty, text4, string.Empty, text5);
			}
			arg.ReplyWith(textTable.ToString());
		}

		[ServerVar(Help = "Sends a message in chat")]
		public static void say(Arg arg)
		{
			Chat.Broadcast(arg.FullString, "SERVER", "#eee", 0uL);
		}

		[ServerVar(Help = "Show user info for players on server.")]
		public static void users(Arg arg)
		{
			string text = "<slot:userid:\"name\">\n";
			int num = 0;
			foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
			{
				text = text + activePlayer.userID + ":\"" + activePlayer.displayName + "\"\n";
				num++;
			}
			text = text + num + "users\n";
			arg.ReplyWith(text);
		}

		[ServerVar(Help = "Show user info for players on server.")]
		public static void sleepingusers(Arg arg)
		{
			string text = "<slot:userid:\"name\">\n";
			int num = 0;
			foreach (BasePlayer sleepingPlayer in BasePlayer.sleepingPlayerList)
			{
				text += $"{sleepingPlayer.userID}:{sleepingPlayer.displayName}\n";
				num++;
			}
			text += $"{num} sleeping users\n";
			arg.ReplyWith(text);
		}

		[ServerVar(Help = "Show user info for sleeping players on server in range of the player.")]
		public static void sleepingusersinrange(Arg arg)
		{
			BasePlayer fromPlayer = ArgEx.Player(arg);
			if (fromPlayer == null)
			{
				return;
			}
			float range = arg.GetFloat(0);
			string text = "<slot:userid:\"name\">\n";
			int num = 0;
			List<BasePlayer> obj = Facepunch.Pool.GetList<BasePlayer>();
			foreach (BasePlayer sleepingPlayer in BasePlayer.sleepingPlayerList)
			{
				obj.Add(sleepingPlayer);
			}
			obj.RemoveAll((BasePlayer p) => p.Distance2D(fromPlayer) > range);
			obj.Sort((BasePlayer player, BasePlayer basePlayer) => (!(player.Distance2D(fromPlayer) < basePlayer.Distance2D(fromPlayer))) ? 1 : (-1));
			foreach (BasePlayer item in obj)
			{
				text += $"{item.userID}:{item.displayName}:{item.Distance2D(fromPlayer)}m\n";
				num++;
			}
			Facepunch.Pool.FreeList(ref obj);
			text += $"{num} sleeping users within {range}m\n";
			arg.ReplyWith(text);
		}

		[ServerVar(Help = "Show user info for players on server in range of the player.")]
		public static void usersinrange(Arg arg)
		{
			BasePlayer fromPlayer = ArgEx.Player(arg);
			if (fromPlayer == null)
			{
				return;
			}
			float range = arg.GetFloat(0);
			string text = "<slot:userid:\"name\">\n";
			int num = 0;
			List<BasePlayer> obj = Facepunch.Pool.GetList<BasePlayer>();
			foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
			{
				obj.Add(activePlayer);
			}
			obj.RemoveAll((BasePlayer p) => p.Distance2D(fromPlayer) > range);
			obj.Sort((BasePlayer player, BasePlayer basePlayer) => (!(player.Distance2D(fromPlayer) < basePlayer.Distance2D(fromPlayer))) ? 1 : (-1));
			foreach (BasePlayer item in obj)
			{
				text += $"{item.userID}:{item.displayName}:{item.Distance2D(fromPlayer)}m\n";
				num++;
			}
			Facepunch.Pool.FreeList(ref obj);
			text += $"{num} users within {range}m\n";
			arg.ReplyWith(text);
		}

		[ServerVar(Help = "List of banned users (sourceds compat)")]
		public static void banlist(Arg arg)
		{
			arg.ReplyWith(ServerUsers.BanListString());
		}

		[ServerVar(Help = "List of banned users - shows reasons and usernames")]
		public static void banlistex(Arg arg)
		{
			arg.ReplyWith(ServerUsers.BanListStringEx());
		}

		[ServerVar(Help = "List of banned users, by ID (sourceds compat)")]
		public static void listid(Arg arg)
		{
			arg.ReplyWith(ServerUsers.BanListString(true));
		}

		[ServerVar]
		public static void mute(Arg arg)
		{
			BasePlayer playerOrSleeper = ArgEx.GetPlayerOrSleeper(arg, 0);
			if (!playerOrSleeper || playerOrSleeper.net == null || playerOrSleeper.net.connection == null)
			{
				arg.ReplyWith("Player not found");
			}
			else
			{
				playerOrSleeper.SetPlayerFlag(BasePlayer.PlayerFlags.ChatMute, true);
			}
		}

		[ServerVar]
		public static void unmute(Arg arg)
		{
			BasePlayer playerOrSleeper = ArgEx.GetPlayerOrSleeper(arg, 0);
			if (!playerOrSleeper || playerOrSleeper.net == null || playerOrSleeper.net.connection == null)
			{
				arg.ReplyWith("Player not found");
			}
			else
			{
				playerOrSleeper.SetPlayerFlag(BasePlayer.PlayerFlags.ChatMute, false);
			}
		}

		[ServerVar(Help = "Print a list of currently muted players")]
		public static void mutelist(Arg arg)
		{
			var obj = from x in BasePlayer.allPlayerList
				where x.HasPlayerFlag(BasePlayer.PlayerFlags.ChatMute)
				select new
				{
					SteamId = x.UserIDString,
					Name = x.displayName
				};
			arg.ReplyWith(obj);
		}

		[ServerVar]
		public static void clientperf(Arg arg)
		{
			foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
			{
				activePlayer.ClientRPCPlayer(null, activePlayer, "GetPerformanceReport");
			}
		}

		[ServerVar(Help = "Get information about all the cars in the world")]
		public static void carstats(Arg arg)
		{
			HashSet<ModularCar> allCarsList = ModularCar.allCarsList;
			TextTable textTable = new TextTable();
			textTable.AddColumn("id");
			textTable.AddColumn("sockets");
			textTable.AddColumn("modules");
			textTable.AddColumn("complete");
			textTable.AddColumn("engine");
			textTable.AddColumn("health");
			textTable.AddColumn("location");
			int count = allCarsList.Count;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (ModularCar item in allCarsList)
			{
				string text = item.net.ID.ToString();
				string text2 = item.TotalSockets.ToString();
				string text3 = item.NumAttachedModules.ToString();
				string text4;
				if (item.IsComplete())
				{
					text4 = "Complete";
					num++;
				}
				else
				{
					text4 = "Partial";
				}
				string text5;
				if (item.HasAnyWorkingEngines())
				{
					text5 = "Working";
					num2++;
				}
				else
				{
					text5 = "Broken";
				}
				string text6 = ((item.TotalMaxHealth() != 0f) ? $"{item.TotalHealth() / item.TotalMaxHealth():0%}" : "0");
				string text7;
				if (item.IsOutside())
				{
					text7 = "Outside";
				}
				else
				{
					text7 = "Inside";
					num3++;
				}
				textTable.AddRow(text, text2, text3, text4, text5, text6, text7);
			}
			string text8 = "";
			text8 = ((count != 1) ? (text8 + $"\nThe world contains {count} modular cars.") : (text8 + "\nThe world contains 1 modular car."));
			text8 = ((num != 1) ? (text8 + $"\n{num} ({(float)num / (float)count:0%}) are in a completed state.") : (text8 + $"\n1 ({1f / (float)count:0%}) is in a completed state."));
			text8 = ((num2 != 1) ? (text8 + $"\n{num2} ({(float)num2 / (float)count:0%}) are driveable.") : (text8 + $"\n1 ({1f / (float)count:0%}) is driveable."));
			arg.ReplyWith(string.Concat(str1: (num3 != 1) ? (text8 + $"\n{num3} ({(float)num3 / (float)count:0%}) are sheltered indoors.") : (text8 + $"\n1 ({1f / (float)count:0%}) is sheltered indoors."), str0: textTable.ToString()));
		}

		[ServerVar]
		public static string teaminfo(Arg arg)
		{
			ulong num = arg.GetUInt64(0, 0uL);
			if (num == 0L)
			{
				BasePlayer player = ArgEx.GetPlayer(arg, 0);
				if (player == null)
				{
					return "Player not found";
				}
				num = player.userID;
			}
			RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindPlayersTeam(num);
			if (playerTeam == null)
			{
				return "Player is not in a team";
			}
			TextTable textTable = new TextTable();
			textTable.AddColumn("steamID");
			textTable.AddColumn("username");
			textTable.AddColumn("online");
			textTable.AddColumn("leader");
			foreach (ulong memberId in playerTeam.members)
			{
				bool flag = Network.Net.sv.connections.FirstOrDefault((Connection c) => c.connected && c.userid == memberId) != null;
				textTable.AddRow(memberId.ToString(), GetPlayerName(memberId), flag ? "x" : "", (memberId == playerTeam.teamLeader) ? "x" : "");
			}
			return textTable.ToString();
		}

		[ServerVar]
		public static void entid(Arg arg)
		{
			BaseEntity baseEntity = BaseNetworkable.serverEntities.Find(arg.GetUInt(1)) as BaseEntity;
			if (!(baseEntity == null) && !(baseEntity is BasePlayer))
			{
				string @string = arg.GetString(0);
				if (ArgEx.Player(arg) != null)
				{
					Debug.Log("[ENTCMD] " + ArgEx.Player(arg).displayName + "/" + ArgEx.Player(arg).userID + " used *" + @string + "* on ent: " + baseEntity.name);
				}
				switch (@string)
				{
				case "kill":
					baseEntity.AdminKill();
					break;
				case "lock":
					baseEntity.SetFlag(BaseEntity.Flags.Locked, true);
					break;
				case "unlock":
					baseEntity.SetFlag(BaseEntity.Flags.Locked, false);
					break;
				case "debug":
					baseEntity.SetFlag(BaseEntity.Flags.Debugging, true);
					break;
				case "undebug":
					baseEntity.SetFlag(BaseEntity.Flags.Debugging, false);
					break;
				case "who":
					arg.ReplyWith(baseEntity.Admin_Who());
					break;
				case "auth":
					arg.ReplyWith(AuthList(baseEntity));
					break;
				default:
					arg.ReplyWith("Unknown command");
					break;
				}
			}
		}

		private static string AuthList(BaseEntity ent)
		{
			if ((object)ent != null)
			{
				BuildingPrivlidge buildingPrivlidge;
				List<PlayerNameID> authorizedPlayers;
				if ((object)(buildingPrivlidge = ent as BuildingPrivlidge) == null)
				{
					AutoTurret autoTurret;
					if ((object)(autoTurret = ent as AutoTurret) == null)
					{
						CodeLock codeLock;
						if ((object)(codeLock = ent as CodeLock) != null)
						{
							return CodeLockAuthList(codeLock);
						}
						goto IL_0042;
					}
					authorizedPlayers = autoTurret.authorizedPlayers;
				}
				else
				{
					authorizedPlayers = buildingPrivlidge.authorizedPlayers;
				}
				if (authorizedPlayers == null || authorizedPlayers.Count == 0)
				{
					return "Nobody is authed to this entity";
				}
				TextTable textTable = new TextTable();
				textTable.AddColumn("steamID");
				textTable.AddColumn("username");
				foreach (PlayerNameID item in authorizedPlayers)
				{
					textTable.AddRow(item.userid.ToString(), GetPlayerName(item.userid));
				}
				return textTable.ToString();
			}
			goto IL_0042;
			IL_0042:
			return "Entity has no auth list";
		}

		private static string CodeLockAuthList(CodeLock codeLock)
		{
			if (codeLock.whitelistPlayers.Count == 0 && codeLock.guestPlayers.Count == 0)
			{
				return "Nobody is authed to this entity";
			}
			TextTable textTable = new TextTable();
			textTable.AddColumn("steamID");
			textTable.AddColumn("username");
			textTable.AddColumn("isGuest");
			foreach (ulong whitelistPlayer in codeLock.whitelistPlayers)
			{
				textTable.AddRow(whitelistPlayer.ToString(), GetPlayerName(whitelistPlayer), "");
			}
			foreach (ulong guestPlayer in codeLock.guestPlayers)
			{
				textTable.AddRow(guestPlayer.ToString(), GetPlayerName(guestPlayer), "x");
			}
			return textTable.ToString();
		}

		public static string GetPlayerName(ulong steamId)
		{
			BasePlayer basePlayer = BasePlayer.allPlayerList.FirstOrDefault((BasePlayer p) => p.userID == steamId);
			string text;
			if (!(basePlayer != null))
			{
				text = SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerName(steamId);
				if (text == null)
				{
					return "[unknown]";
				}
			}
			else
			{
				text = basePlayer.displayName;
			}
			return text;
		}

		[ServerVar(Help = "Get a list of players")]
		public static PlayerInfo[] playerlist()
		{
			return BasePlayer.activePlayerList.Select(delegate(BasePlayer x)
			{
				PlayerInfo result = default(PlayerInfo);
				result.SteamID = x.UserIDString;
				result.OwnerSteamID = x.OwnerID.ToString();
				result.DisplayName = x.displayName;
				result.Ping = Network.Net.sv.GetAveragePing(x.net.connection);
				result.Address = x.net.connection.ipaddress;
				result.ConnectedSeconds = (int)x.net.connection.GetSecondsConnected();
				result.VoiationLevel = x.violationLevel;
				result.Health = x.Health();
				return result;
			}).ToArray();
		}

		[ServerVar(Help = "List of banned users")]
		public static ServerUsers.User[] Bans()
		{
			return ServerUsers.GetAll(ServerUsers.UserGroup.Banned).ToArray();
		}

		[ServerVar(Help = "Get a list of information about the server")]
		public static ServerInfoOutput ServerInfo()
		{
			ServerInfoOutput result = default(ServerInfoOutput);
			result.Hostname = Server.hostname;
			result.MaxPlayers = Server.maxplayers;
			result.Players = BasePlayer.activePlayerList.Count;
			result.Queued = SingletonComponent<ServerMgr>.Instance.connectionQueue.Queued;
			result.Joining = SingletonComponent<ServerMgr>.Instance.connectionQueue.Joining;
			result.EntityCount = BaseNetworkable.serverEntities.Count;
			result.GameTime = ((TOD_Sky.Instance != null) ? TOD_Sky.Instance.Cycle.DateTime.ToString() : DateTime.UtcNow.ToString());
			result.Uptime = (int)UnityEngine.Time.realtimeSinceStartup;
			result.Map = Server.level;
			result.Framerate = Performance.report.frameRate;
			result.Memory = (int)Performance.report.memoryAllocations;
			result.Collections = (int)Performance.report.memoryCollections;
			result.NetworkIn = (int)((Network.Net.sv != null) ? Network.Net.sv.GetStat(null, BaseNetwork.StatTypeLong.BytesReceived_LastSecond) : 0);
			result.NetworkOut = (int)((Network.Net.sv != null) ? Network.Net.sv.GetStat(null, BaseNetwork.StatTypeLong.BytesSent_LastSecond) : 0);
			result.Restarting = SingletonComponent<ServerMgr>.Instance.Restarting;
			result.SaveCreatedTime = SaveRestore.SaveCreatedTime.ToString();
			return result;
		}

		[ServerVar(Help = "Get information about this build")]
		public static BuildInfo BuildInfo()
		{
			return Facepunch.BuildInfo.Current;
		}

		[ServerVar]
		public static void AdminUI_RequestPlayerList(Arg arg)
		{
			if (allowAdminUI)
			{
				ConsoleNetwork.SendClientCommand(arg.Connection, "AdminUI_ReceivePlayerList", JsonConvert.SerializeObject(playerlist()));
			}
		}

		[ServerVar]
		public static void AdminUI_RequestServerInfo(Arg arg)
		{
			if (allowAdminUI)
			{
				ConsoleNetwork.SendClientCommand(arg.Connection, "AdminUI_ReceiveServerInfo", JsonConvert.SerializeObject(ServerInfo()));
			}
		}

		[ServerVar]
		public static void AdminUI_RequestServerConvars(Arg arg)
		{
			if (!allowAdminUI)
			{
				return;
			}
			List<ServerConvarInfo> obj = Facepunch.Pool.GetList<ServerConvarInfo>();
			Command[] all = Index.All;
			foreach (Command command in all)
			{
				if (command.Server && command.Variable && command.ServerAdmin && command.ShowInAdminUI)
				{
					obj.Add(new ServerConvarInfo
					{
						FullName = command.FullName,
						Value = command.GetOveride?.Invoke(),
						Help = command.Description
					});
				}
			}
			ConsoleNetwork.SendClientCommand(arg.Connection, "AdminUI_ReceiveCommands", JsonConvert.SerializeObject(obj));
			Facepunch.Pool.FreeList(ref obj);
		}
	}
}
