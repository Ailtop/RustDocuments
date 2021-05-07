using System;
using Characters.Gear.Weapons;
using Characters.Player;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class BoneOfBrave : Ability
	{
		public class Instance : AbilityInstance<BoneOfBrave>
		{
			private WeaponInventory _weaponInventory;

			private float _remainCooldownTime;

			private int _powerHeadCount;

			public override int iconStacks => _powerHeadCount;

			public override float iconFillAmount => _remainCooldownTime / ability._cooldownTime;

			public Instance(Character owner, BoneOfBrave ability)
				: base(owner, ability)
			{
				_weaponInventory = owner.playerComponents.inventory.weapon;
				UpdatePowerHeadCount();
			}

			protected override void OnAttach()
			{
				owner.onGiveDamage.Add(0, OnOwnerGiveDamage);
				_weaponInventory.onChanged += OnWeaponChanged;
			}

			protected override void OnDetach()
			{
				owner.onGiveDamage.Remove(OnOwnerGiveDamage);
				_weaponInventory.onChanged -= OnWeaponChanged;
			}

			public override void UpdateTime(float deltaTime)
			{
				base.UpdateTime(deltaTime);
				_remainCooldownTime -= deltaTime;
			}

			private bool OnOwnerGiveDamage(ITarget target, ref Damage damage)
			{
				if (_remainCooldownTime > 0f)
				{
					return false;
				}
				if (!ability._motionTypeFilter[damage.motionType])
				{
					return false;
				}
				if (!ability._damageTypeFilter[damage.attackType])
				{
					return false;
				}
				if (!ability._attributeFilter[damage.attribute])
				{
					return false;
				}
				damage.@base *= ability.damagePercents[_powerHeadCount];
				_remainCooldownTime = ability._cooldownTime;
				return false;
			}

			private void OnWeaponChanged(Weapon old, Weapon @new)
			{
				UpdatePowerHeadCount();
			}

			private void UpdatePowerHeadCount()
			{
				_powerHeadCount = _weaponInventory.GetCountByCategory(Weapon.Category.Power);
			}
		}

		[SerializeField]
		private MotionTypeBoolArray _motionTypeFilter;

		[SerializeField]
		private AttackTypeBoolArray _damageTypeFilter;

		[SerializeField]
		private DamageAttributeBoolArray _attributeFilter;

		[SerializeField]
		private float _cooldownTime = 5f;

		[Tooltip("파워 타입 스컬 개수가 0개일 때 피해량 증폭")]
		[SerializeField]
		private double _damagePercent0 = 1.5;

		[Tooltip("파워 타입 스컬 개수가 1개일 때 피해량 증폭")]
		[SerializeField]
		private double _damagePercent1 = 2.0;

		[Tooltip("파워 타입 스컬 개수가 2개일 때 피해량 증폭")]
		[SerializeField]
		private double _damagePercent2 = 3.0;

		private double[] damagePercents;

		public BoneOfBrave()
		{
			damagePercents = new double[3] { _damagePercent0, _damagePercent1, _damagePercent2 };
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
