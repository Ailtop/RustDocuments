namespace ConVar;

[Factory("halloween")]
public class Halloween : ConsoleSystem
{
	[ServerVar]
	public static bool enabled = false;

	[ServerVar(Help = "Population active on the server, per square km")]
	public static float murdererpopulation = 0f;

	[ServerVar(Help = "Population active on the server, per square km")]
	public static float scarecrowpopulation = 0f;

	[ServerVar(Help = "Scarecrows can throw beancans (Default: true).")]
	public static bool scarecrows_throw_beancans = true;

	[ServerVar(Help = "The delay globally on a server between each time a scarecrow throws a beancan (Default: 8 seconds).")]
	public static float scarecrow_throw_beancan_global_delay = 8f;

	[ServerVar(Help = "Modified damage from beancan explosion vs players (Default: 0.1).")]
	public static float scarecrow_beancan_vs_player_dmg_modifier = 0.1f;

	[ServerVar(Help = "Modifier to how much damage scarecrows take to the body. (Default: 0.25)")]
	public static float scarecrow_body_dmg_modifier = 0.25f;

	[ServerVar(Help = "Stopping distance for destinations set while chasing a target (Default: 0.5)")]
	public static float scarecrow_chase_stopping_distance = 0.5f;
}
