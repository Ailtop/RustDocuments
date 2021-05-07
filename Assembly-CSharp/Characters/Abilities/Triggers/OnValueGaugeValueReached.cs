using System;
using Characters.Gear.Weapons.Gauges;
using UnityEngine;

namespace Characters.Abilities.Triggers
{
	[Serializable]
	public class OnValueGaugeValueReached : Trigger
	{
		[SerializeField]
		private ValueGauge _gauge;

		[SerializeField]
		private int _targetValue;

		[SerializeField]
		private bool _clearGauge;

		private Character _character;

		public override void Attach(Character character)
		{
			_gauge.onChanged += OnGaugeValueChanged;
		}

		public override void Detach()
		{
			_gauge.onChanged -= OnGaugeValueChanged;
		}

		private void OnGaugeValueChanged(float oldValue, float newValue)
		{
			if (!(oldValue < (float)_targetValue) || !(newValue < (float)_targetValue))
			{
				if (_clearGauge)
				{
					_gauge.Clear();
				}
				Invoke();
			}
		}
	}
}
