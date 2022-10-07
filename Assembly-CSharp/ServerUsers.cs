using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ConVar;
using Facepunch.Extend;
using Facepunch.Math;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Oxide.Core;
using UnityEngine;

public static class ServerUsers
{
	public enum UserGroup
	{
		None = 0,
		Owner = 1,
		Moderator = 2,
		Banned = 3,
		SkipQueue = 4
	}

	public class User
	{
		public ulong steamid;

		[JsonConverter(typeof(StringEnumConverter))]
		public UserGroup group;

		public string username;

		public string notes;

		public long expiry;

		[JsonIgnore]
		public bool IsExpired
		{
			get
			{
				if (expiry > 0)
				{
					return Epoch.Current > expiry;
				}
				return false;
			}
		}
	}

	public static Dictionary<ulong, User> users = new Dictionary<ulong, User>();

	public static void Remove(ulong uid)
	{
		Interface.CallHook("OnServerUserRemove", uid);
		users.Remove(uid);
	}

	public static void Set(ulong uid, UserGroup group, string username, string notes, long expiry = -1L)
	{
		Remove(uid);
		User value = new User
		{
			steamid = uid,
			group = group,
			username = username,
			notes = notes,
			expiry = expiry
		};
		Interface.CallHook("OnServerUserSet", uid, group, username, notes, expiry);
		users.Add(uid, value);
	}

	public static User Get(ulong uid)
	{
		if (!users.TryGetValue(uid, out var value))
		{
			return null;
		}
		if (!value.IsExpired)
		{
			return value;
		}
		Remove(uid);
		return null;
	}

	public static bool Is(ulong uid, UserGroup group)
	{
		User user = Get(uid);
		if (user == null)
		{
			return false;
		}
		return user.group == group;
	}

	public static IEnumerable<User> GetAll(UserGroup group)
	{
		return from x in users.Values
			where x.@group == @group
			where !x.IsExpired
			select x;
	}

	public static void Clear()
	{
		users.Clear();
	}

	public static void Load()
	{
		Clear();
		string serverFolder = Server.GetServerFolder("cfg");
		if (File.Exists(serverFolder + "/bans.cfg"))
		{
			string text = File.ReadAllText(serverFolder + "/bans.cfg");
			if (!string.IsNullOrEmpty(text))
			{
				Debug.Log("Running " + serverFolder + "/bans.cfg");
				ConsoleSystem.RunFile(ConsoleSystem.Option.Server.Quiet(), text);
			}
		}
		if (File.Exists(serverFolder + "/users.cfg"))
		{
			string text2 = File.ReadAllText(serverFolder + "/users.cfg");
			if (!string.IsNullOrEmpty(text2))
			{
				Debug.Log("Running " + serverFolder + "/users.cfg");
				ConsoleSystem.RunFile(ConsoleSystem.Option.Server.Quiet(), text2);
			}
		}
	}

	public static void Save()
	{
		foreach (ulong item in (from kv in users
			where kv.Value.IsExpired
			select kv.Key).ToList())
		{
			Remove(item);
		}
		string serverFolder = Server.GetServerFolder("cfg");
		StringBuilder stringBuilder = new StringBuilder(67108864);
		stringBuilder.Clear();
		foreach (User item2 in GetAll(UserGroup.Banned))
		{
			if (!(item2.notes == "EAC"))
			{
				stringBuilder.Append("banid ");
				stringBuilder.Append(item2.steamid);
				stringBuilder.Append(' ');
				stringBuilder.Append(Facepunch.Extend.StringExtensions.QuoteSafe(item2.username));
				stringBuilder.Append(' ');
				stringBuilder.Append(Facepunch.Extend.StringExtensions.QuoteSafe(item2.notes));
				stringBuilder.Append(' ');
				stringBuilder.Append(item2.expiry);
				stringBuilder.Append("\r\n");
			}
		}
		File.WriteAllText(serverFolder + "/bans.cfg", stringBuilder.ToString());
		stringBuilder.Clear();
		foreach (User item3 in GetAll(UserGroup.Owner))
		{
			stringBuilder.Append("ownerid ");
			stringBuilder.Append(item3.steamid);
			stringBuilder.Append(' ');
			stringBuilder.Append(Facepunch.Extend.StringExtensions.QuoteSafe(item3.username));
			stringBuilder.Append(' ');
			stringBuilder.Append(Facepunch.Extend.StringExtensions.QuoteSafe(item3.notes));
			stringBuilder.Append("\r\n");
		}
		foreach (User item4 in GetAll(UserGroup.Moderator))
		{
			stringBuilder.Append("moderatorid ");
			stringBuilder.Append(item4.steamid);
			stringBuilder.Append(' ');
			stringBuilder.Append(Facepunch.Extend.StringExtensions.QuoteSafe(item4.username));
			stringBuilder.Append(' ');
			stringBuilder.Append(Facepunch.Extend.StringExtensions.QuoteSafe(item4.notes));
			stringBuilder.Append("\r\n");
		}
		foreach (User item5 in GetAll(UserGroup.SkipQueue))
		{
			stringBuilder.Append("skipqueueid ");
			stringBuilder.Append(item5.steamid);
			stringBuilder.Append(' ');
			stringBuilder.Append(Facepunch.Extend.StringExtensions.QuoteSafe(item5.username));
			stringBuilder.Append(' ');
			stringBuilder.Append(Facepunch.Extend.StringExtensions.QuoteSafe(item5.notes));
			stringBuilder.Append("\r\n");
		}
		File.WriteAllText(serverFolder + "/users.cfg", stringBuilder.ToString());
	}

	public static string BanListString(bool bHeader = false)
	{
		List<User> list = GetAll(UserGroup.Banned).ToList();
		StringBuilder stringBuilder = new StringBuilder(67108864);
		if (bHeader)
		{
			if (list.Count == 0)
			{
				return "ID filter list: empty\n";
			}
			if (list.Count == 1)
			{
				stringBuilder.Append("ID filter list: 1 entry\n");
			}
			else
			{
				stringBuilder.Append($"ID filter list: {list.Count} entries\n");
			}
		}
		int num = 1;
		foreach (User item in list)
		{
			stringBuilder.Append(num);
			stringBuilder.Append(' ');
			stringBuilder.Append(item.steamid);
			stringBuilder.Append(" : ");
			if (item.expiry > 0)
			{
				stringBuilder.Append(((double)(item.expiry - Epoch.Current) / 60.0).ToString("F3"));
				stringBuilder.Append(" min");
			}
			else
			{
				stringBuilder.Append("permanent");
			}
			stringBuilder.Append('\n');
			num++;
		}
		return stringBuilder.ToString();
	}

	public static string BanListStringEx()
	{
		IEnumerable<User> all = GetAll(UserGroup.Banned);
		StringBuilder stringBuilder = new StringBuilder(67108864);
		int num = 1;
		foreach (User item in all)
		{
			stringBuilder.Append(num);
			stringBuilder.Append(' ');
			stringBuilder.Append(item.steamid);
			stringBuilder.Append(' ');
			stringBuilder.Append(Facepunch.Extend.StringExtensions.QuoteSafe(item.username));
			stringBuilder.Append(' ');
			stringBuilder.Append(Facepunch.Extend.StringExtensions.QuoteSafe(item.notes));
			stringBuilder.Append(' ');
			stringBuilder.Append(item.expiry);
			stringBuilder.Append('\n');
			num++;
		}
		return stringBuilder.ToString();
	}
}
