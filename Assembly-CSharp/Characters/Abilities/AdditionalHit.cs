using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Operations;
using FX.BoundsAttackVisualEffect;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class AdditionalHit : Ability
	{
		public class Instance : AbilityInstance<AdditionalHit>
		{
			private float _remainCooldownTime;

			private int _remainCount;

			internal Instance(Character owner, AdditionalHit ability)
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
				if (!(_remainCooldownTime > 0f) && !target.character.health.dead && target.transform.gameObject.activeSelf && (!ability._needCritical || tookDamage.critical) && ability._attackTypes[tookDamage.motionType] && ability._damageTypes[tookDamage.attackType])
				{
					Damage damage = owner.stat.GetDamage(ability._additionalDamageAmount, MMMaths.RandomPointWithinBounds(target.collider.bounds), ability._additionalHit);
					owner.Attack(target, ref damage);
					ability._hitEffect.Spawn(owner, target.collider.bounds, ref damage, target);
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
		private float _additionalDamageAmount;

		[SerializeField]
		private HitInfo _additionalHit = new HitInfo(Damage.AttackType.Additional);

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
