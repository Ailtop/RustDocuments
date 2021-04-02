namespace ConVar
{
	[Factory("vis")]
	public class Vis : ConsoleSystem
	{
		[ClientVar]
		[Help("Turns on debug display of lerp")]
		public static bool lerp;

		[ServerVar]
		[Help("Turns on debug display of damages")]
		public static bool damage;

		[Help("Turns on debug display of attacks")]
		[ServerVar]
		[ClientVar]
		public static bool attack;

		[Help("Turns on debug display of protection")]
		[ClientVar]
		[ServerVar]
		public static bool protection;

		[Help("Turns on debug display of weakspots")]
		[ServerVar]
		public static bool weakspots;

		[ServerVar]
		[Help("Show trigger entries")]
		public static bool triggers;

		[Help("Turns on debug display of hitboxes")]
		[ServerVar]
		public static bool hitboxes;

		[Help("Turns on debug display of line of sight checks")]
		[ServerVar]
		public static bool lineofsight;

		[ServerVar]
		[Help("Turns on debug display of senses, which are received by Ai")]
		public static bool sense;
	}
}
