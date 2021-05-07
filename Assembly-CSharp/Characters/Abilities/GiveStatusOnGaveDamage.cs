using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class GiveStatusOnGaveDamage : Ability
	{
		public class Instance : AbilityInstance<GiveStatusOnGaveDamage>
		{
			private float _remainCooldown;

			public override float iconFillAmount => _remainCooldown / ability._cooldown;

			public Instance(Character owner, GiveStatusOnGaveDamage ability)
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

			public override void UpdateTime(float deltaTime)
			{
				base.UpdateTime(deltaTime);
				_remainCooldown -= deltaTime;
			}

			private void OnGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage gaveDamage, double damageDealt)
			{
				if (!(_remainCooldown > 0f) && !(target.character == null) && !target.character.health.dead && ability._motionTypeFilter[gaveDamage.motionType] && ability._damageTypeFilter[gaveDamage.attackType])
				{
					_remainCooldown = ability._cooldown;
					owner.GiveStatus(target.character, ability._status);
				}
			}
		}

		[SerializeField]
		private float _cooldown;

		[SerializeField]
		private MotionTypeBoolArray _motionTypeFilter;

		[SerializeField]
		private AttackTypeBoolArray _damageTypeFilter;

		[SerializeField]
		private CharacterStatus.ApplyInfo _status;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
