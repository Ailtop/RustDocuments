using System.Collections;
using UnityEngine;

namespace Characters.Actions.Cooldowns
{
	public abstract class Basic : Cooldown
	{
		[SerializeField]
		protected int _maxStacks = 1;

		[SerializeField]
		protected int _streakCount;

		[SerializeField]
		protected float _streakTimeout;

		protected int _remainStreaks;

		protected float _remainStreaksTime;

		private Coroutine _streak;

		public override bool canUse
		{
			get
			{
				if (base.stacks <= 0)
				{
					return _remainStreaks > 0;
				}
				return true;
			}
		}

		public float remainStreaks => _remainStreaks;

		internal override bool Consume()
		{
			if (_remainStreaks > 0)
			{
				_remainStreaks--;
				return true;
			}
			if (base.stacks > 0)
			{
				base.stacks--;
				if (_streak != null)
				{
					StopCoroutine(_streak);
				}
				_streak = StartCoroutine(CStreak());
				return true;
			}
			return false;
		}

		protected override void Awake()
		{
			base.Awake();
			_stacks = _maxStacks;
		}

		private void OnDisable()
		{
			_remainStreaks = 0;
		}

		private IEnumerator CStreak()
		{
			_remainStreaks = _streakCount;
			_remainStreaksTime = _streakTimeout;
			Chronometer chronometer = _character.chronometer.master;
			while (_remainStreaksTime > 0f)
			{
				yield return null;
				_remainStreaksTime -= chronometer.deltaTime;
			}
			_remainStreaks = 0;
			_streak = null;
		}
	}
}
