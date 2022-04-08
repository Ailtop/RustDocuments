using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Facepunch;
using UnityEngine;

namespace ConVar;

[Factory("entity")]
public class Entity : ConsoleSystem
{
	private struct EntityInfo
	{
		public BaseNetworkable entity;

		public uint entityID;

		public uint groupID;

		public uint parentID;

		public string status;

		public EntityInfo(BaseNetworkable src)
		{
			entity = src;
			BaseEntity baseEntity = entity as BaseEntity;
			BaseEntity baseEntity2 = ((baseEntity != null) ? baseEntity.GetParentEntity() : null);
			entityID = ((entity != null && entity.net != null) ? entity.net.ID : 0u);
			groupID = ((entity != null && entity.net != null && entity.net.group != null) ? entity.net.group.ID : 0u);
			parentID = ((baseEntity != null) ? baseEntity.parentEntity.uid : 0u);
			if (baseEntity != null && baseEntity.parentEntity.uid != 0)
			{
				if (baseEntity2 == null)
				{
					status = "orphan";
				}
				else
				{
					status = "child";
				}
			}
			else
			{
				status = string.Empty;
			}
		}
	}

	private static TextTable GetEntityTable(Func<EntityInfo, bool> filter)
	{
		TextTable textTable = new TextTable();
		textTable.AddColumn("realm");
		textTable.AddColumn("entity");
		textTable.AddColumn("group");
		textTable.AddColumn("parent");
		textTable.AddColumn("name");
		textTable.AddColumn("position");
		textTable.AddColumn("local");
		textTable.AddColumn("rotation");
		textTable.AddColumn("local");
		textTable.AddColumn("status");
		textTable.AddColumn("invokes");
		foreach (BaseNetworkable serverEntity in BaseNetworkable.serverEntities)
		{
			if (!(serverEntity == null))
			{
				EntityInfo arg = new EntityInfo(serverEntity);
				if (filter(arg))
				{
					textTable.AddRow("sv", arg.entityID.ToString(), arg.groupID.ToString(), arg.parentID.ToString(), arg.entity.ShortPrefabName, arg.entity.transform.position.ToString(), arg.entity.transform.localPosition.ToString(), arg.entity.transform.rotation.eulerAngles.ToString(), arg.entity.transform.localRotation.eulerAngles.ToString(), arg.status, arg.entity.InvokeString());
				}
			}
		}
		return textTable;
	}

	[ServerVar]
	[ClientVar]
	public static void find_entity(Arg args)
	{
		string filter = args.GetString(0);
		TextTable entityTable = GetEntityTable((EntityInfo info) => string.IsNullOrEmpty(filter) || info.entity.PrefabName.Contains(filter));
		args.ReplyWith(entityTable.ToString());
	}

	[ServerVar]
	[ClientVar]
	public static void find_id(Arg args)
	{
		uint filter = args.GetUInt(0);
		TextTable entityTable = GetEntityTable((EntityInfo info) => info.entityID == filter);
		args.ReplyWith(entityTable.ToString());
	}

	[ServerVar]
	[ClientVar]
	public static void find_group(Arg args)
	{
		uint filter = args.GetUInt(0);
		TextTable entityTable = GetEntityTable((EntityInfo info) => info.groupID == filter);
		args.ReplyWith(entityTable.ToString());
	}

	[ServerVar]
	[ClientVar]
	public static void find_parent(Arg args)
	{
		uint filter = args.GetUInt(0);
		TextTable entityTable = GetEntityTable((EntityInfo info) => info.parentID == filter);
		args.ReplyWith(entityTable.ToString());
	}

	[ServerVar]
	[ClientVar]
	public static void find_status(Arg args)
	{
		string filter = args.GetString(0);
		TextTable entityTable = GetEntityTable((EntityInfo info) => string.IsNullOrEmpty(filter) || info.status.Contains(filter));
		args.ReplyWith(entityTable.ToString());
	}

