using System;

namespace Characters.Abilities
{
	[Serializable]
	public class IgnoreTrapDamage : Ability
	{
		public class Instance : AbilityInstance<IgnoreTrapDamage>
		{
			private float _remainCooldownTime;

			public Instance(Character owner, IgnoreTrapDamage ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				owner.health.onTakeDamage.Add(int.MaxValue, OnOwnerTakeDamage);
			}

			protected override void OnDetach()
			{
				owner.health.onTakeDamage.Remove(OnOwnerTakeDamage);
			}

			private bool OnOwnerTakeDamage(ref Damage damage)
			{
				if (damage.attacker.character != null && damage.attacker.character.type == Character.Type.Trap)
				{
					return true;
				}
				return false;
			}
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
