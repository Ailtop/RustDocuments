using System;
using Characters.Cooldowns.Streaks;
using Characters.Gear.Weapons.Gauges;

namespace Characters.Cooldowns
{
	public class Gauge : ICooldown
	{
		private ValueGauge _gauge;

		private int _requiredAmount;

		public int maxStack => 1;

		public int stacks
		{
			get
			{
				if (!_gauge.Has(_requiredAmount))
				{
					return 0;
				}
				return 1;
			}
			set
			{
			}
		}

		public bool canUse
		{
			get
			{
				if (streak.remains <= 0)
				{
					return _gauge.Has(_requiredAmount);
				}
				return true;
			}
		}

		public IStreak streak { get; private set; }

		public float remainPercent => 0f;

		public event Action onReady;

		public Gauge(ValueGauge gauge, int requiredAmount, int streakCount, float streakTimeout)
		{
			_gauge = gauge;
			_gauge.onChanged += OnChanged;
			_requiredAmount = requiredAmount;
			streak = new Streak(streakCount, streakTimeout);
		}

		private void OnChanged(float oldValue, float newValue)
		{
			if (oldValue < (float)_requiredAmount && newValue >= (float)_requiredAmount)
			{
				this.onReady?.Invoke();
			}
		}

		public bool Consume()
		{
			if (streak.Consume())
			{
				return true;
			}
			if (stacks > 0)
			{
				stacks--;
				streak.Start();
				return true;
			}
			return _gauge.Consume(_requiredAmount);
		}

		public void ExpireStreak()
		{
		}
	}
}
