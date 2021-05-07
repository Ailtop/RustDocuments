using System;
using Characters.Cooldowns.Streaks;

namespace Characters.Cooldowns
{
	public class None : ICooldown
	{
		private readonly NoStreak _streak = new NoStreak();

		public int maxStack => 1;

		public int streakCount => 0;

		public float streakTimeout => 0f;

		public int streakRemains => 0;

		public int stacks
		{
			get
			{
				return 1;
			}
			set
			{
			}
		}

		public float remainPercent => 0f;

		public float streakRemainPercent => 0f;

		public bool canUse => true;

		public IStreak streak => _streak;

		public event Action onReady;

		public bool Consume()
		{
			return true;
		}

		public void ExpireStreak()
		{
		}
	}
}
