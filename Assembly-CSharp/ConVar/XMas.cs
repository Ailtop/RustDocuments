namespace ConVar;

[Factory("xmas")]
public class XMas : ConsoleSystem
{
	private const string path = "assets/prefabs/misc/xmas/xmasrefill.prefab";

	[ServerVar]
	public static bool enabled = false;

	[ServerVar]
	public static float spawnRange = 40f;

	[ServerVar]
	public static int spawnAttempts = 5;

	[ServerVar]
	public static int giftsPerPlayer = 2;

	[ServerVar]
	public static void refill(Arg arg)
	{
		BaseEntity baseEntity = GameManager.server.CreateEntity("assets/prefabs/misc/xmas/xmasrefill.prefab");
		if ((bool)baseEntity)
		{
			baseEntity.Spawn();
		}
	}
}
