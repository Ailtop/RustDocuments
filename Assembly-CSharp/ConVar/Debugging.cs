using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Facepunch;
using Facepunch.Unity;
using Rust;
using UnityEngine;

namespace ConVar;

[Factory("debug")]
public class Debugging : ConsoleSystem
{
	[ServerVar]
	[ClientVar]
	public static bool checktriggers = false;

	[ServerVar]
	public static bool checkparentingtriggers = true;

	[ServerVar(Saved = false, Help = "Shows some debug info for dismount attempts.")]
	public static bool DebugDismounts = false;

	[ServerVar(Help = "Do not damage any items")]
	public static bool disablecondition = false;

	[ClientVar]
	[ServerVar]
	public static bool callbacks = false;

	[ServerVar]
	[ClientVar]
	public static bool log
	{
		get
		{
			return Debug.unityLogger.logEnabled;
		}
		set
		{
			Debug.unityLogger.logEnabled = value;
		}
	}

	[ServerVar]
	[ClientVar]
	public static void renderinfo(Arg arg)
	{
		RenderInfo.GenerateReport();
	}

	[ServerVar]
	public static void enable_player_movement(Arg arg)
	{
		if (arg.IsAdmin)
		{
			bool @bool = arg.GetBool(0, def: true);
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (basePlayer == null)
			{
				arg.ReplyWith("Must be called from client with player model");
				return;
			}
			basePlayer.ClientRPCPlayer(null, basePlayer, "TogglePlayerMovement", @bool);
			arg.ReplyWith((@bool ? "enabled" : "disabled") + " player movement");
		}
	}

	[ClientVar]
	[ServerVar]
	public static void stall(Arg arg)
	{
		float num = Mathf.Clamp(arg.GetFloat(0), 0f, 1f);
		arg.ReplyWith("Stalling for " + num + " seconds...");
		Thread.Sleep(Mathf.RoundToInt(num * 1000f));
	}

