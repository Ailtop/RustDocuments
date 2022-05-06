namespace ConVar;

[Factory("ai")]
public class AI : ConsoleSystem
{
	[ReplicatedVar(Saved = true)]
	public static bool allowdesigning = true;

	[ServerVar]
	public static bool think = true;

	[ServerVar]
	public static bool navthink = true;

	[ServerVar]
	public static bool ignoreplayers = false;

	[ServerVar]
	public static bool groups = true;

	[ServerVar]
	public static bool spliceupdates = true;

	[ServerVar]
	public static bool setdestinationsamplenavmesh = true;

	[ServerVar]
	public static bool usecalculatepath = true;

	[ServerVar]
	public static bool usesetdestinationfallback = true;

	[ServerVar]
	public static bool npcswimming = true;

	[ServerVar]
	public static bool accuratevisiondistance = true;

	[ServerVar]
	public static bool move = true;

	[ServerVar]
	public static bool usegrid = true;

	[ServerVar]
	public static bool sleepwake = true;

	[ServerVar]
	public static float sensetime = 1f;

	[ServerVar]
	public static float frametime = 5f;

	[ServerVar]
	public static int ocean_patrol_path_iterations = 100000;

	[ServerVar(Help = "If npc_enable is set to false then npcs won't spawn. (default: true)")]
	public static bool npc_enable = true;

	[ServerVar(Help = "npc_max_population_military_tunnels defines the size of the npc population at military tunnels. (default: 3)")]
	public static int npc_max_population_military_tunnels = 3;

	[ServerVar(Help = "npc_spawn_per_tick_max_military_tunnels defines how many can maximum spawn at once at military tunnels. (default: 1)")]
	public static int npc_spawn_per_tick_max_military_tunnels = 1;

	[ServerVar(Help = "npc_spawn_per_tick_min_military_tunnels defineshow many will minimum spawn at once at military tunnels. (default: 1)")]
	public static int npc_spawn_per_tick_min_military_tunnels = 1;

	[ServerVar(Help = "npc_respawn_delay_max_military_tunnels defines the maximum delay between spawn ticks at military tunnels. (default: 1920)")]
	public static float npc_respawn_delay_max_military_tunnels = 1920f;

	[ServerVar(Help = "npc_respawn_delay_min_military_tunnels defines the minimum delay between spawn ticks at military tunnels. (default: 480)")]
	public static float npc_respawn_delay_min_military_tunnels = 480f;

	[ServerVar(Help = "npc_valid_aim_cone defines how close their aim needs to be on target in order to fire. (default: 0.8)")]
	public static float npc_valid_aim_cone = 0.8f;

	[ServerVar(Help = "npc_valid_mounted_aim_cone defines how close their aim needs to be on target in order to fire while mounted. (default: 0.92)")]
	public static float npc_valid_mounted_aim_cone = 0.92f;

	[ServerVar(Help = "npc_cover_compromised_cooldown defines how long a cover point is marked as compromised before it's cleared again for selection. (default: 10)")]
	public static float npc_cover_compromised_cooldown = 10f;

	[ServerVar(Help = "If npc_cover_use_path_distance is set to true then npcs will look at the distance between the cover point and their target using the path between the two, rather than the straight-line distance.")]
	public static bool npc_cover_use_path_distance = true;

	[ServerVar(Help = "npc_cover_path_vs_straight_dist_max_diff defines what the maximum difference between straight-line distance and path distance can be when evaluating cover points. (default: 2)")]
	public static float npc_cover_path_vs_straight_dist_max_diff = 2f;

	[ServerVar(Help = "npc_door_trigger_size defines the size of the trigger box on doors that opens the door as npcs walk close to it (default: 1.5)")]
	public static float npc_door_trigger_size = 1.5f;

	[ServerVar(Help = "npc_patrol_point_cooldown defines the cooldown time on a patrol point until it's available again (default: 5)")]
	public static float npc_patrol_point_cooldown = 5f;

	[ServerVar(Help = "npc_speed_walk define the speed of an npc when in the walk state, and should be a number between 0 and 1. (Default: 0.18)")]
	public static float npc_speed_walk = 0.18f;

	[ServerVar(Help = "npc_speed_walk define the speed of an npc when in the run state, and should be a number between 0 and 1. (Default: 0.4)")]
	public static float npc_speed_run = 0.4f;

