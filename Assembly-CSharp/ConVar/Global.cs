using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Facepunch;
using Facepunch.Extend;
using Network;
using Network.Visibility;
using Rust;
using UnityEngine;
using UnityEngine.Profiling;

namespace ConVar;

[Factory("global")]
public class Global : ConsoleSystem
{
	private static int _developer;

	[ServerVar]
	[ClientVar]
	public static bool skipassetwarmup = false;

	[ServerVar]
	[ClientVar]
	public static int maxthreads = 8;

	[ServerVar(Saved = true)]
	[ClientVar(Saved = true)]
	public static int perf = 0;

	[ClientVar(ClientInfo = true, Saved = true, Help = "If you're an admin this will enable god mode")]
	public static bool god = false;

	[ClientVar(ClientInfo = true, Saved = true, Help = "If enabled you will be networked when you're spectating. This means that you will hear audio chat, but also means that cheaters will potentially be able to detect you watching them.")]
	public static bool specnet = false;

	[ServerVar(Saved = true, ShowInAdminUI = true, Help = "Multiplier applied to SprayDuration if a spray isn't in the sprayers auth (cannot go above 1f)")]
	public static float SprayOutOfAuthMultiplier = 0.5f;

	[ServerVar(Saved = true, ShowInAdminUI = true, Help = "Base time (in seconds) that sprays last")]
	public static float SprayDuration = 10800f;

	[ServerVar(Saved = true, ShowInAdminUI = true, Help = "If a player sprays more than this, the oldest spray will be destroyed. 0 will disable")]
	public static int MaxSpraysPerPlayer = 25;

	[ServerVar]
	[ClientVar]
	public static int developer
	{
		get
		{
			return _developer;
		}
		set
		{
			_developer = value;
		}
	}

	[ServerVar]
	public static void restart(Arg args)
	{
		ServerMgr.RestartServer(args.GetString(1, string.Empty), args.GetInt(0, 300));
	}

	[ClientVar]
	[ServerVar]
	public static void quit(Arg args)
	{
		SingletonComponent<ServerMgr>.Instance.Shutdown();
		Rust.Application.isQuitting = true;
		Network.Net.sv.Stop("quit");
		Process.GetCurrentProcess().Kill();
		UnityEngine.Debug.Log("Quitting");
		Rust.Application.Quit();
	}

	[ServerVar]
	public static void report(Arg args)
	{
		ServerPerformance.DoReport();
	}

	[ServerVar]
	[ClientVar]
	public static void objects(Arg args)
	{
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType<UnityEngine.Object>();
		string text = "";
		Dictionary<Type, int> dictionary = new Dictionary<Type, int>();
		Dictionary<Type, long> dictionary2 = new Dictionary<Type, long>();
		UnityEngine.Object[] array2 = array;
		foreach (UnityEngine.Object @object in array2)
		{
			int runtimeMemorySize = Profiler.GetRuntimeMemorySize(@object);
			if (dictionary.ContainsKey(@object.GetType()))
			{
				dictionary[@object.GetType()]++;
			}
			else
			{
				dictionary.Add(@object.GetType(), 1);
			}
			if (dictionary2.ContainsKey(@object.GetType()))
			{
				dictionary2[@object.GetType()] += runtimeMemorySize;
			}
			else
			{
				dictionary2.Add(@object.GetType(), runtimeMemorySize);
			}
		}
		foreach (KeyValuePair<Type, long> item in dictionary2.OrderByDescending(delegate(KeyValuePair<Type, long> x)
		{
			KeyValuePair<Type, long> keyValuePair = x;
			return keyValuePair.Value;
		}))
		{
			text = string.Concat(text, dictionary[item.Key].ToString().PadLeft(10), " ", item.Value.FormatBytes().PadLeft(15), "\t", item.Key, "\n");
		}
		args.ReplyWith(text);
	}

	[ServerVar]
	[ClientVar]
	public static void textures(Arg args)
	{
		UnityEngine.Texture[] array = UnityEngine.Object.FindObjectsOfType<UnityEngine.Texture>();
		string text = "";
		UnityEngine.Texture[] array2 = array;
		foreach (UnityEngine.Texture texture in array2)
		{
			string text2 = Profiler.GetRuntimeMemorySize(texture).FormatBytes();
			text = text + texture.ToString().PadRight(30) + texture.name.PadRight(30) + text2 + "\n";
		}
		args.ReplyWith(text);
	}

	[ServerVar]
	[ClientVar]
	public static void colliders(Arg args)
	{
		int num = (from x in UnityEngine.Object.FindObjectsOfType<Collider>()
			where x.enabled
			select x).Count();
		int num2 = (from x in UnityEngine.Object.FindObjectsOfType<Collider>()
			where !x.enabled
			select x).Count();
		string strValue = num + " colliders enabled, " + num2 + " disabled";
		args.ReplyWith(strValue);
	}

