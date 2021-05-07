using System;
using Characters.Abilities.Constraints;
using Characters.Gear.Weapons.Gauges;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class AlchemistPassive : Ability
	{
		public class Instance : AbilityInstance<AlchemistPassive>
		{
			public Instance(Character owner, AlchemistPassive ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
			}

			protected override void OnDetach()
			{
			}

			public override void UpdateTime(float deltaTime)
			{
				base.UpdateTime(deltaTime);
				if (!ability._constraints.Pass())
				{
					return;
				}
				if (ability._deactivate.baseAbility.attached)
				{
					ability._valueGague.Clear();
					return;
				}
				float num = ability._gaugeAmountPerSecond * deltaTime;
				if (ability._multiplyGaugeAmountByCooldown)
				{
					num *= (float)owner.stat.GetFinal(Stat.Kind.SkillCooldownSpeed);
				}
				if (ability._boost.baseAbility.attached)
				{
					num *= (float)ability._boost.baseAbility.multiplier;
				}
				ability._valueGague.Add(num);
			}
		}

		[Header("Passive Settings")]
		[SerializeField]
		private ValueGauge _valueGague;

		[SerializeField]
		private float _gaugeAmountPerSecond;

		[SerializeField]
		private bool _multiplyGaugeAmountByCooldown;

		[Space]
		[SerializeField]
		private AlchemistGaugeBoostComponent _boost;

		[Space]
		[SerializeField]
		private AlchemistGaugeDeactivateComponent _deactivate;

		[Space]
		[SerializeField]
		[Constraint.Subcomponent]
		private Constraint.Subcomponents _constraints;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
