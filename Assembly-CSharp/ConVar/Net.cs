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
}
