using System.Collections.Generic;
using System.Text;
using Facepunch;
using ProtoBuf;
using UnityEngine;

namespace ConVar
{
	[Factory("player")]
	public class Player : ConsoleSystem
	{
		[ServerVar]
		public static int tickrate_cl = 20;

		[ServerVar]
		public static int tickrate_sv = 16;

		[ServerUserVar]
		[ClientVar(AllowRunFromServer = true)]
		public static void cinematic_play(Arg arg)
		{
			if (!arg.HasArgs() || !arg.IsServerside)
			{
				return;
			}
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (!(basePlayer == null))
			{
				string strCommand = string.Empty;
				if (basePlayer.IsAdmin || basePlayer.IsDeveloper)
				{
					strCommand = arg.cmd.FullName + " " + arg.FullString + " " + basePlayer.UserIDString;
				}
				else if (Server.cinematic)
				{
					strCommand = arg.cmd.FullName + " " + arg.GetString(0) + " " + basePlayer.UserIDString;
				}
				if (Server.cinematic)
				{
					ConsoleNetwork.BroadcastToAllClients(strCommand);
				}
				else if (basePlayer.IsAdmin || basePlayer.IsDeveloper)
				{
					ConsoleNetwork.SendClientCommand(arg.Connection, strCommand);
				}
			}
		}

		[ServerUserVar]
		[ClientVar(AllowRunFromServer = true)]
		public static void cinematic_stop(Arg arg)
		{
			if (!arg.IsServerside)
			{
				return;
			}
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (!(basePlayer == null))
			{
				string strCommand = string.Empty;
				if (basePlayer.IsAdmin || basePlayer.IsDeveloper)
				{
					strCommand = arg.cmd.FullName + " " + arg.FullString + " " + basePlayer.UserIDString;
				}
				else if (Server.cinematic)
				{
					strCommand = arg.cmd.FullName + " " + basePlayer.UserIDString;
				}
				if (Server.cinematic)
				{
					ConsoleNetwork.BroadcastToAllClients(strCommand);
				}
				else if (basePlayer.IsAdmin || basePlayer.IsDeveloper)
				{
					ConsoleNetwork.SendClientCommand(arg.Connection, strCommand);
				}
			}
		}

		[ServerUserVar]
		public static void cinematic_gesture(Arg arg)
		{
			if (Server.cinematic)
			{
				string @string = arg.GetString(0);
				BasePlayer basePlayer = ArgEx.GetPlayer(arg, 1);
				if (basePlayer == null)
				{
					basePlayer = ArgEx.Player(arg);
				}
				basePlayer.UpdateActiveItem(0u);
				basePlayer.SignalBroadcast(BaseEntity.Signal.Gesture, @string);
			}
		}

		[ServerUserVar]
		public static void copyrotation(Arg arg)
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (basePlayer.IsAdmin || basePlayer.IsDeveloper || Server.cinematic)
			{
				uint uInt = arg.GetUInt(0);
				BasePlayer basePlayer2 = BasePlayer.FindByID(uInt);
				if (basePlayer2 == null)
				{
					basePlayer2 = BasePlayer.FindBot(uInt);
				}
				if (basePlayer2 != null)
				{
					basePlayer2.CopyRotation(basePlayer);
					Debug.Log("Copied rotation of " + basePlayer2.UserIDString);
				}
			}
		}

