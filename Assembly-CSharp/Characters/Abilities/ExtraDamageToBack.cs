using System;
using Characters.Movements;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class ExtraDamageToBack : Ability
	{
		public class Instance : AbilityInstance<ExtraDamageToBack>
		{
			private float _remainCooldownTime;

			public Instance(Character owner, ExtraDamageToBack ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				owner.onGiveDamage.Add(0, OnOwnerGiveDamage);
			}

			protected override void OnDetach()
			{
				owner.onGiveDamage.Remove(OnOwnerGiveDamage);
			}

			private bool OnOwnerGiveDamage(ITarget target, ref Damage damage)
			{
				if (!ability._motionTypeFilter[damage.motionType] || !ability._attackTypeFilter[damage.attackType])
				{
					return false;
				}
				if (!(target.character == null))
				{
					Movement movement = target.character.movement;
					if ((object)movement == null || movement.config.type != 0)
					{
						int num = Math.Sign(damage.attacker.transform.position.x - target.transform.position.x);
						if ((num == 1 && target.character.lookingDirection == Character.LookingDirection.Right) || (num == -1 && target.character.lookingDirection == Character.LookingDirection.Left))
						{
							return false;
						}
						damage.@base *= ability._percent;
						damage.multiplier += ability._percentPoint;
						_remainCooldownTime = ability._cooldownTime;
						return false;
					}
				}
				return false;
			}
		}

		[SerializeField]
		private float _cooldownTime;

		[SerializeField]
		private float _percent = 1f;

		[SerializeField]
		private float _percentPoint;

		[Header("Filter")]
		[SerializeField]
		private MotionTypeBoolArray _motionTypeFilter;

		[SerializeField]
		private AttackTypeBoolArray _attackTypeFilter;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
