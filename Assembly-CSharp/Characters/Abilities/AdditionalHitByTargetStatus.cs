using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Operations;
using FX.BoundsAttackVisualEffect;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class AdditionalHitByTargetStatus : Ability
	{
		public enum DamageAmountType
		{
			Constant,
			PercentOfOriginalDamage
		}

		public class Instance : AbilityInstance<AdditionalHitByTargetStatus>
		{
			private float _remainCooldownTime;

			private int _remainCount;

			internal Instance(Character owner, AdditionalHitByTargetStatus ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				_remainCount = ability._applyCount;
				Character character = owner;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(character.onGaveDamage, new GaveDamageDelegate(OnOwnerGaveDamage));
			}

			protected override void OnDetach()
			{
				Character character = owner;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Remove(character.onGaveDamage, new GaveDamageDelegate(OnOwnerGaveDamage));
			}

			private void OnOwnerGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
			{
				if (!(_remainCooldownTime > 0f) && !target.character.health.dead && !(target.character.status == null) && target.character.status.IsApplying(ability._targetStatusFilter) && target.transform.gameObject.activeSelf && (!ability._needCritical || tookDamage.critical) && ability._attackTypes[tookDamage.motionType] && ability._damageTypes[tookDamage.attackType])
				{
					double baseDamage;
					if (ability._damageAmountType == DamageAmountType.Constant)
					{
						baseDamage = ability._additionalDamageAmount;
					}
					else
					{
						double num = (double)ability._additionalDamageAmount * 0.01;
						Damage damage = originalDamage;
						baseDamage = num * damage.amount;
					}
					Damage damage2 = owner.stat.GetDamage(baseDamage, MMMaths.RandomPointWithinBounds(target.collider.bounds), ability._additionalHit);
					owner.Attack(target, ref damage2);
					ability._hitEffect.Spawn(owner, target.collider.bounds, ref damage2, target);
					_remainCooldownTime = ability._cooldownTime;
					_remainCount--;
					if (_remainCount == 0)
					{
						owner.ability.Remove(this);
					}
				}
			}
		}

		[SerializeField]
		private float _cooldownTime;

		[SerializeField]
		private int _applyCount;

		[SerializeField]
		private DamageAmountType _damageAmountType;

		[SerializeField]
		private int _additionalDamageAmount;

		[SerializeField]
		private HitInfo _additionalHit = new HitInfo(Damage.AttackType.Additional);

		[SerializeField]
		private CharacterStatusKindBoolArray _targetStatusFilter;

		[SerializeField]
		private bool _needCritical;

		[SerializeField]
		private MotionTypeBoolArray _attackTypes;

		[SerializeField]
		private AttackTypeBoolArray _damageTypes;

		[SerializeField]
		[BoundsAttackVisualEffect.Subcomponent]
		private BoundsAttackVisualEffect.Subcomponents _hitEffect;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
