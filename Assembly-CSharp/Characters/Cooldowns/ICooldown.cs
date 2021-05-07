using System;
using Characters.Cooldowns.Streaks;

namespace Characters.Cooldowns
{
	public interface ICooldown
	{
		int maxStack { get; }

		int stacks { get; set; }

		bool canUse { get; }

		float remainPercent { get; }

		IStreak streak { get; }

		event Action onReady;

		bool Consume();
	}
}
