using System;
using Characters.Cooldowns.Streaks;

namespace Characters.Cooldowns
{
	public class Custom : ICooldown
	{
		private readonly NoStreak _streak = new NoStreak();

		public int maxStack => int.MaxValue;

		public int streakCount => 0;

		public float streakTimeout => 0f;

		public int streakRemains => 0;

		public int stacks { get; set; }

		public float remainPercent => 0f;

		public float streakRemainPercent => 0f;

		public bool canUse => stacks > 0;

		public IStreak streak => _streak;

		public event Action onReady;

		public bool Consume()
		{
			if (stacks == 0)
			{
				return false;
			}
			stacks--;
			return true;
		}

		public void ExpireStreak()
		{
		}
	}
}
