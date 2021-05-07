using System;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class CriticalToMaximumHealth : Ability
	{
		public class Instance : AbilityInstance<CriticalToMaximumHealth>
		{
			private float _remainCooldownTime;

			internal Instance(Character owner, CriticalToMaximumHealth ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				owner.onGiveDamage.Add(0, OnGiveDamage);
			}

			protected override void OnDetach()
			{
				owner.onGiveDamage.Remove(OnGiveDamage);
			}

			private bool OnGiveDamage(ITarget target, ref Damage damage)
			{
				CharacterHealth characterHealth = target.character?.health;
				if (characterHealth == null || characterHealth.percent < 1.0)
				{
					return false;
				}
				damage.criticalChance = 1.0;
				_remainCooldownTime = ability._cooldownTime;
				return false;
			}
		}

		[SerializeField]
		private float _cooldownTime;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
