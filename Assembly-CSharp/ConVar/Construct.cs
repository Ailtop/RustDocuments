namespace ConVar;

[Factory("construct")]
public class Construct : ConsoleSystem
{
	[Help("How many minutes before a placed frame gets destroyed")]
	[ServerVar]
	public static float frameminutes = 30f;
}
