using Characters.Gear.Weapons.Gauges;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	public class LivingArmor2PassiveAttacher : AbilityAttacher
	{
		[SerializeField]
		private ValueGauge _gauge;

		[Header("Passive Components")]
		[SerializeField]
		private LivingArmorPassiveComponent _livingArmorPassive;

		[SerializeField]
		private LivingArmorPassiveComponent _livingArmorPassive2;

		private float _targetValue;

		public override void OnIntialize()
		{
			_targetValue = _gauge.maxValue / 2f;
			_livingArmorPassive.Initialize();
			_livingArmorPassive2.Initialize();
		}

		public override void StartAttach()
		{
			_gauge.onChanged += OnGaugeValueChanged;
		}

		public override void StopAttach()
		{
			if (!(base.owner == null))
			{
				_gauge.onChanged -= OnGaugeValueChanged;
				base.owner.ability.Remove(_livingArmorPassive.ability);
				base.owner.ability.Remove(_livingArmorPassive2.ability);
			}
		}

		public override string ToString()
		{
			return this.GetAutoName();
		}

		private void OnGaugeValueChanged(float oldValue, float newValue)
		{
			if (!(oldValue > newValue))
			{
				if (_targetValue > oldValue && _targetValue <= newValue)
				{
					base.owner.ability.Add(_livingArmorPassive.ability);
				}
				if (newValue == _gauge.maxValue)
				{
					base.owner.ability.Remove(_livingArmorPassive.ability);
					base.owner.ability.Add(_livingArmorPassive2.ability);
				}
			}
		}
	}
}
