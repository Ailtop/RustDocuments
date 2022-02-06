using UnityEngine;

namespace ConVar
{
	[Factory("vehicle")]
	public class vehicle : ConsoleSystem
	{
		[ServerVar]
		[Help("how long until boat corpses despawn")]
		public static float boat_corpse_seconds = 300f;

		[ServerVar(Help = "If true, trains always explode when destroyed, and hitting a barrier always destroys the train immediately. Default: false")]
		public static bool cinematictrains = false;

		[ServerVar(Help = "Determines whether modular cars turn into wrecks when destroyed, or just immediately gib. Default: true")]
		public static bool carwrecks = true;

		[ServerVar(Help = "Determines whether vehicles drop storage items when destroyed. Default: true")]
		public static bool vehiclesdroploot = true;

		[ServerUserVar]
		public static void swapseats(Arg arg)
		{
			int targetSeat = 0;
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (basePlayer == null || basePlayer.SwapSeatCooldown())
			{
				return;
			}
			BaseMountable mounted = basePlayer.GetMounted();
			if (!(mounted == null))
			{
				BaseVehicle baseVehicle = mounted.GetComponent<BaseVehicle>();
				if (baseVehicle == null)
				{
					baseVehicle = mounted.VehicleParent();
				}
				if (!(baseVehicle == null))
				{
					baseVehicle.SwapSeats(basePlayer, targetSeat);
				}
			}
		}

		[ServerVar]
		public static void fixcars(Arg arg)
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (basePlayer == null)
			{
				arg.ReplyWith("Null player.");
				return;
			}
			if (!basePlayer.IsAdmin)
			{
				arg.ReplyWith("Must be an admin to use fixcars.");
				return;
			}
			int @int = arg.GetInt(0, 2);
			@int = Mathf.Clamp(@int, 1, 3);
			BaseVehicle[] array = Object.FindObjectsOfType<BaseVehicle>();
			int num = 0;
			BaseVehicle[] array2 = array;
			foreach (BaseVehicle baseVehicle in array2)
			{
				if (baseVehicle.isServer && Vector3.Distance(baseVehicle.transform.position, basePlayer.transform.position) <= 5f && baseVehicle.AdminFixUp(@int))
				{
					num++;
				}
			}
			MLRS[] array3 = Object.FindObjectsOfType<MLRS>();
			foreach (MLRS mLRS in array3)
			{
				if (mLRS.isServer && Vector3.Distance(mLRS.transform.position, basePlayer.transform.position) <= 5f && mLRS.AdminFixUp())
				{
					num++;
				}
			}
			arg.ReplyWith($"Fixed up {num} vehicles.");
		}
	}
}
