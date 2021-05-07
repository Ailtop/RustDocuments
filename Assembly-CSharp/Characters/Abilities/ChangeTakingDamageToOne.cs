using System;

namespace Characters.Abilities
{
	[Serializable]
	public class ChangeTakingDamageToOne : Ability
	{
		public class Instance : AbilityInstance<ChangeTakingDamageToOne>
		{
			private float _remainCooldownTime;

			public Instance(Character owner, ChangeTakingDamageToOne ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				owner.health.onTakeDamage.Add(int.MinValue, OnOwnerTakeDamage);
			}

			protected override void OnDetach()
			{
				owner.health.onTakeDamage.Remove(OnOwnerTakeDamage);
			}

			private bool OnOwnerTakeDamage(ref Damage damage)
			{
				if (damage.amount < 1.0)
				{
					return false;
				}
				damage.@base = 1.0;
				damage.multiplier = 1.0;
				damage.criticalDamageMultiplier = 1.0;
				return false;
			}
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
