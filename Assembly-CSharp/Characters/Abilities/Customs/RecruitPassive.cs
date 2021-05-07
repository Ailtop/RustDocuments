using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Gear.Weapons.Gauges;
using Characters.Operations;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class RecruitPassive : Ability
	{
		public class Instance : AbilityInstance<RecruitPassive>
		{
			private Color _defaultGaugeColor;

			private float _remainSummoningTime;

			private float _remainSummoningInterval;

			public Instance(Character owner, RecruitPassive ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				Character character = owner;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(character.onGaveDamage, new GaveDamageDelegate(OnOwnerGaveDamage));
				ability._gauge.onChanged += OnGaugeChanged;
			}

			protected override void OnDetach()
			{
				Character character = owner;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Remove(character.onGaveDamage, new GaveDamageDelegate(OnOwnerGaveDamage));
				if (_remainSummoningTime > 0f)
				{
					ability._gauge.defaultBarColor = _defaultGaugeColor;
					ability._gauge.Clear();
				}
			}

			public override void UpdateTime(float deltaTime)
			{
				base.UpdateTime(deltaTime);
				if (_remainSummoningTime <= 0f)
				{
					if (ability._gauge.currentValue == ability._gauge.maxValue)
					{
						ability._gauge.defaultBarColor = _defaultGaugeColor;
						ability._gauge.Clear();
					}
					return;
				}
				_remainSummoningInterval -= deltaTime;
				_remainSummoningTime -= deltaTime;
				if (_remainSummoningInterval < 0f)
				{
					ability._summoningOperation.Run(owner);
					_remainSummoningInterval += ability._summoningInterval;
				}
			}

			private void OnOwnerGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage gaveDamage, double damageDealt)
			{
				if (string.IsNullOrEmpty(gaveDamage.key))
				{
					return;
				}
				for (int i = 0; i < ability._attackKeyAndGaugeAmounts.Length; i++)
				{
					GaugeAmountByAttackKey gaugeAmountByAttackKey = ability._attackKeyAndGaugeAmounts[i];
					if (string.Equals(gaugeAmountByAttackKey.attackKey, gaveDamage.key, StringComparison.OrdinalIgnoreCase))
					{
						ability._gauge.Add(gaugeAmountByAttackKey.gaugeAmountByAttack);
						break;
					}
				}
			}

			private void OnGaugeChanged(float oldValue, float newValue)
			{
				if (!(oldValue >= newValue) && !(newValue < ability._gauge.maxValue))
				{
					_defaultGaugeColor = ability._gauge.defaultBarColor;
					ability._gauge.defaultBarColor = ability._fullGaugeColor;
					_remainSummoningTime = ability._summoningDuration;
					_remainSummoningInterval = 0f;
				}
			}
		}

		[Serializable]
		private class GaugeAmountByAttackKey
		{
			public string attackKey;

			public int gaugeAmountByAttack;
		}

		[Header("Gauge")]
		[SerializeField]
		private ValueGauge _gauge;

		[SerializeField]
		private Color _fullGaugeColor;

		[SerializeField]
		private GaugeAmountByAttackKey[] _attackKeyAndGaugeAmounts;

		[Header("Summon")]
		[SerializeField]
		private float _summoningDuration;

		[SerializeField]
		private float _summoningInterval;

		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _summoningOperation;

		public override void Initialize()
		{
			base.Initialize();
			_summoningOperation.Initialize();
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
