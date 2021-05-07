using Characters.Abilities.Customs;
using Characters.Gear.Weapons.Gauges;
using UnityEngine;

namespace Characters.Abilities
{
	public class DarkPaladinPassiveAttacher : AbilityAttacher
	{
		[SerializeField]
		private DarkPaladinPassive _passive;

		[SerializeField]
		private ValueGauge _gauge;

		public override void OnIntialize()
		{
			_passive.owner = base.owner;
			_passive.Initialize();
		}

		public override void StartAttach()
		{
			_gauge.onChanged += OnGaugeChanged;
		}

		public override void StopAttach()
		{
			_gauge.onChanged -= OnGaugeChanged;
			if (!(base.owner == null))
			{
				base.owner.ability.Remove(_passive);
			}
		}

		private void OnGaugeChanged(float oldValue, float newValue)
		{
			if (newValue == _gauge.maxValue)
			{
				_gauge.Clear();
				base.owner.ability.Add(_passive);
			}
		}

		public override string ToString()
		{
			return this.GetAutoName();
		}
	}
}
