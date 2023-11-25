namespace ConVar;

[Factory("net")]
public class Net : ConsoleSystem
{
	[ServerVar]
	public static bool visdebug = false;

	[ClientVar]
	public static bool debug = false;

	[ServerVar]
	public static int visibilityRadiusFarOverride = -1;

	[ServerVar]
	public static int visibilityRadiusNearOverride = -1;

	[ServerVar(Name = "global_networked_bases")]
	public static bool globalNetworkedBases = true;

	[ServerVar(Help = "Toggle printing time taken to send all trees & all global entities to client when they connect")]
	public static bool global_network_debug = false;

	[ServerVar(Help = "(default) true = only broadcast to clients with global networking enabled, false = broadcast to every client regardless")]
	public static bool limit_global_update_broadcast = true;
}
