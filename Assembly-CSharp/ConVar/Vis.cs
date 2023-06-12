namespace ConVar;

[Factory("vis")]
public class Vis : ConsoleSystem
{
	[Help("Turns on debug display of lerp")]
	[ClientVar]
	public static bool lerp;

	[ServerVar]
	[Help("Turns on debug display of damages")]
	public static bool damage;

	[ServerVar]
	[ClientVar]
	[Help("Turns on debug display of attacks")]
	public static bool attack;

	[ServerVar]
	[ClientVar]
	[Help("Turns on debug display of protection")]
	public static bool protection;

	[Help("Turns on debug display of weakspots")]
	[ServerVar]
	public static bool weakspots;

	[Help("Show trigger entries")]
	[ServerVar]
	public static bool triggers;

	[Help("Turns on debug display of hitboxes")]
	[ServerVar]
	public static bool hitboxes;

	[ServerVar]
	[Help("Turns on debug display of line of sight checks")]
	public static bool lineofsight;

	[Help("Turns on debug display of senses, which are received by Ai")]
	[ServerVar]
	public static bool sense;
}
