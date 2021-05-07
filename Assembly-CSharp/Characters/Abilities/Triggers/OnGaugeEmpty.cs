using System;
using Characters.Gear.Weapons.Gauges;
using UnityEngine;

namespace Characters.Abilities.Triggers
{
	[Serializable]
	public class OnGaugeEmpty : Trigger
	{
		[SerializeField]
		private ValueGauge _gauge;

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
			if (newValue == 0f)
			{
				Invoke();
			}
		}
	}
}
