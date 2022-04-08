namespace ConVar;

[Factory("batching")]
public class Batching : ConsoleSystem
{
	[ClientVar]
	public static bool renderers = true;

	[ClientVar]
	public static bool renderer_threading = true;

	[ClientVar]
	public static int renderer_capacity = 30000;

	[ClientVar]
	public static int renderer_vertices = 1000;

	[ClientVar]
	public static int renderer_submeshes = 1;

	[ServerVar]
	[ClientVar]
	public static int verbose = 0;
}
