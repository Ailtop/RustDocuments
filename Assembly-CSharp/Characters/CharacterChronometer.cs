namespace Characters
{
	public class CharacterChronometer
	{
		public readonly Chronometer master;

		public readonly Chronometer effect;

		public readonly Chronometer projectile;

		public readonly Chronometer animation;

		public CharacterChronometer()
		{
			master = new Chronometer();
			effect = new Chronometer(master);
			projectile = new Chronometer(master);
			animation = new Chronometer(master);
		}
	}
}
