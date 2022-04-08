namespace ConVar;

[Factory("water")]
public class Water : ConsoleSystem
{
	[ClientVar(Saved = true)]
	public static int quality = 1;

	public static int MaxQuality = 2;

	public static int MinQuality = 0;

	[ClientVar(Saved = true)]
	public static int reflections = 1;

	public static int MaxReflections = 2;

	public static int MinReflections = 0;
}
