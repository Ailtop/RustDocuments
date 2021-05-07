using System;
using System.Collections;
using Characters.Cooldowns.Streaks;

namespace Characters.Cooldowns
{
	public class Time : ICooldown
	{
		private int _stacks;

		private Action _onReady;

		public static readonly Func<float> GetDefaultCooldownSpeed = () => 1f;

		public Func<float> GetCooldownSpeed = GetDefaultCooldownSpeed;

		public readonly float cooldownTime;

		private CoroutineReference _updateReference;

		public float remainTime;

		public int stacks
		{
			get
			{
				return _stacks;
			}
			set
			{
				if (_stacks == 0 && value > 0 && _onReady != null)
				{
					_onReady();
				}
				_stacks = value;
			}
		}

		public int maxStack { get; protected set; }

		public bool canUse
		{
			get
			{
				if (stacks <= 0)
				{
					return streak.remains > 0;
				}
				return true;
			}
		}

		public IStreak streak { get; protected set; }

		public float remainPercent
		{
			get
			{
				if (streak.remains > 0)
				{
					return 0f;
				}
				if (stacks != maxStack)
				{
					return remainTime / cooldownTime;
				}
				return 0f;
			}
		}

		public event Action onReady
		{
			add
			{
				_onReady = (Action)Delegate.Combine(_onReady, value);
			}
			remove
			{
				_onReady = (Action)Delegate.Remove(_onReady, value);
			}
		}

		public Time(int maxStack, int streakCount, float streakTimeout, float cooldownTime)
		{
			this.cooldownTime = cooldownTime;
			this.maxStack = maxStack;
			streak = new Streak(streakCount, streakTimeout);
			_updateReference.Stop();
			_updateReference = CoroutineProxy.instance.StartCoroutineWithReference(CUpdate());
		}

		private void OnDestroy()
		{
			_updateReference.Stop();
		}

		private IEnumerator CUpdate()
		{
			while (true)
			{
				yield return null;
				if (stacks != maxStack && streak.remains <= 0)
				{
					if (remainTime > cooldownTime)
					{
						remainTime = cooldownTime;
					}
					remainTime -= Chronometer.global.deltaTime * GetCooldownSpeed();
					if (remainTime <= 0f)
					{
						remainTime = cooldownTime;
						stacks++;
					}
				}
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
			return false;
		}
	}
}
