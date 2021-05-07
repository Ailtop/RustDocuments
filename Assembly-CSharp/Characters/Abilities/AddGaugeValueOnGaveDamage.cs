using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Gear.Weapons.Gauges;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class AddGaugeValueOnGaveDamage : Ability
	{
		public class Instance : AbilityInstance<AddGaugeValueOnGaveDamage>
		{
			internal Instance(Character owner, AddGaugeValueOnGaveDamage ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				Character character = owner;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(character.onGaveDamage, new GaveDamageDelegate(OnGaveDamage));
			}

			protected override void OnDetach()
			{
				Character character = owner;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Remove(character.onGaveDamage, new GaveDamageDelegate(OnGaveDamage));
			}

			private void OnGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
			{
				if (!(target.character == null) && ability._attackTypes[tookDamage.motionType] && ability._types[tookDamage.attackType] && MMMaths.PercentChance(ability._chance))
				{
					float num = (tookDamage.critical ? ability._amountOnCritical : ability._amount);
					if (ability._multiplierByDamageDealt)
					{
						num *= (float)damageDealt;
					}
					ability._gauge.Add(num);
				}
			}
		}

		[Serializable]
		private class AttackTypeBoolArray : EnumArray<Damage.MotionType, bool>
		{
		}

		[Serializable]
		private class DamageTypeBoolArray : EnumArray<Damage.AttackType, bool>
		{
		}

		[SerializeField]
		private ValueGauge _gauge;

		[SerializeField]
		[Range(1f, 100f)]
		private int _chance = 100;

		[SerializeField]
		private int _amount = 1;

		[SerializeField]
		private int _amountOnCritical = 1;

		[SerializeField]
		private bool _multiplierByDamageDealt;

		[SerializeField]
		private AttackTypeBoolArray _attackTypes;

		[SerializeField]
		private DamageTypeBoolArray _types;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