	[ServerVar(Help = "Takes you in and out of your current network group, causing you to delete and then download all entities in your PVS again")]
	public static void flushgroup(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!(basePlayer == null))
		{
			basePlayer.net.SwitchGroup(BaseNetworkable.LimboNetworkGroup);
			basePlayer.UpdateNetworkGroup();
		}
	}

	[ServerVar(Help = "Break the current held object")]
	public static void breakheld(Arg arg)
	{
		Item activeItem = ArgEx.Player(arg).GetActiveItem();
		activeItem?.LoseCondition(activeItem.condition * 2f);
	}

	[ServerVar(Help = "reset all puzzles")]
	public static void puzzlereset(Arg arg)
	{
		if (!(ArgEx.Player(arg) == null))
		{
			PuzzleReset[] array = Object.FindObjectsOfType<PuzzleReset>();
			Debug.Log("iterating...");
			PuzzleReset[] array2 = array;
			foreach (PuzzleReset puzzleReset in array2)
			{
				Debug.Log("resetting puzzle at :" + puzzleReset.transform.position);
				puzzleReset.DoReset();
				puzzleReset.ResetTimer();
			}
		}
	}

	[ServerVar(EditorOnly = true, Help = "respawn all puzzles from their prefabs")]
	public static void puzzleprefabrespawn(Arg arg)
	{
		foreach (BaseNetworkable item in BaseNetworkable.serverEntities.Where((BaseNetworkable x) => x is IOEntity && PrefabAttribute.server.Find<Construction>(x.prefabID) == null).ToList())
		{
			item.Kill();
		}
		foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
		{
			GameObject gameObject = GameManager.server.FindPrefab(monument.gameObject.name);
			if (gameObject == null)
			{
				continue;
			}
			Dictionary<IOEntity, IOEntity> dictionary = new Dictionary<IOEntity, IOEntity>();
			IOEntity[] componentsInChildren = gameObject.GetComponentsInChildren<IOEntity>(includeInactive: true);
			foreach (IOEntity iOEntity in componentsInChildren)
			{
				Quaternion rot = monument.transform.rotation * iOEntity.transform.rotation;
				Vector3 pos = monument.transform.TransformPoint(iOEntity.transform.position);
				BaseEntity newEntity = GameManager.server.CreateEntity(iOEntity.PrefabName, pos, rot);
				IOEntity iOEntity2 = newEntity as IOEntity;
				if (!(iOEntity2 != null))
				{
					continue;
				}
				dictionary.Add(iOEntity, iOEntity2);
				DoorManipulator doorManipulator = newEntity as DoorManipulator;
				if (doorManipulator != null)
				{
					List<Door> obj = Facepunch.Pool.GetList<Door>();
					global::Vis.Entities(newEntity.transform.position, 10f, obj);
					Door door = obj.OrderBy((Door x) => x.Distance(newEntity.transform.position)).FirstOrDefault();
					if (door != null)
					{
						doorManipulator.targetDoor = door;
					}
					Facepunch.Pool.FreeList(ref obj);
				}
				CardReader cardReader = newEntity as CardReader;
				if (cardReader != null)
				{
					CardReader cardReader2 = iOEntity as CardReader;
					if (cardReader2 != null)
					{
						cardReader.accessLevel = cardReader2.accessLevel;
						cardReader.accessDuration = cardReader2.accessDuration;
					}
				}
				TimerSwitch timerSwitch = newEntity as TimerSwitch;
				if (timerSwitch != null)
				{
					TimerSwitch timerSwitch2 = iOEntity as TimerSwitch;
					if (timerSwitch2 != null)
					{
						timerSwitch.timerLength = timerSwitch2.timerLength;
					}
				}
			}
			foreach (KeyValuePair<IOEntity, IOEntity> item2 in dictionary)
			{
				IOEntity key = item2.Key;
				IOEntity value = item2.Value;
				for (int j = 0; j < key.outputs.Length; j++)
				{
					if (!(key.outputs[j].connectedTo.ioEnt == null))
					{
						value.outputs[j].connectedTo.ioEnt = dictionary[key.outputs[j].connectedTo.ioEnt];
						value.outputs[j].connectedToSlot = key.outputs[j].connectedToSlot;
					}
				}
			}
			foreach (IOEntity value2 in dictionary.Values)
			{
				value2.Spawn();
			}
		}
	}

	[ServerVar(Help = "Break all the items in your inventory whose name match the passed string")]
	public static void breakitem(Arg arg)
	{
		string @string = arg.GetString(0);
		foreach (Item item in ArgEx.Player(arg).inventory.containerMain.itemList)
		{
			if (item.info.shortname.Contains(@string, CompareOptions.IgnoreCase) && item.hasCondition)
			{
				item.LoseCondition(item.condition * 2f);
			}
		}
	}

	[ServerVar]
	public static void refillvitals(Arg arg)
	{
		AdjustHealth(ArgEx.Player(arg), 1000f);
		AdjustCalories(ArgEx.Player(arg), 1000f);
		AdjustHydration(ArgEx.Player(arg), 1000f);
	}

	[ServerVar]
	public static void heal(Arg arg)
	{
		AdjustHealth(ArgEx.Player(arg), arg.GetInt(0, 1));
	}

	[ServerVar]
	public static void hurt(Arg arg)
	{
		AdjustHealth(ArgEx.Player(arg), -arg.GetInt(0, 1), arg.GetString(1, string.Empty));
	}

	[ServerVar]
	public static void eat(Arg arg)
	{
		AdjustCalories(ArgEx.Player(arg), arg.GetInt(0, 1), arg.GetInt(1, 1));
	}

	[ServerVar]
	public static void drink(Arg arg)
	{
		AdjustHydration(ArgEx.Player(arg), arg.GetInt(0, 1), arg.GetInt(1, 1));
	}

	private static void AdjustHealth(BasePlayer player, float amount, string bone = null)
	{
		HitInfo hitInfo = new HitInfo(player, player, DamageType.Bullet, 0f - amount);
		if (!string.IsNullOrEmpty(bone))
		{
			hitInfo.HitBone = StringPool.Get(bone);
		}
		player.OnAttacked(hitInfo);
	}

	private static void AdjustCalories(BasePlayer player, float amount, float time = 1f)
	{
		player.metabolism.ApplyChange(MetabolismAttribute.Type.Calories, amount, time);
	}

	private static void AdjustHydration(BasePlayer player, float amount, float time = 1f)
	{
		player.metabolism.ApplyChange(MetabolismAttribute.Type.Hydration, amount, time);
	}

	[ServerVar]
	public static void ResetSleepingBagTimers(Arg arg)
	{
		SleepingBag.ResetTimersForPlayer(ArgEx.Player(arg));
	}
}
