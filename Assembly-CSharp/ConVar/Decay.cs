namespace ConVar;

[Factory("decay")]
public class Decay : ConsoleSystem
{
	[ServerVar(Help = "Maximum distance to test to see if a structure is outside, higher values are slower but accurate for huge buildings")]
	public static float outside_test_range = 50f;

	[ServerVar]
	public static float tick = 600f;

	[ServerVar]
	public static float scale = 1f;

	[ServerVar]
	public static bool debug = false;

	[ServerVar(Help = "Is upkeep enabled")]
	public static bool upkeep = true;

	[ServerVar(Help = "How many minutes does the upkeep cost last? default : 1440 (24 hours)")]
	public static float upkeep_period_minutes = 1440f;

	[ServerVar(Help = "How many minutes can the upkeep cost last after the cupboard was destroyed? default : 1440 (24 hours)")]
	public static float upkeep_grief_protection = 1440f;

	[ServerVar(Help = "Scale at which objects heal when upkeep conditions are met, default of 1 is same rate at which they decay")]
	public static float upkeep_heal_scale = 1f;

	[ServerVar(Help = "Scale at which objects decay when they are inside, default of 0.1")]
	public static float upkeep_inside_decay_scale = 0.1f;

	[ServerVar(Help = "When set to a value above 0 everything will decay with this delay")]
	public static float delay_override = 0f;

	[ServerVar(Help = "How long should this building grade decay be delayed when not protected by upkeep, in hours")]
	public static float delay_twig = 0f;

	[ServerVar(Help = "How long should this building grade decay be delayed when not protected by upkeep, in hours")]
	public static float delay_wood = 0f;

	[ServerVar(Help = "How long should this building grade decay be delayed when not protected by upkeep, in hours")]
	public static float delay_stone = 0f;

	[ServerVar(Help = "How long should this building grade decay be delayed when not protected by upkeep, in hours")]
	public static float delay_metal = 0f;

	[ServerVar(Help = "How long should this building grade decay be delayed when not protected by upkeep, in hours")]
	public static float delay_toptier = 0f;

	[ServerVar(Help = "When set to a value above 0 everything will decay with this duration")]
	public static float duration_override = 0f;

	[ServerVar(Help = "How long should this building grade take to decay when not protected by upkeep, in hours")]
	public static float duration_twig = 1f;

	[ServerVar(Help = "How long should this building grade take to decay when not protected by upkeep, in hours")]
	public static float duration_wood = 3f;

	[ServerVar(Help = "How long should this building grade take to decay when not protected by upkeep, in hours")]
	public static float duration_stone = 5f;

	[ServerVar(Help = "How long should this building grade take to decay when not protected by upkeep, in hours")]
	public static float duration_metal = 8f;

	[ServerVar(Help = "How long should this building grade take to decay when not protected by upkeep, in hours")]
	public static float duration_toptier = 12f;

	[ServerVar(Help = "Between 0 and this value are considered bracket 0 and will cost bracket_0_costfraction per upkeep period to maintain")]
	public static int bracket_0_blockcount = 15;

	[ServerVar(Help = "blocks within bracket 0 will cost this fraction per upkeep period to maintain")]
	public static float bracket_0_costfraction = 0.1f;

	[ServerVar(Help = "Between bracket_0_blockcount and this value are considered bracket 1 and will cost bracket_1_costfraction per upkeep period to maintain")]
	public static int bracket_1_blockcount = 50;

	[ServerVar(Help = "blocks within bracket 1 will cost this fraction per upkeep period to maintain")]
	public static float bracket_1_costfraction = 0.15f;

	[ServerVar(Help = "Between bracket_1_blockcount and this value are considered bracket 2 and will cost bracket_2_costfraction per upkeep period to maintain")]
	public static int bracket_2_blockcount = 125;

	[ServerVar(Help = "blocks within bracket 2 will cost this fraction per upkeep period to maintain")]
	public static float bracket_2_costfraction = 0.2f;

	[ServerVar(Help = "Between bracket_2_blockcount and this value (and beyond) are considered bracket 3 and will cost bracket_3_costfraction per upkeep period to maintain")]
	public static int bracket_3_blockcount = 200;

	[ServerVar(Help = "blocks within bracket 3 will cost this fraction per upkeep period to maintain")]
	public static float bracket_3_costfraction = 0.333f;
}
