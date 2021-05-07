using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Gear.Weapons;
using Characters.Operations;
using Characters.Player;
using FX.BoundsAttackVisualEffect;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class BoneOfSpeed : Ability
	{
		public class Instance : AbilityInstance<BoneOfSpeed>
		{
			private WeaponInventory _weaponInventory;

			private int _speedHeadCount;

			private float _remainCooldown;

			public override int iconStacks => _speedHeadCount;

			public Instance(Character owner, BoneOfSpeed ability)
				: base(owner, ability)
			{
				_weaponInventory = owner.playerComponents.inventory.weapon;
				UpdateSpeedHeadCount();
			}

			protected override void OnAttach()
			{
				Character character = owner;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(character.onGaveDamage, new GaveDamageDelegate(OnOwnerGaveDamage));
				_weaponInventory.onChanged += OnWeaponChanged;
				_remainCooldown = ability._cooldownTime;
			}

			protected override void OnDetach()
			{
				Character character = owner;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Remove(character.onGaveDamage, new GaveDamageDelegate(OnOwnerGaveDamage));
				_weaponInventory.onChanged -= OnWeaponChanged;
			}

			public override void UpdateTime(float deltaTime)
			{
				base.UpdateTime(deltaTime);
				_remainCooldown -= deltaTime;
			}

			private void OnOwnerGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage gaveDamage, double damageDealt)
			{
				if (gaveDamage.critical && ability._motionTypeFilter[gaveDamage.motionType] && ability._damageTypeFilter[gaveDamage.attackType] && !(_remainCooldown > 0f))
				{
					_remainCooldown = ability._cooldownTime;
					Damage damage = owner.stat.GetDamage(ability._baseDamage * ability._damagePercents[_speedHeadCount], MMMaths.RandomPointWithinBounds(target.collider.bounds), ability._hitInfo);
					owner.Attack(target, ref damage);
					ability._hitEffect.Spawn(owner, target.collider.bounds, ref damage, target);
				}
			}

			private void OnWeaponChanged(Weapon old, Weapon @new)
			{
				UpdateSpeedHeadCount();
			}

			private void UpdateSpeedHeadCount()
			{
				_speedHeadCount = _weaponInventory.GetCountByCategory(Weapon.Category.Speed);
			}
		}

		[SerializeField]
		private MotionTypeBoolArray _motionTypeFilter;

		[SerializeField]
		private AttackTypeBoolArray _damageTypeFilter;

		[SerializeField]
		private HitInfo _hitInfo;

		[SerializeField]
		private double _baseDamage = 10.0;

		[SerializeField]
		private float _cooldownTime = 0.5f;

		[Tooltip("스피드 타입 스컬 개수가 0개일 때 피해량 증폭")]
		[SerializeField]
		private double _damagePercent0 = 1.0;

		[Tooltip("스피드 타입 스컬 개수가 1개일 때 피해량 증폭")]
		[SerializeField]
		private double _damagePercent1 = 1.3;

		[Tooltip("스피드 타입 스컬 개수가 2개일 때 피해량 증폭")]
		[SerializeField]
		private double _damagePercent2 = 2.0;

		[SerializeField]
		[BoundsAttackVisualEffect.Subcomponent]
		private BoundsAttackVisualEffect.Subcomponents _hitEffect;

		private double[] _damagePercents;

		public BoneOfSpeed()
		{
			_damagePercents = new double[3] { _damagePercent0, _damagePercent1, _damagePercent2 };
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
