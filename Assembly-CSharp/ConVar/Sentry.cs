namespace ConVar;

[Factory("sentry")]
public class Sentry : ConsoleSystem
{
	[ServerVar(Help = "target everyone regardless of authorization")]
	public static bool targetall = false;

	[ServerVar(Help = "how long until something is considered hostile after it attacked")]
	public static float hostileduration = 120f;
}
