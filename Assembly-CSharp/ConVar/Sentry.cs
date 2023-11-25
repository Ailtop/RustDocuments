namespace ConVar;

[Factory("sentry")]
public class Sentry : ConsoleSystem
{
	[ServerVar(Help = "target everyone regardless of authorization")]
	public static bool targetall = false;

	[ServerVar(Help = "how long until something is considered hostile after it attacked")]
	public static float hostileduration = 120f;

	[ServerVar(Help = "radius to check for other turrets")]
	public static float interferenceradius = 40f;

	[ServerVar(Help = "max interference from other turrets")]
	public static float maxinterference = 12f;
}
