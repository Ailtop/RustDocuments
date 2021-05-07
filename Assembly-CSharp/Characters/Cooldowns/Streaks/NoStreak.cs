namespace Characters.Cooldowns.Streaks
{
	public class NoStreak : IStreak
	{
		public int count
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public float timeout
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public int remains => 0;

		public float remainPercent => 0f;

		public bool onStreak => false;

		public bool Consume()
		{
			return false;
		}

		public void Start()
		{
		}

		public void Expire()
		{
		}
	}
}
