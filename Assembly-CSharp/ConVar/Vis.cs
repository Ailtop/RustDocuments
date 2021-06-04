namespace ConVar
{
	[Factory("vis")]
	public class Vis : ConsoleSystem
	{
		[Help("Turns on debug display of lerp")]
		[ClientVar]
		public static bool lerp;

		[Help("Turns on debug display of damages")]
		[ServerVar]
		public static bool damage;

		[ClientVar]
		[Help("Turns on debug display of attacks")]
		[ServerVar]
		public static bool attack;

		[ServerVar]
		[ClientVar]
		[Help("Turns on debug display of protection")]
		public static bool protection;

		[ServerVar]
		[Help("Turns on debug display of weakspots")]
		public static bool weakspots;

		[ServerVar]
		[Help("Show trigger entries")]
		public static bool triggers;

		[Help("Turns on debug display of hitboxes")]
		[ServerVar]
		public static bool hitboxes;

		[ServerVar]
		[Help("Turns on debug display of line of sight checks")]
		public static bool lineofsight;

		[ServerVar]
		[Help("Turns on debug display of senses, which are received by Ai")]
		public static bool sense;
	}
}
