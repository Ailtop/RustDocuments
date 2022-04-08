namespace ConVar;

[Factory("file")]
public class FileConVar : ConsoleSystem
{
	[ClientVar]
	public static bool debug
	{
		get
		{
			return FileSystem.LogDebug;
		}
		set
		{
			FileSystem.LogDebug = value;
		}
	}

	[ClientVar]
	public static bool time
	{
		get
		{
			return FileSystem.LogTime;
		}
		set
		{
			FileSystem.LogTime = value;
		}
	}
}
