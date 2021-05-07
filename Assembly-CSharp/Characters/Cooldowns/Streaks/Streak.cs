using System.Collections;

namespace Characters.Cooldowns.Streaks
{
	public class Streak : IStreak
	{
		private float _remainTime;

		private CoroutineReference _update;

		public int count { get; set; }

		public float timeout { get; set; }

		public int remains { get; private set; }

		public float remainPercent => _remainTime / timeout;

		public Streak(int count, float timeout)
		{
			this.count = count;
			this.timeout = timeout;
		}

		public bool Consume()
		{
			if (remains > 0)
			{
				remains--;
				return true;
			}
			return false;
		}

		public void Start()
		{
			if (count != 0)
			{
				_update.Stop();
				_update = CoroutineProxy.instance.StartCoroutineWithReference(CUpdate());
			}
		}

		private IEnumerator CUpdate()
		{
			remains = count;
			_remainTime = timeout;
			Chronometer.Global chronometer = Chronometer.global;
			while (_remainTime > 0f)
			{
				yield return null;
				_remainTime -= chronometer.deltaTime;
			}
			remains = 0;
		}

		public void Expire()
		{
			_update.Stop();
			remains = 0;
			_remainTime = 0f;
		}
	}
}
