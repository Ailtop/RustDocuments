namespace ConVar
{
	[Factory("antihack")]
	public class AntiHack : ConsoleSystem
	{
		[Help("report violations to the anti cheat backend")]
		[ServerVar]
		public static bool reporting = true;

		[Help("are admins allowed to use their admin cheat")]
		[ServerVar]
		public static bool admincheat = true;

		[Help("use antihack to verify object placement by players")]
		[ServerVar]
		public static bool objectplacement = true;

		[ServerVar]
		[Help("use antihack to verify model state sent by players")]
		public static bool modelstate = true;

		[ServerVar]
		[Help("whether or not to force the position on the client")]
		public static bool forceposition = true;

		[ServerVar]
		[Help("0 == users, 1 == admins, 2 == developers")]
		public static int userlevel = 2;

		[ServerVar]
		[Help("0 == no enforcement, 1 == kick, 2 == ban (DISABLED)")]
		public static int enforcementlevel = 1;

		[Help("max allowed client desync, lower value = more false positives")]
		[ServerVar]
		public static float maxdesync = 1f;

		[Help("max allowed client tick interval delta time, lower value = more false positives")]
		[ServerVar]
		public static float maxdeltatime = 1f;

		[Help("line of sight sphere cast radius, 0 == raycast")]
		[ServerVar]
		public static float losradius = 0f;

		[Help("line of sight directional forgiveness when checking eye or center position")]
		[ServerVar]
		public static float losforgiveness = 0.2f;

		[Help("for how many seconds to keep a tick history to use for distance checks")]
		[ServerVar]
		public static float tickhistorytime = 0.5f;

		[Help("how much forgiveness to add when checking the distance from the player tick history")]
		[ServerVar]
		public static float tickhistoryforgiveness = 0.1f;

		[Help("the rate at which violation values go back down")]
		[ServerVar]
		public static float relaxationrate = 0.1f;

		[Help("the time before violation values go back down")]
		[ServerVar]
		public static float relaxationpause = 10f;

		[ServerVar]
		[Help("violation value above this results in enforcement")]
		public static float maxviolation = 100f;

		[Help("0 == disabled, 1 == enabled")]
		[ServerVar]
		public static int terrain_protection = 1;

		[ServerVar]
		[Help("how many slices to subdivide players into for the terrain check")]
		public static int terrain_timeslice = 64;

		[ServerVar]
		[Help("how far to penetrate the terrain before violating")]
		public static float terrain_padding = 2f;

		[ServerVar]
		[Help("violation penalty to hand out when terrain is detected")]
		public static float terrain_penalty = 0f;

		[ServerVar]
		[Help("whether or not to kill the player when terrain is detected")]
		public static bool terrain_kill = true;

		[Help("0 == disabled, 1 == ray, 2 == sphere, 3 == curve")]
		[ServerVar]
		public static int noclip_protection = 3;

		[Help("whether or not to reject movement when noclip is detected")]
		[ServerVar]
		public static bool noclip_reject = true;

		[Help("violation penalty to hand out when noclip is detected")]
		[ServerVar]
		public static float noclip_penalty = 0f;

		[Help("collider margin when checking for noclipping")]
		[ServerVar]
		public static float noclip_margin = 0.09f;

		[ServerVar]
		[Help("collider backtracking when checking for noclipping")]
		public static float noclip_backtracking = 0.01f;

		[ServerVar]
		[Help("movement curve step size, lower value = less false positives")]
		public static float noclip_stepsize = 0.1f;

		[Help("movement curve max steps, lower value = more false positives")]
		[ServerVar]
		public static int noclip_maxsteps = 15;

		[ServerVar]
		[Help("0 == disabled, 1 == simple, 2 == advanced")]
		public static int speedhack_protection = 2;

		[ServerVar]
		[Help("whether or not to reject movement when speedhack is detected")]
		public static bool speedhack_reject = true;

		[ServerVar]
		[Help("violation penalty to hand out when speedhack is detected")]
		public static float speedhack_penalty = 0f;

		[Help("speed threshold to assume speedhacking, lower value = more false positives")]
		[ServerVar]
		public static float speedhack_forgiveness = 2f;

		[ServerVar]
		[Help("speed threshold to assume speedhacking, lower value = more false positives")]
		public static float speedhack_forgiveness_inertia = 10f;

		[Help("speed forgiveness when moving down slopes, lower value = more false positives")]
		[ServerVar]
		public static float speedhack_slopespeed = 10f;

		[Help("0 == disabled, 1 == client, 2 == capsule, 3 == curve")]
		[ServerVar]
		public static int flyhack_protection = 3;

		[Help("whether or not to reject movement when flyhack is detected")]
		[ServerVar]
		public static bool flyhack_reject = false;

		[Help("violation penalty to hand out when flyhack is detected")]
		[ServerVar]
		public static float flyhack_penalty = 100f;

		[Help("distance threshold to assume flyhacking, lower value = more false positives")]
		[ServerVar]
		public static float flyhack_forgiveness_vertical = 1.5f;

		[Help("distance threshold to assume flyhacking, lower value = more false positives")]
		[ServerVar]
		public static float flyhack_forgiveness_vertical_inertia = 10f;

		[Help("distance threshold to assume flyhacking, lower value = more false positives")]
		[ServerVar]
		public static float flyhack_forgiveness_horizontal = 1.5f;

		[ServerVar]
		[Help("distance threshold to assume flyhacking, lower value = more false positives")]
		public static float flyhack_forgiveness_horizontal_inertia = 10f;

		[Help("collider downwards extrusion when checking for flyhacking")]
		[ServerVar]
		public static float flyhack_extrusion = 2f;

		[Help("collider margin when checking for flyhacking")]
		[ServerVar]
		public static float flyhack_margin = 0.05f;

		[Help("movement curve step size, lower value = less false positives")]
		[ServerVar]
		public static float flyhack_stepsize = 0.1f;

		[ServerVar]
		[Help("movement curve max steps, lower value = more false positives")]
		public static int flyhack_maxsteps = 15;

		[ServerVar]
		[Help("0 == disabled, 1 == speed, 2 == speed + entity, 3 == speed + entity + LOS, 4 == speed + entity + LOS + trajectory, 5 == speed + entity + LOS + trajectory + update, 6 == speed + entity + LOS + trajectory + tickhistory")]
		public static int projectile_protection = 6;

		[ServerVar]
		[Help("violation penalty to hand out when projectile hack is detected")]
		public static float projectile_penalty = 0f;

		[ServerVar]
		[Help("projectile speed forgiveness in percent, lower value = more false positives")]
		public static float projectile_forgiveness = 0.5f;

		[ServerVar]
		[Help("projectile server frames to include in delay, lower value = more false positives")]
		public static float projectile_serverframes = 2f;

		[ServerVar]
		[Help("projectile client frames to include in delay, lower value = more false positives")]
		public static float projectile_clientframes = 2f;

		[ServerVar]
		[Help("projectile trajectory forgiveness, lower value = more false positives")]
		public static float projectile_trajectory = 1f;

		[ServerVar]
		[Help("projectile penetration angle change, lower value = more false positives")]
		public static float projectile_anglechange = 60f;

		[Help("projectile penetration velocity change, lower value = more false positives")]
		[ServerVar]
		public static float projectile_velocitychange = 1.1f;

		[ServerVar]
		[Help("projectile desync forgiveness, lower value = more false positives")]
		public static float projectile_desync = 1f;

		[ServerVar]
		[Help("projectile backtracking when checking for LOS")]
		public static float projectile_backtracking = 0.01f;

		[ServerVar]
		[Help("whether or not to include terrain in the projectile LOS checks")]
		public static bool projectile_terraincheck = true;

		[ServerVar]
		[Help("0 == disabled, 1 == initiator, 2 == initiator + target, 3 == initiator + target + LOS, 4 == initiator + target + LOS + tickhistory")]
		public static int melee_protection = 4;

		[ServerVar]
		[Help("violation penalty to hand out when melee hack is detected")]
		public static float melee_penalty = 0f;

		[Help("melee distance forgiveness in percent, lower value = more false positives")]
		[ServerVar]
		public static float melee_forgiveness = 0.5f;

		[ServerVar]
		[Help("melee server frames to include in delay, lower value = more false positives")]
		public static float melee_serverframes = 2f;

		[ServerVar]
		[Help("melee client frames to include in delay, lower value = more false positives")]
		public static float melee_clientframes = 2f;

		[Help("whether or not to include terrain in the melee LOS checks")]
		[ServerVar]
		public static bool melee_terraincheck = true;

		[Help("0 == disabled, 1 == distance, 2 == distance + LOS")]
		[ServerVar]
		public static int eye_protection = 2;

		[Help("violation penalty to hand out when eye hack is detected")]
		[ServerVar]
		public static float eye_penalty = 0f;

		[Help("eye speed forgiveness in percent, lower value = more false positives")]
		[ServerVar]
		public static float eye_forgiveness = 0.5f;

		[ServerVar]
		[Help("eye server frames to include in delay, lower value = more false positives")]
		public static float eye_serverframes = 2f;

		[ServerVar]
		[Help("eye client frames to include in delay, lower value = more false positives")]
		public static float eye_clientframes = 2f;

		[ServerVar]
		[Help("whether or not to include terrain in the eye LOS checks")]
		public static bool eye_terraincheck = true;

		[ServerVar]
		[Help("whether or not to include terrain in the build LOS checks")]
		public static bool build_terraincheck = true;

		[ServerVar]
		[Help("0 == silent, 1 == print max violation, 2 == print nonzero violation, 3 == print any violation")]
		public static int debuglevel = 1;
	}
}
