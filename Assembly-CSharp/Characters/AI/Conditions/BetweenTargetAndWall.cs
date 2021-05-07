using UnityEngine;

namespace Characters.AI.Conditions
{
	public sealed class BetweenTargetAndWall : Condition
	{
		protected override bool Check(AIController controller)
		{
			Character target = controller.target;
			Character character = controller.character;
			Collider2D lastStandingCollider = controller.character.movement.controller.collisionState.lastStandingCollider;
			if (character.transform.position.x < lastStandingCollider.bounds.center.x)
			{
				if (character.transform.position.x <= target.transform.position.x)
				{
					return true;
				}
				return false;
			}
			if (character.transform.position.x >= target.transform.position.x)
			{
				return true;
			}
			return false;
		}
	}
}
