using System.IO;

namespace ConVar;

[Factory("profile")]
public class Profile : ConsoleSystem
{
	private static void NeedProfileFolder()
	{
		if (!Directory.Exists("profile"))
		{
			Directory.CreateDirectory("profile");
		}
	}

	[ClientVar]
	[ServerVar]
	public static void start(Arg arg)
	{
	}

	[ServerVar]
	[ClientVar]
	public static void stop(Arg arg)
	{
	}

	[ServerVar]
	[ClientVar]
	public static void flush_analytics(Arg arg)
	{
	}
}
