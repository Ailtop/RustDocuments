using System;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class IgnoreTakingDamageByDirection : Ability
	{
		public class Instance : AbilityInstance<IgnoreTakingDamageByDirection>
		{
			public Instance(Character owner, IgnoreTakingDamageByDirection ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				owner.health.onTakeDamage.Add(int.MaxValue, CancelDamage);
			}

			protected override void OnDetach()
			{
				owner.health.onTakeDamage.Remove(CancelDamage);
			}

			private bool CancelDamage(ref Damage damage)
			{
				if (damage.attackType == Damage.AttackType.Additional)
				{
					return false;
				}
				Vector3 position = owner.transform.position;
				Vector3 vector = damage.attacker.transform.position;
				if (damage.attackType == Damage.AttackType.Ranged || damage.attackType == Damage.AttackType.Projectile)
				{
					vector = damage.hitPoint;
				}
				bool flag = ability._from == Direction.Front && owner.lookingDirection == Character.LookingDirection.Right;
				flag |= ability._from == Direction.Back && owner.lookingDirection == Character.LookingDirection.Left;
				bool flag2 = position.x < vector.x;
				if ((flag && !flag2) || (!flag && flag2))
				{
					return false;
				}
				return true;
			}
		}

		private enum Direction
		{
			Front,
			Back
		}

		[SerializeField]
		private Direction _from;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
