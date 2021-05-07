using System;
using UnityEngine;

namespace Characters.Actions.Cooldowns
{
	public class Gauge : Cooldown
	{
		[SerializeField]
		protected float _maxGauge;

		[SerializeField]
		protected float _minGaugeToUse;

		[SerializeField]
		protected float _recoveryPerSecond;

		protected float _gauge;

		public float gauge
		{
			get
			{
				return _gauge;
			}
			set
			{
				if (value > _maxGauge)
				{
					_gauge = _maxGauge;
				}
				_gauge = value;
			}
		}

		public override float remainPercent
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override bool canUse => gauge >= _minGaugeToUse;

		internal override bool Consume()
		{
			if (canUse)
			{
				_gauge -= _minGaugeToUse;
				return true;
			}
			return false;
		}

		private void Update()
		{
			gauge += _recoveryPerSecond * _character.chronometer.master.deltaTime;
		}
	}
}