	[ServerVar(Help = "npc_speed_walk define the speed of an npc when in the sprint state, and should be a number between 0 and 1. (Default: 1.0)")]
	public static float npc_speed_sprint = 1f;

	[ServerVar(Help = "npc_speed_walk define the speed of an npc when in the crouched walk state, and should be a number between 0 and 1. (Default: 0.1)")]
	public static float npc_speed_crouch_walk = 0.1f;

	[ServerVar(Help = "npc_speed_crouch_run define the speed of an npc when in the crouched run state, and should be a number between 0 and 1. (Default: 0.25)")]
	public static float npc_speed_crouch_run = 0.25f;

	[ServerVar(Help = "npc_alertness_drain_rate define the rate at which we drain the alertness level of an NPC when there are no enemies in sight. (Default: 0.01)")]
	public static float npc_alertness_drain_rate = 0.01f;

	[ServerVar(Help = "npc_alertness_zero_detection_mod define the threshold of visibility required to detect an enemy when alertness is zero. (Default: 0.5)")]
	public static float npc_alertness_zero_detection_mod = 0.5f;

	[ServerVar(Help = "defines the chance for scientists to spawn at NPC junkpiles. (Default: 0.1)")]
	public static float npc_junkpilespawn_chance = 0.1f;

	[ServerVar(Help = "npc_junkpile_a_spawn_chance define the chance for scientists to spawn at junkpile a. (Default: 0.1)")]
	public static float npc_junkpile_a_spawn_chance = 0.1f;

	[ServerVar(Help = "npc_junkpile_g_spawn_chance define the chance for scientists to spawn at junkpile g. (Default: 0.1)")]
	public static float npc_junkpile_g_spawn_chance = 0.1f;

	[ServerVar(Help = "npc_junkpile_dist_aggro_gate define at what range (or closer) a junkpile scientist will get aggressive. (Default: 8)")]
	public static float npc_junkpile_dist_aggro_gate = 8f;

	[ServerVar(Help = "npc_max_junkpile_count define how many npcs can spawn into the world at junkpiles at the same time (does not include monuments) (Default: 30)")]
	public static int npc_max_junkpile_count = 30;

	[ServerVar(Help = "If npc_families_no_hurt is true, npcs of the same family won't be able to hurt each other. (default: true)")]
	public static bool npc_families_no_hurt = true;

	[ServerVar(Help = "If npc_ignore_chairs is true, npcs won't care about seeking out and sitting in chairs. (default: true)")]
	public static bool npc_ignore_chairs = true;

	[ServerVar(Help = "The rate at which we tick the sensory system. Minimum value is 1, as it multiplies with the tick-rate of the fixed AI tick rate of 0.1 (Default: 5)")]
	public static float npc_sensory_system_tick_rate_multiplier = 5f;

	[ServerVar(Help = "The rate at which we gather information about available cover points. Minimum value is 1, as it multiplies with the tick-rate of the fixed AI tick rate of 0.1 (Default: 20)")]
	public static float npc_cover_info_tick_rate_multiplier = 20f;

	[ServerVar(Help = "The rate at which we tick the reasoning system. Minimum value is 1, as it multiplies with the tick-rate of the fixed AI tick rate of 0.1 (Default: 1)")]
	public static float npc_reasoning_system_tick_rate_multiplier = 1f;

	[ServerVar(Help = "If animal_ignore_food is true, animals will not sense food sources or interact with them (server optimization). (default: true)")]
	public static bool animal_ignore_food = true;

	[ServerVar(Help = "The modifier by which a silencer reduce the noise that a gun makes when shot. (Default: 0.15)")]
	public static float npc_gun_noise_silencer_modifier = 0.15f;

	[ServerVar(Help = "If nav_carve_use_building_optimization is true, we attempt to reduce the amount of navmesh carves for a building. (default: false)")]
	public static bool nav_carve_use_building_optimization = false;

	[ServerVar(Help = "The minimum number of building blocks a building needs to consist of for this optimization to be applied. (default: 25)")]
	public static int nav_carve_min_building_blocks_to_apply_optimization = 25;

	[ServerVar(Help = "The minimum size we allow a carving volume to be. (default: 2)")]
	public static float nav_carve_min_base_size = 2f;