	[ServerVar]
	[ClientVar]
	public static void find_radius(Arg args)
	{
		BasePlayer player = ArgEx.Player(args);
		if (!(player == null))
		{
			uint filter = args.GetUInt(0, 10u);
			TextTable entityTable = GetEntityTable((EntityInfo info) => Vector3.Distance(info.entity.transform.position, player.transform.position) <= (float)filter);
			args.ReplyWith(entityTable.ToString());
		}
	}

	[ServerVar]
	[ClientVar]
	public static void find_self(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (!(basePlayer == null) && basePlayer.net != null)
		{
			uint filter = basePlayer.net.ID;
			TextTable entityTable = GetEntityTable((EntityInfo info) => info.entityID == filter);
			args.ReplyWith(entityTable.ToString());
		}
	}

	[ServerVar]
	public static void debug_toggle(Arg args)
	{
		int @int = args.GetInt(0);
		if (@int == 0)
		{
			return;
		}
		BaseEntity baseEntity = BaseNetworkable.serverEntities.Find((uint)@int) as BaseEntity;
		if (!(baseEntity == null))
		{
			baseEntity.SetFlag(BaseEntity.Flags.Debugging, !baseEntity.IsDebugging());
			if (baseEntity.IsDebugging())
			{
				baseEntity.OnDebugStart();
			}
			args.ReplyWith("Debugging for " + baseEntity.net.ID + " " + (baseEntity.IsDebugging() ? "enabled" : "disabled"));
		}
	}