		[ServerUserVar]
		public static void mount(Arg arg)
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (!basePlayer.IsAdmin && !basePlayer.IsDeveloper && !Server.cinematic)
			{
				return;
			}
			uint uInt = arg.GetUInt(0);
			BasePlayer basePlayer2 = BasePlayer.FindByID(uInt);
			if (basePlayer2 == null)
			{
				basePlayer2 = BasePlayer.FindBot(uInt);
			}
			RaycastHit hitInfo;
			if (!basePlayer2 || !UnityEngine.Physics.Raycast(basePlayer.eyes.position, basePlayer.eyes.HeadForward(), out hitInfo, 5f, 10496, QueryTriggerInteraction.Ignore))
			{
				return;
			}
			BaseEntity entity = RaycastHitEx.GetEntity(hitInfo);
			if (!entity)
			{
				return;
			}
			BaseMountable baseMountable = entity.GetComponent<BaseMountable>();
			if (!baseMountable)
			{
				BaseVehicle baseVehicle = entity.GetComponentInParent<BaseVehicle>();
				if ((bool)baseVehicle)
				{
					if (!baseVehicle.isServer)
					{
						baseVehicle = BaseNetworkable.serverEntities.Find(baseVehicle.net.ID) as BaseVehicle;
					}
					baseVehicle.AttemptMount(basePlayer2);
					return;
				}
			}
			if ((bool)baseMountable && !baseMountable.isServer)
			{
				baseMountable = BaseNetworkable.serverEntities.Find(baseMountable.net.ID) as BaseMountable;
			}
			if ((bool)baseMountable)
			{
				baseMountable.AttemptMount(basePlayer2);
			}
		}

		[ServerVar]
		public static void gotosleep(Arg arg)
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (!basePlayer.IsAdmin && !basePlayer.IsDeveloper && !Server.cinematic)
			{
				return;
			}
			uint uInt = arg.GetUInt(0);
			BasePlayer basePlayer2 = BasePlayer.FindSleeping(uInt.ToString());
			if (!basePlayer2)
			{
				basePlayer2 = BasePlayer.FindBotClosestMatch(uInt.ToString());
				if (basePlayer2.IsSleeping())
				{
					basePlayer2 = null;
				}
			}
			if ((bool)basePlayer2)
			{
				basePlayer2.StartSleeping();
			}
		}

		[ServerVar]
		public static void dismount(Arg arg)
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (basePlayer.IsAdmin || basePlayer.IsDeveloper || Server.cinematic)
			{
				uint uInt = arg.GetUInt(0);
				BasePlayer basePlayer2 = BasePlayer.FindByID(uInt);
				if (basePlayer2 == null)
				{
					basePlayer2 = BasePlayer.FindBot(uInt);
				}
				if ((bool)basePlayer2 && (bool)basePlayer2 && basePlayer2.isMounted)
				{
					basePlayer2.GetMounted().DismountPlayer(basePlayer2);
				}
			}
		}

		[ServerVar]
		public static void swapseat(Arg arg)
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (!basePlayer.IsAdmin && !basePlayer.IsDeveloper && !Server.cinematic)
			{
				return;
			}
			uint uInt = arg.GetUInt(0);
			BasePlayer basePlayer2 = BasePlayer.FindByID(uInt);
			if (basePlayer2 == null)
			{
				basePlayer2 = BasePlayer.FindBot(uInt);
			}
			if ((bool)basePlayer2)
			{
				int @int = arg.GetInt(1);
				if ((bool)basePlayer2 && basePlayer2.isMounted && (bool)basePlayer2.GetMounted().VehicleParent())
				{
					basePlayer2.GetMounted().VehicleParent().SwapSeats(basePlayer2, @int);
				}
			}
		}

		[ServerVar]
		public static void wakeup(Arg arg)
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (basePlayer.IsAdmin || basePlayer.IsDeveloper || Server.cinematic)
			{
				BasePlayer basePlayer2 = BasePlayer.FindSleeping(arg.GetUInt(0).ToString());
				if ((bool)basePlayer2)
				{
					basePlayer2.EndSleeping();
				}
			}
		}

		[ServerVar]
		public static void wakeupall(Arg arg)
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (!basePlayer.IsAdmin && !basePlayer.IsDeveloper && !Server.cinematic)
			{
				return;
			}
			List<BasePlayer> obj = Facepunch.Pool.GetList<BasePlayer>();
			foreach (BasePlayer sleepingPlayer in BasePlayer.sleepingPlayerList)
			{
				obj.Add(sleepingPlayer);
			}
			foreach (BasePlayer item in obj)
			{
				item.EndSleeping();
			}
			Facepunch.Pool.FreeList(ref obj);
		}

		[ServerVar]
		public static void printstats(Arg arg)
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (!basePlayer)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"{basePlayer.lifeStory.secondsAlive:F1}s alive");
			stringBuilder.AppendLine($"{basePlayer.lifeStory.secondsSleeping:F1}s sleeping");
			stringBuilder.AppendLine($"{basePlayer.lifeStory.secondsSwimming:F1}s swimming");
			stringBuilder.AppendLine($"{basePlayer.lifeStory.secondsInBase:F1}s in base");
			stringBuilder.AppendLine($"{basePlayer.lifeStory.secondsWilderness:F1}s in wilderness");
			stringBuilder.AppendLine($"{basePlayer.lifeStory.secondsInMonument:F1}s in monuments");
			stringBuilder.AppendLine($"{basePlayer.lifeStory.secondsFlying:F1}s flying");
			stringBuilder.AppendLine($"{basePlayer.lifeStory.secondsBoating:F1}s boating");
			stringBuilder.AppendLine($"{basePlayer.lifeStory.secondsDriving:F1}s driving");
			stringBuilder.AppendLine($"{basePlayer.lifeStory.metersRun:F1}m run");
			stringBuilder.AppendLine($"{basePlayer.lifeStory.metersWalked:F1}m walked");
			stringBuilder.AppendLine($"{basePlayer.lifeStory.totalDamageTaken:F1} damage taken");
			stringBuilder.AppendLine($"{basePlayer.lifeStory.totalHealing:F1} damage healed");
			stringBuilder.AppendLine("===");
			stringBuilder.AppendLine($"{basePlayer.lifeStory.killedPlayers} other players killed");
			stringBuilder.AppendLine($"{basePlayer.lifeStory.killedScientists} scientists killed");
			stringBuilder.AppendLine($"{basePlayer.lifeStory.killedAnimals} animals killed");
			stringBuilder.AppendLine("===");
			stringBuilder.AppendLine("Weapon stats:");
			if (basePlayer.lifeStory.weaponStats != null)
			{
				foreach (PlayerLifeStory.WeaponStats weaponStat in basePlayer.lifeStory.weaponStats)
				{
					float num = (float)weaponStat.shotsHit / (float)weaponStat.shotsFired;
					num *= 100f;
					stringBuilder.AppendLine($"{weaponStat.weaponName} - shots fired: {weaponStat.shotsFired} shots hit: {weaponStat.shotsHit} accuracy: {num:F1}%");
				}
			}
			stringBuilder.AppendLine("===");
			stringBuilder.AppendLine("Misc stats:");
			if (basePlayer.lifeStory.genericStats != null)
			{
				foreach (PlayerLifeStory.GenericStat genericStat in basePlayer.lifeStory.genericStats)
				{
					stringBuilder.AppendLine($"{genericStat.key} = {genericStat.value}");
				}
			}
			arg.ReplyWith(stringBuilder.ToString());
		}

		[ServerVar]
		public static void printpresence(Arg arg)
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			bool flag = (basePlayer.currentTimeCategory & 1) != 0;
			bool flag2 = (basePlayer.currentTimeCategory & 4) != 0;
			bool flag3 = (basePlayer.currentTimeCategory & 2) != 0;
			bool flag4 = (basePlayer.currentTimeCategory & 0x20) != 0;
			bool flag5 = (basePlayer.currentTimeCategory & 0x10) != 0;
			bool flag6 = (basePlayer.currentTimeCategory & 8) != 0;
			arg.ReplyWith($"Wilderness:{flag} Base:{flag2} Monument:{flag3} Swimming: {flag4} Boating: {flag5} Flying: {flag6}");
		}

		[ServerVar(Help = "Resets the PlayerState of the given player")]
		public static void resetstate(Arg args)
		{
			BasePlayer playerOrSleeper = ArgEx.GetPlayerOrSleeper(args, 0);
			if (playerOrSleeper == null)
			{
				args.ReplyWith("Player not found");
				return;
			}
			playerOrSleeper.ResetPlayerState();
			args.ReplyWith("Player state reset");
		}

		[ServerVar(ServerAdmin = true)]
		public static void fillwater(Arg arg)
		{
			bool num = arg.GetString(0).ToLower() == "salt";
			BasePlayer basePlayer = ArgEx.Player(arg);
			ItemDefinition liquidType = ItemManager.FindItemDefinition(num ? "water.salt" : "water");
			for (int i = 0; i < PlayerBelt.MaxBeltSlots; i++)
			{
				Item itemInSlot = basePlayer.Belt.GetItemInSlot(i);
				BaseLiquidVessel baseLiquidVessel;
				if (itemInSlot != null && (object)(baseLiquidVessel = itemInSlot.GetHeldEntity() as BaseLiquidVessel) != null && baseLiquidVessel.hasLid)
				{
					int amount = 999;
					ItemModContainer component;
					if (baseLiquidVessel.GetItem().info.TryGetComponent<ItemModContainer>(out component))
					{
						amount = component.maxStackSize;
					}
					baseLiquidVessel.AddLiquid(liquidType, amount);
				}
			}
		}

		[ServerVar]
		public static void createskull(Arg arg)
		{
			string text = arg.GetString(0);
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (string.IsNullOrEmpty(text))
			{
				text = RandomUsernames.Get(Random.Range(0, 1000));
			}
			Item item = ItemManager.Create(ItemManager.FindItemDefinition("skull.human"), 1, 0uL);
			item.name = HumanBodyResourceDispenser.CreateSkullName(text);
			basePlayer.inventory.GiveItem(item);
		}

		[ServerVar]
		public static void gesture_radius(Arg arg)
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (basePlayer == null || !basePlayer.IsAdmin)
			{
				return;
			}
			float @float = arg.GetFloat(0);
			List<string> list = Facepunch.Pool.GetList<string>();
			for (int i = 0; i < 5; i++)
			{
				if (!string.IsNullOrEmpty(arg.GetString(i + 1)))
				{
					list.Add(arg.GetString(i + 1));
				}
			}
			if (list.Count == 0)
			{
				arg.ReplyWith("No gestures provided. eg. player.gesture_radius 10f cabbagepatch raiseroof");
				return;
			}
			List<BasePlayer> obj = Facepunch.Pool.GetList<BasePlayer>();
			global::Vis.Entities(basePlayer.transform.position, @float, obj, 131072);
			foreach (BasePlayer item in obj)
			{
				GestureConfig toPlay = basePlayer.gestureList.StringToGesture(list[Random.Range(0, list.Count)]);
				item.Server_StartGesture(toPlay);
			}
			Facepunch.Pool.FreeList(ref obj);
		}

		[ServerVar]
		public static void stopgesture_radius(Arg arg)
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (basePlayer == null || !basePlayer.IsAdmin)
			{
				return;
			}
			float @float = arg.GetFloat(0);
			List<BasePlayer> obj = Facepunch.Pool.GetList<BasePlayer>();
			global::Vis.Entities(basePlayer.transform.position, @float, obj, 131072);
			foreach (BasePlayer item in obj)
			{
				item.Server_CancelGesture();
			}
			Facepunch.Pool.FreeList(ref obj);
		}

		[ServerVar]
		public static void markhostile(Arg arg)
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (basePlayer != null)
			{
				basePlayer.MarkHostileFor();
			}
		}
	}
}
