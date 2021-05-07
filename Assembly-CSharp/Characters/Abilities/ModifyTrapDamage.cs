using System;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class ModifyTrapDamage : Ability
	{
		public class Instance : AbilityInstance<ModifyTrapDamage>
		{
			private float _remainCooldownTime;

			public Instance(Character owner, ModifyTrapDamage ability)
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
					damage.@base *= ability._damagePercent;
					damage.multiplier += ability._damagePercentPoint;
				}
				return false;
			}
		}

		[SerializeField]
		private float _damagePercent = 1f;

		[SerializeField]
		private float _damagePercentPoint;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
