using UnityEngine;

namespace Characters.AI
{
	public static class Precondition
	{
		public static bool CanMove(Character character)
		{
			Collider2D lastStandingCollider = character.movement.controller.collisionState.lastStandingCollider;
			if (lastStandingCollider == null)
			{
				return true;
			}
			if (lastStandingCollider.bounds.size.x > 3f)
			{
				return true;
			}
			return false;
		}

		public static bool CanChase(Character character, Character target)
		{
			Collider2D lastStandingCollider = character.movement.controller.collisionState.lastStandingCollider;
			if (target == null || target.movement == null || target.movement.controller == null || target.movement.controller.collisionState == null || target.movement.controller.collisionState.lastStandingCollider == null)
			{
				return true;
			}
			Collider2D lastStandingCollider2 = target.movement.controller.collisionState.lastStandingCollider;
			if (lastStandingCollider != lastStandingCollider2)
			{
				return false;
			}
			if (lastStandingCollider.bounds.center.y - character.collider.bounds.size.y > lastStandingCollider2.bounds.center.y && lastStandingCollider.bounds.max.x > target.transform.position.x && lastStandingCollider.bounds.min.x < target.transform.position.x)
			{
				return false;
			}
			return true;
		}
	}
}