	[ServerVar]
	[ClientVar]
	public static void error(Arg args)
	{
		((GameObject)null).transform.position = Vector3.zero;
	}

	[ServerVar]
	[ClientVar]
	public static void queue(Arg args)
	{
		string text = "";
		text = text + "stabilityCheckQueue:\t\t" + StabilityEntity.stabilityCheckQueue.Info() + "\n";
		text = text + "updateSurroundingsQueue:\t" + StabilityEntity.updateSurroundingsQueue.Info() + "\n";
		args.ReplyWith(text);
	}

	[ServerUserVar]
	public static void setinfo(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer)
		{
			string @string = args.GetString(0, null);
			string string2 = args.GetString(1, null);
			if (@string != null && string2 != null)
			{
				basePlayer.SetInfo(@string, string2);
			}
		}
	}

	[ServerVar]
	public static void sleep(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer && !basePlayer.IsSleeping() && !basePlayer.IsSpectating() && !basePlayer.IsDead())
		{
			basePlayer.StartSleeping();
		}
	}

	[ServerUserVar]
	public static void kill(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer && !basePlayer.IsSpectating() && !basePlayer.IsDead())
		{
			if (basePlayer.CanSuicide())
			{
				basePlayer.MarkSuicide();
				basePlayer.Hurt(1000f, DamageType.Suicide, basePlayer, useProtection: false);
			}
			else
			{
				basePlayer.ConsoleMessage("You can't suicide again so quickly, wait a while");
			}
		}
	}

	[ServerUserVar]
	public static void respawn(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (!basePlayer)
		{
			return;
		}
		if (!basePlayer.IsDead() && !basePlayer.IsSpectating())
		{
			if (developer > 0)
			{
				UnityEngine.Debug.LogWarning(string.Concat(basePlayer, " wanted to respawn but isn't dead or spectating"));
			}
			basePlayer.SendNetworkUpdate();
		}
		else if (basePlayer.CanRespawn())
		{
			basePlayer.MarkRespawn();
			basePlayer.Respawn();
		}
		else
		{
			basePlayer.ConsoleMessage("You can't respawn again so quickly, wait a while");
		}
	}

	[ServerVar]
	public static void injure(Arg args)
	{
		InjurePlayer(ArgEx.Player(args));
	}

	public static void InjurePlayer(BasePlayer ply)
	{
		if (ply == null || ply.IsDead())
		{
			return;
		}
		if (Server.woundingenabled && !ply.IsIncapacitated() && !ply.IsSleeping() && !ply.isMounted)
		{
			if (ply.IsCrawling())
			{
				ply.GoToIncapacitated(null);
			}
			else
			{
				ply.BecomeWounded();
			}
		}
		else
		{
			ply.ConsoleMessage("Can't go to wounded state right now.");
		}
	}

	[ServerVar]
	public static void recover(Arg args)
	{
		RecoverPlayer(ArgEx.Player(args));
	}

	public static void RecoverPlayer(BasePlayer ply)
	{
		if (!(ply == null) && !ply.IsDead())
		{
			ply.StopWounded();
		}
	}

	[ServerVar]
	public static void spectate(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer)
		{
			if (!basePlayer.IsDead())
			{
				basePlayer.DieInstantly();
			}
			string @string = args.GetString(0);
			if (basePlayer.IsDead())
			{
				basePlayer.StartSpectating();
				basePlayer.UpdateSpectateTarget(@string);
			}
		}
	}

	[ServerUserVar]
	public static void respawn_sleepingbag(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (!basePlayer || !basePlayer.IsDead())
		{
			return;
		}
		uint uInt = args.GetUInt(0);
		if (uInt == 0)
		{
			args.ReplyWith("Missing sleeping bag ID");
		}
		else if (basePlayer.CanRespawn())
		{
			if (SleepingBag.SpawnPlayer(basePlayer, uInt))
			{
				basePlayer.MarkRespawn();
			}
			else
			{
				args.ReplyWith("Couldn't spawn in sleeping bag!");
			}
		}
		else
		{
			basePlayer.ConsoleMessage("You can't respawn again so quickly, wait a while");
		}
	}

	[ServerUserVar]
	public static void respawn_sleepingbag_remove(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer)
		{
			uint uInt = args.GetUInt(0);
			if (uInt == 0)
			{
				args.ReplyWith("Missing sleeping bag ID");
			}
			else
			{
				SleepingBag.DestroyBag(basePlayer, uInt);
			}
		}
	}

	[ServerUserVar]
	public static void status_sv(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer)
		{
			args.ReplyWith(basePlayer.GetDebugStatus());
		}
	}

	[ClientVar]
	public static void status_cl(Arg args)
	{
	}

	[ServerVar]
	public static void teleport(Arg args)
	{
		if (args.HasArgs(2))
		{
			BasePlayer playerOrSleeperOrBot = ArgEx.GetPlayerOrSleeperOrBot(args, 0);
			if ((bool)playerOrSleeperOrBot && playerOrSleeperOrBot.IsAlive())
			{
				BasePlayer playerOrSleeperOrBot2 = ArgEx.GetPlayerOrSleeperOrBot(args, 1);
				if ((bool)playerOrSleeperOrBot2 && playerOrSleeperOrBot2.IsAlive())
				{
					playerOrSleeperOrBot.Teleport(playerOrSleeperOrBot2);
				}
			}
			return;
		}
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer && basePlayer.IsAlive())
		{
			BasePlayer playerOrSleeperOrBot3 = ArgEx.GetPlayerOrSleeperOrBot(args, 0);
			if ((bool)playerOrSleeperOrBot3 && playerOrSleeperOrBot3.IsAlive())
			{
				basePlayer.Teleport(playerOrSleeperOrBot3);
			}
		}
	}

	[ServerVar]
	public static void teleport2me(Arg args)
	{
		BasePlayer playerOrSleeperOrBot = ArgEx.GetPlayerOrSleeperOrBot(args, 0);
		if (playerOrSleeperOrBot == null)
		{
			args.ReplyWith("Player or bot not found");
			return;
		}
		if (!playerOrSleeperOrBot.IsAlive())
		{
			args.ReplyWith("Target is not alive");
			return;
		}
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer && basePlayer.IsAlive())
		{
			playerOrSleeperOrBot.Teleport(basePlayer);
		}
	}

	[ServerVar]
	public static void teleportany(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer && basePlayer.IsAlive())
		{
			basePlayer.Teleport(args.GetString(0), playersOnly: false);
		}
	}

	[ServerVar]
	public static void teleportpos(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer && basePlayer.IsAlive())
		{
			basePlayer.Teleport(args.GetVector3(0, Vector3.zero));
		}
	}

	[ServerVar]
	public static void teleportlos(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer && basePlayer.IsAlive())
		{
			Ray ray = basePlayer.eyes.HeadRay();
			int @int = args.GetInt(0, 1000);
			if (UnityEngine.Physics.Raycast(ray, out var hitInfo, @int, 1218652417))
			{
				basePlayer.Teleport(hitInfo.point);
			}
			else
			{
				basePlayer.Teleport(ray.origin + ray.direction * @int);
			}
		}
	}

	[ServerVar]
	public static void teleport2owneditem(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		BasePlayer playerOrSleeper = ArgEx.GetPlayerOrSleeper(arg, 0);
		ulong result;
		if (playerOrSleeper != null)
		{
			result = playerOrSleeper.userID;
		}
		else if (!ulong.TryParse(arg.GetString(0), out result))
		{
			arg.ReplyWith("No player with that id found");
			return;
		}
		string @string = arg.GetString(1);
		BaseEntity[] array = BaseEntity.Util.FindTargetsOwnedBy(result, @string);
		if (array.Length == 0)
		{
			arg.ReplyWith("No targets found");
			return;
		}
		int num = UnityEngine.Random.Range(0, array.Length);
		arg.ReplyWith($"Teleporting to {array[num].ShortPrefabName} at {array[num].transform.position}");
		basePlayer.Teleport(array[num].transform.position);
	}

	[ServerVar]
	public static void teleport2autheditem(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		BasePlayer playerOrSleeper = ArgEx.GetPlayerOrSleeper(arg, 0);
		ulong result;
		if (playerOrSleeper != null)
		{
			result = playerOrSleeper.userID;
		}
		else if (!ulong.TryParse(arg.GetString(0), out result))
		{
			arg.ReplyWith("No player with that id found");
			return;
		}
		string @string = arg.GetString(1);
		BaseEntity[] array = BaseEntity.Util.FindTargetsAuthedTo(result, @string);
		if (array.Length == 0)
		{
			arg.ReplyWith("No targets found");
			return;
		}
		int num = UnityEngine.Random.Range(0, array.Length);
		arg.ReplyWith($"Teleporting to {array[num].ShortPrefabName} at {array[num].transform.position}");
		basePlayer.Teleport(array[num].transform.position);
	}

	[ServerVar]
	public static void teleport2marker(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer.ServerCurrentMapNote == null)
		{
			arg.ReplyWith("You don't have a marker set");
			return;
		}
		Vector3 worldPosition = basePlayer.ServerCurrentMapNote.worldPosition;
		float height = TerrainMeta.HeightMap.GetHeight(worldPosition);
		float height2 = TerrainMeta.WaterMap.GetHeight(worldPosition);
		worldPosition.y = Mathf.Max(height, height2);
		basePlayer.Teleport(worldPosition);
	}

	[ServerVar]
	public static void teleport2death(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer.ServerCurrentDeathNote == null)
		{
			arg.ReplyWith("You don't have a current death note!");
		}
		Vector3 worldPosition = basePlayer.ServerCurrentDeathNote.worldPosition;
		basePlayer.Teleport(worldPosition);
	}

	[ServerVar]
	[ClientVar]
	public static void free(Arg args)
	{
		Pool.clear_prefabs(args);
		Pool.clear_assets(args);
		Pool.clear_memory(args);
		GC.collect();
		GC.unload();
	}

	[ServerVar(ServerUser = true)]
	[ClientVar]
	public static void version(Arg arg)
	{
		arg.ReplyWith($"Protocol: {Protocol.printable}\nBuild Date: {BuildInfo.Current.BuildDate}\nUnity Version: {UnityEngine.Application.unityVersion}\nChangeset: {BuildInfo.Current.Scm.ChangeId}\nBranch: {BuildInfo.Current.Scm.Branch}");
	}

	[ServerVar]
	[ClientVar]
	public static void sysinfo(Arg arg)
	{
		arg.ReplyWith(SystemInfoGeneralText.currentInfo);
	}

	[ServerVar]
	[ClientVar]
	public static void sysuid(Arg arg)
	{
		arg.ReplyWith(SystemInfo.deviceUniqueIdentifier);
	}

	[ServerVar]
	public static void breakitem(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer)
		{
			Item activeItem = basePlayer.GetActiveItem();
			activeItem?.LoseCondition(activeItem.condition);
		}
	}

	[ServerVar]
	public static void breakclothing(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (!basePlayer)
		{
			return;
		}
		foreach (Item item in basePlayer.inventory.containerWear.itemList)
		{
			item?.LoseCondition(item.condition);
		}
	}

	[ServerVar]
	[ClientVar]
	public static void subscriptions(Arg arg)
	{
		TextTable textTable = new TextTable();
		textTable.AddColumn("realm");
		textTable.AddColumn("group");
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((bool)basePlayer)
		{
			foreach (Group item in basePlayer.net.subscriber.subscribed)
			{
				textTable.AddRow("sv", item.ID.ToString());
			}
		}
		arg.ReplyWith(textTable.ToString());
	}

	[ServerVar]
	public static void ClearAllSprays()
	{
		List<SprayCanSpray> obj = Facepunch.Pool.GetList<SprayCanSpray>();
		foreach (SprayCanSpray allSpray in SprayCanSpray.AllSprays)
		{
			obj.Add(allSpray);
		}
		foreach (SprayCanSpray item in obj)
		{
			item.Kill();
		}
		Facepunch.Pool.FreeList(ref obj);
	}

	[ServerVar]
	public static void ClearAllSpraysByPlayer(Arg arg)
	{
		if (!arg.HasArgs())
		{
			return;
		}
		ulong uLong = arg.GetULong(0, 0uL);
		List<SprayCanSpray> obj = Facepunch.Pool.GetList<SprayCanSpray>();
		foreach (SprayCanSpray allSpray in SprayCanSpray.AllSprays)
		{
			if (allSpray.sprayedByPlayer == uLong)
			{
				obj.Add(allSpray);
			}
		}
		foreach (SprayCanSpray item in obj)
		{
			item.Kill();
		}
		int count = obj.Count;
		Facepunch.Pool.FreeList(ref obj);
		arg.ReplyWith($"Deleted {count} sprays by {uLong}");
	}

	[ServerVar]
	public static void ClearSpraysInRadius(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			return;
		}
		float @float = arg.GetFloat(0, 16f);
		Vector3 position = basePlayer.transform.position;
		List<SprayCanSpray> obj = Facepunch.Pool.GetList<SprayCanSpray>();
		foreach (SprayCanSpray allSpray in SprayCanSpray.AllSprays)
		{
			if (allSpray.Distance(position) <= @float)
			{
				obj.Add(allSpray);
			}
		}
		foreach (SprayCanSpray item in obj)
		{
			item.Kill();
		}
		int count = obj.Count;
		Facepunch.Pool.FreeList(ref obj);
		arg.ReplyWith($"Deleted {count} sprays within {@float} of {basePlayer.displayName}");
	}
}
