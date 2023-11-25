namespace ConVar;

[Factory("construct")]
public class Construct : ConsoleSystem
{
	[ServerVar]
	[Help("How many minutes before a placed frame gets destroyed")]
	public static float frameminutes = 30f;
}