	[ServerVar]
	public static void nudge(int entID)
	{
		if (entID != 0)
		{
			BaseEntity baseEntity = BaseNetworkable.serverEntities.Find((uint)entID) as BaseEntity;
			if (!(baseEntity == null))
			{
				baseEntity.BroadcastMessage("DebugNudge", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	[ServerVar(Name = "spawn")]
	public static string svspawn(string name, Vector3 pos, Vector3 dir)
	{
		BasePlayer arg = ArgEx.Player(ConsoleSystem.CurrentArgs);
		if (string.IsNullOrEmpty(name))
		{
			return "No entity name provided";
		}
		string[] array = (from x in GameManifest.Current.entities
			where Path.GetFileNameWithoutExtension(x).Contains(name, CompareOptions.IgnoreCase)
			select x.ToLower()).ToArray();
		if (array.Length == 0)
		{
			return "Entity type not found";
		}
		if (array.Length > 1)
		{
			string text = array.FirstOrDefault((string x) => string.Compare(Path.GetFileNameWithoutExtension(x), name, StringComparison.OrdinalIgnoreCase) == 0);
			if (text == null)
			{
				Debug.Log($"{arg} failed to spawn \"{name}\"");
				return "Unknown entity - could be:\n\n" + string.Join("\n", array.Select(Path.GetFileNameWithoutExtension).ToArray());
			}
			array[0] = text;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(array[0], pos, Quaternion.LookRotation(dir, Vector3.up));
		if (baseEntity == null)
		{
			Debug.Log($"{arg} failed to spawn \"{array[0]}\" (tried to spawn \"{name}\")");
			return "Couldn't spawn " + name;
		}
		BasePlayer basePlayer = baseEntity as BasePlayer;
		if (basePlayer != null)
		{
			basePlayer.OverrideViewAngles(Quaternion.LookRotation(dir, Vector3.up).eulerAngles);
		}
		baseEntity.Spawn();
		Debug.Log($"{arg} spawned \"{baseEntity}\" at {pos}");
		return string.Concat("spawned ", baseEntity, " at ", pos);
	}

	[ServerVar(Name = "spawnitem")]
	public static string svspawnitem(string name, Vector3 pos)
	{
		BasePlayer arg = ArgEx.Player(ConsoleSystem.CurrentArgs);
		if (string.IsNullOrEmpty(name))
		{
			return "No entity name provided";
		}
		string[] array = (from x in ItemManager.itemList
			select x.shortname into x
			where x.Contains(name, CompareOptions.IgnoreCase)
			select x).ToArray();
		if (array.Length == 0)
		{
			return "Entity type not found";
		}
		if (array.Length > 1)
		{
			string text = array.FirstOrDefault((string x) => string.Compare(x, name, StringComparison.OrdinalIgnoreCase) == 0);
			if (text == null)
			{
				Debug.Log($"{arg} failed to spawn \"{name}\"");
				return "Unknown entity - could be:\n\n" + string.Join("\n", array);
			}
			array[0] = text;
		}
		Item item = ItemManager.CreateByName(array[0], 1, 0uL);
		if (item == null)
		{
			Debug.Log($"{arg} failed to spawn \"{array[0]}\" (tried to spawnitem \"{name}\")");
			return "Couldn't spawn " + name;
		}
		BaseEntity arg2 = item.CreateWorldObject(pos);
		Debug.Log($"{arg} spawned \"{arg2}\" at {pos} (via spawnitem)");
		return string.Concat("spawned ", item, " at ", pos);
	}

	[ServerVar]
	public static void spawnlootfrom(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		string @string = args.GetString(0, string.Empty);
		int @int = args.GetInt(1, 1);
		Vector3 vector = args.GetVector3(1, basePlayer ? basePlayer.CenterPoint() : Vector3.zero);
		if (string.IsNullOrEmpty(@string))
		{
			return;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(@string, vector);
		if (baseEntity == null)
		{
			return;
		}
		baseEntity.Spawn();
		basePlayer.ChatMessage("Contents of " + @string + " spawned " + @int + " times");
		LootContainer component = baseEntity.GetComponent<LootContainer>();
		if (component != null)
		{
			for (int i = 0; i < @int * component.maxDefinitionsToSpawn; i++)
			{
				component.lootDefinition.SpawnIntoContainer(basePlayer.inventory.containerMain);
			}
		}
		baseEntity.Kill();
	}

	public static int DeleteBy(ulong id)
	{
		List<ulong> obj = Facepunch.Pool.GetList<ulong>();
		obj.Add(id);
		int result = DeleteBy(obj);
		Facepunch.Pool.FreeList(ref obj);
		return result;
	}

	[ServerVar(Help = "Destroy all entities created by provided users (separate users by space)")]
	public static int DeleteBy(Arg arg)
	{
		if (!arg.HasArgs())
		{
			return 0;
		}
		List<ulong> obj = Facepunch.Pool.GetList<ulong>();
		string[] args = arg.Args;
		for (int i = 0; i < args.Length; i++)
		{
			if (ulong.TryParse(args[i], out var result))
			{
				obj.Add(result);
			}
		}
		int result2 = DeleteBy(obj);
		Facepunch.Pool.FreeList(ref obj);
		return result2;
	}

	private static int DeleteBy(List<ulong> ids)
	{
		int num = 0;
		foreach (BaseEntity serverEntity in BaseNetworkable.serverEntities)
		{
			if (serverEntity == null)
			{
				continue;
			}
			bool flag = false;
			foreach (ulong id in ids)
			{
				if (serverEntity.OwnerID == id)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				serverEntity.Invoke(serverEntity.KillMessage, (float)num * 0.2f);
				num++;
			}
		}
		return num;
	}

	[ServerVar(Help = "Destroy all entities created by users in the provided text block (can use with copied results from ent auth)")]
	public static void DeleteByTextBlock(Arg arg)
	{
		if (arg.Args.Length != 1)
		{
			arg.ReplyWith("Invalid arguments, provide a text block surrounded by \" and listing player id's at the start of each line");
			return;
		}
		MatchCollection matchCollection = Regex.Matches(arg.GetString(0), "^\\b\\d{17}", RegexOptions.Multiline);
		List<ulong> obj = Facepunch.Pool.GetList<ulong>();
		foreach (Match item in matchCollection)
		{
			if (ulong.TryParse(item.Value, out var result))
			{
				obj.Add(result);
			}
		}
		int num = DeleteBy(obj);
		Facepunch.Pool.FreeList(ref obj);
		arg.ReplyWith($"Destroyed {num} entities");
	}
}
