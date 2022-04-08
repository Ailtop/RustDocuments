using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Facepunch;
using Facepunch.Extend;
using UnityEngine;

namespace ConVar;

[Factory("pool")]
public class Pool : ConsoleSystem
{
	[ServerVar]
	[ClientVar]
	public static int mode = 2;

	[ServerVar]
	[ClientVar]
	public static bool prewarm = true;

	[ServerVar]
	[ClientVar]
	public static bool enabled = true;

	[ServerVar]
	[ClientVar]
	public static bool debug = false;

	[ServerVar]
	[ClientVar]
	public static void print_memory(Arg arg)
	{
		if (Facepunch.Pool.directory.Count == 0)
		{
			arg.ReplyWith("Memory pool is empty.");
			return;
		}
		TextTable textTable = new TextTable();
		textTable.AddColumn("type");
		textTable.AddColumn("pooled");
		textTable.AddColumn("active");
		textTable.AddColumn("hits");
		textTable.AddColumn("misses");
		textTable.AddColumn("spills");
		foreach (KeyValuePair<Type, Facepunch.Pool.ICollection> item in Facepunch.Pool.directory.OrderByDescending((KeyValuePair<Type, Facepunch.Pool.ICollection> x) => x.Value.ItemsCreated))
		{
			string text = item.Key.ToString().Replace("System.Collections.Generic.", "");
			Facepunch.Pool.ICollection value = item.Value;
			textTable.AddRow(text, value.ItemsInStack.FormatNumberShort(), value.ItemsInUse.FormatNumberShort(), value.ItemsTaken.FormatNumberShort(), value.ItemsCreated.FormatNumberShort(), value.ItemsSpilled.FormatNumberShort());
		}
		arg.ReplyWith(textTable.ToString());
	}

	[ServerVar]
	[ClientVar]
	public static void print_prefabs(Arg arg)
	{
		PrefabPoolCollection pool = GameManager.server.pool;
		if (pool.storage.Count == 0)
		{
			arg.ReplyWith("Prefab pool is empty.");
			return;
		}
		string @string = arg.GetString(0, string.Empty);
		TextTable textTable = new TextTable();
		textTable.AddColumn("id");
		textTable.AddColumn("name");
		textTable.AddColumn("count");
		foreach (KeyValuePair<uint, PrefabPool> item in pool.storage)
		{
			string text = item.Key.ToString();
			string text2 = StringPool.Get(item.Key);
			string text3 = item.Value.Count.ToString();
			if (string.IsNullOrEmpty(@string) || text2.Contains(@string, CompareOptions.IgnoreCase))
			{
				textTable.AddRow(text, Path.GetFileNameWithoutExtension(text2), text3);
			}
		}
		arg.ReplyWith(textTable.ToString());
	}

	[ServerVar]
	[ClientVar]
	public static void print_assets(Arg arg)
	{
		if (AssetPool.storage.Count == 0)
		{
			arg.ReplyWith("Asset pool is empty.");
			return;
		}
		string @string = arg.GetString(0, string.Empty);
		TextTable textTable = new TextTable();
		textTable.AddColumn("type");
		textTable.AddColumn("allocated");
		textTable.AddColumn("available");
		foreach (KeyValuePair<Type, AssetPool.Pool> item in AssetPool.storage)
		{
			string text = item.Key.ToString();
			string text2 = item.Value.allocated.ToString();
			string text3 = item.Value.available.ToString();
			if (string.IsNullOrEmpty(@string) || text.Contains(@string, CompareOptions.IgnoreCase))
			{
				textTable.AddRow(text, text2, text3);
			}
		}
		arg.ReplyWith(textTable.ToString());
	}

	[ServerVar]
	[ClientVar]
	public static void clear_memory(Arg arg)
	{
		Facepunch.Pool.Clear();
	}

	[ServerVar]
	[ClientVar]
	public static void clear_prefabs(Arg arg)
	{
		GameManager.server.pool.Clear();
	}

	[ServerVar]
	[ClientVar]
	public static void clear_assets(Arg arg)
	{
		AssetPool.Clear();
	}

	[ServerVar]
	[ClientVar]
	public static void export_prefabs(Arg arg)
	{
		PrefabPoolCollection pool = GameManager.server.pool;
		if (pool.storage.Count == 0)
		{
			arg.ReplyWith("Prefab pool is empty.");
			return;
		}
		string @string = arg.GetString(0, string.Empty);
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<uint, PrefabPool> item in pool.storage)
		{
			string arg2 = item.Key.ToString();
			string text = StringPool.Get(item.Key);
			string arg3 = item.Value.Count.ToString();
			if (string.IsNullOrEmpty(@string) || text.Contains(@string, CompareOptions.IgnoreCase))
			{
				stringBuilder.AppendLine($"{arg2},{Path.GetFileNameWithoutExtension(text)},{arg3}");
			}
		}
		File.WriteAllText("prefabs.csv", stringBuilder.ToString());
	}
}
