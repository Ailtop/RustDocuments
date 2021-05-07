using System;
using System.Linq;
using Characters.Actions;
using Characters.Operations;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class Skeleton_Shield4Guard : Ability
	{
		public class Instance : AbilityInstance<Skeleton_Shield4Guard>
		{
			public Instance(Character owner, Skeleton_Shield4Guard ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				owner.health.onTakeDamage.Add(int.MaxValue, Guard);
			}

			protected override void OnDetach()
			{
				owner.health.onTakeDamage.Remove(Guard);
			}

			private bool Guard(ref Damage damage)
			{
				Characters.Actions.Motion runningMotion = owner.runningMotion;
				if (runningMotion == null)
				{
					return false;
				}
				if (!ability._motions.Contains(runningMotion))
				{
					return false;
				}
				if (damage.attackType == Damage.AttackType.Additional)
				{
					return false;
				}
				Vector3 position = owner.transform.position;
				Vector3 position2 = damage.attacker.transform.position;
				if (owner.lookingDirection == Character.LookingDirection.Right && position.x > position2.x)
				{
					return false;
				}
				if (owner.lookingDirection == Character.LookingDirection.Left && position.x < position2.x)
				{
					return false;
				}
				if (ability._operationOnGuard.components.Length == 0)
				{
					return true;
				}
				Vector3 position3 = ((!(damage.attacker.projectile == null)) ? ((Vector3)ability._operationRange.ClosestPoint(damage.hitPoint)) : ((Vector3)MMMaths.RandomPointWithinBounds(ability._operationRange.bounds)));
				ability._operationRunPosition.position = position3;
				ability._operationOnGuard.Run(owner);
				return true;
			}
		}

		[SerializeField]
		private Characters.Actions.Motion[] _motions;

		[SerializeField]
		private BoxCollider2D _operationRange;

		[SerializeField]
		private Transform _operationRunPosition;

		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _operationOnGuard;

		public override void Initialize()
		{
			base.Initialize();
			_operationOnGuard.Initialize();
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
