using UnityEngine;

namespace Characters.Operations.LookAtTargets
{
	public class ClosestSideOnPlatform : Target
	{
		public override Character.LookingDirection GetDirectionFrom(Character character)
		{
			Collider2D lastStandingCollider = character.movement.controller.collisionState.lastStandingCollider;
			if (lastStandingCollider == null)
			{
				return character.lookingDirection;
			}
			if (character.transform.position.x > lastStandingCollider.bounds.center.x)
			{
				return Character.LookingDirection.Right;
			}
			return Character.LookingDirection.Left;
		}
	}
}
