namespace ConVar;

[Factory("terrain")]
public class Terrain : ConsoleSystem
{
	[ClientVar(Saved = true)]
	public static float quality = 100f;
}
