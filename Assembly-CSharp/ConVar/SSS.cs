namespace ConVar;

[Factory("SSS")]
public class SSS : ConsoleSystem
{
	[ClientVar(Saved = true)]
	public static bool enabled = true;

	[ClientVar(Saved = true)]
	public static int quality = 0;

	[ClientVar(Saved = true)]
	public static bool halfres = true;

	[ClientVar(Saved = true)]
	public static float scale = 1f;
}
