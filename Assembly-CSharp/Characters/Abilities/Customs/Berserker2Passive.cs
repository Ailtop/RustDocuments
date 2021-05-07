using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Gear.Weapons;
using Characters.Gear.Weapons.Gauges;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class Berserker2Passive : Ability
	{
		public class Instance : AbilityInstance<Berserker2Passive>
		{
			public Instance(Character owner, Berserker2Passive ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				Character character = owner;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(character.onGaveDamage, new GaveDamageDelegate(OnOwnerGaveDamage));
				owner.health.onTookDamage += OnOwnerTookDamage;
			}

			protected override void OnDetach()
			{
				Character character = owner;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Remove(character.onGaveDamage, new GaveDamageDelegate(OnOwnerGaveDamage));
				owner.health.onTookDamage -= OnOwnerTookDamage;
			}

			public override void UpdateTime(float deltaTime)
			{
				base.UpdateTime(deltaTime);
				if (!owner.playerComponents.combatDetector.inCombat)
				{
					ability._gauge.Add((0f - ability._losingGaugeAmountPerSecond) * deltaTime);
				}
				CheckGaugeAndPolymorph();
			}

			private void CheckGaugeAndPolymorph()
			{
				if (!(ability._gauge.currentValue < ability._gauge.maxValue) && (!(owner.motion != null) || !owner.motion.running))
				{
					ability._gauge.Clear();
					double amount = owner.health.currentHealth * (double)ability._losingHealthPercentOnPolymorph * 0.01;
					owner.health.TakeHealth(amount);
					Singleton<Service>.Instance.floatingTextSpawner.SpawnPlayerTakingDamage(amount, owner.transform.position);
					owner.playerComponents.inventory.weapon.Polymorph(ability._polymorphWeapon);
				}
			}

			private void OnOwnerGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
			{
				if (!(target.character == null) && ability._attackTypeFilter[tookDamage.attackType] && ability._motionTypeFilter[tookDamage.motionType])
				{
					ability._gauge.Add(ability._gettingGaugeAmountByGaveDamage);
				}
			}

			private void OnOwnerTookDamage([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
			{
				if (ability._attackTypeFilter[tookDamage.attackType] && ability._motionTypeFilter[tookDamage.motionType])
				{
					ability._gauge.Add(ability._gettingGaugeAmountByTookDamage);
				}
			}
		}

		[Space]
		[SerializeField]
		[Range(0f, 99f)]
		private int _losingHealthPercentOnPolymorph;

		[Header("Gauge")]
		[SerializeField]
		private ValueGauge _gauge;

		[Space]
		[SerializeField]
		private float _gettingGaugeAmountByGaveDamage;

		[SerializeField]
		private float _gettingGaugeAmountByTookDamage;

		[Space]
		[SerializeField]
		private float _losingGaugeAmountPerSecond;

		[Header("Filter")]
		[SerializeField]
		private AttackTypeBoolArray _attackTypeFilter;

		[SerializeField]
		private MotionTypeBoolArray _motionTypeFilter;

		[NonSerialized]
		public Weapon _polymorphWeapon;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