	[ServerVar(Help = "The size multiplier applied to the size of the carve volume. The smaller the value, the tighter the skirt around foundation edges, but too small and animals can attack through walls. (default: 4)")]
	public static float nav_carve_size_multiplier = 4f;

	[ServerVar(Help = "The height of the carve volume. (default: 2)")]
	public static float nav_carve_height = 2f;

	[ServerVar(Help = "If npc_only_hurt_active_target_in_safezone is true, npcs won't any player other than their actively targeted player when in a safe zone. (default: true)")]
	public static bool npc_only_hurt_active_target_in_safezone = true;

	[ServerVar(Help = "If npc_use_new_aim_system is true, npcs will miss on purpose on occasion, where the old system would randomize aim cone. (default: true)")]
	public static bool npc_use_new_aim_system = true;

	[ServerVar(Help = "If npc_use_thrown_weapons is true, npcs will throw grenades, etc. This is an experimental feature. (default: true)")]
	public static bool npc_use_thrown_weapons = true;

	[ServerVar(Help = "This is multiplied with the max roam range stat of an NPC to determine how far from its spawn point the NPC is allowed to roam. (default: 3)")]
	public static float npc_max_roam_multiplier = 3f;

	[ServerVar(Help = "This is multiplied with the current alertness (0-10) to decide how long it will take for the NPC to deliberately miss again. (default: 0.33)")]
	public static float npc_alertness_to_aim_modifier = 0.5f;

	[ServerVar(Help = "The time it takes for the NPC to deliberately miss to the time the NPC tries to hit its target. (default: 1.5)")]
	public static float npc_deliberate_miss_to_hit_alignment_time = 1.5f;

	[ServerVar(Help = "The offset with which the NPC will maximum miss the target. (default: 1.25)")]
	public static float npc_deliberate_miss_offset_multiplier = 1.25f;

	[ServerVar(Help = "The percentage away from a maximum miss the randomizer is allowed to travel when shooting to deliberately hit the target (we don't want perfect hits with every shot). (default: 0.85f)")]
	public static float npc_deliberate_hit_randomizer = 0.85f;

	[ServerVar(Help = "Baseline damage modifier for the new HTN Player NPCs to nerf their damage compared to the old NPCs. (default: 1.15f)")]
	public static float npc_htn_player_base_damage_modifier = 1.15f;

	[ServerVar(Help = "Spawn NPCs on the Cargo Ship. (default: true)")]
	public static bool npc_spawn_on_cargo_ship = true;

	[ServerVar(Help = "npc_htn_player_frustration_threshold defines where the frustration threshold for NPCs go, where they have the opportunity to change to a more aggressive tactic. (default: 3)")]
	public static int npc_htn_player_frustration_threshold = 3;

	[ServerVar]
	public static float tickrate = 5f;

	[ServerVar]
	public static void sleepwakestats(Arg args)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (AIInformationZone zone in AIInformationZone.zones)
		{
			if (!(zone == null) && zone.ShouldSleepAI)
			{
				num++;
				if (zone.Sleeping)
				{
					num2++;
					num3 += zone.SleepingCount;
				}
			}
		}
		args.ReplyWith("Sleeping AIZs: " + num2 + " / " + num + ". Total sleeping ents: " + num3);
	}

	[ServerVar]
	public static void wakesleepingai(Arg args)
	{
		int num = 0;
		int num2 = 0;
		foreach (AIInformationZone zone in AIInformationZone.zones)
		{
			if (!(zone == null) && zone.ShouldSleepAI && zone.Sleeping)
			{
				num++;
				num2 += zone.SleepingCount;
				zone.WakeAI();
			}
		}
		args.ReplyWith("Woke " + num + " sleeping AIZs containing " + num2 + " sleeping entities.");
	}

	[ServerVar]
	public static void brainstats(Arg args)
	{
		args.ReplyWith("Animal: " + AnimalBrain.Count + ". Scientist: " + ScientistBrain.Count + ". Pet: " + PetBrain.Count + ". Total: " + (AnimalBrain.Count + ScientistBrain.Count + PetBrain.Count));
	}

	[ServerVar]
	public static void killscientists(Arg args)
	{
		ScientistNPC[] array = BaseEntity.Util.FindScientists();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Kill();
		}
	}

	public static float TickDelta()
	{
		return 1f / tickrate;
	}

	[ServerVar]
	public static void selectNPCLookatServer(Arg args)
	{
	}
}
