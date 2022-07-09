namespace ConVar;

[Factory("harmony")]
public class Harmony : ConsoleSystem
{
	[ServerVar(Name = "load")]
	public static void Load(Arg args)
	{
		HarmonyLoader.TryLoadMod(args.GetString(0));
	}

	[ServerVar(Name = "unload")]
	public static void Unload(Arg args)
	{
		HarmonyLoader.TryUnloadMod(args.GetString(0));
	}
}
