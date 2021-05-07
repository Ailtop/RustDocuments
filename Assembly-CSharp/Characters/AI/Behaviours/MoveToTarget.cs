using System.Collections;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class MoveToTarget : Move
	{
		public override IEnumerator CRun(AIController controller)
		{
			Character character = controller.character;
			Character target = controller.target;
			base.result = Result.Doing;
			while (base.result == Result.Doing)
			{
				if (target.movement.controller.collisionState.lastStandingCollider == null)
				{
					yield return null;
					continue;
				}
				if (controller.target == null || !Precondition.CanChase(character, controller.target))
				{
					base.result = Result.Fail;
					break;
				}
				Bounds bounds = character.movement.controller.collisionState.lastStandingCollider.bounds;
				Bounds bounds2 = target.movement.controller.collisionState.lastStandingCollider.bounds;
				if (bounds.center.y - character.collider.bounds.size.y > bounds2.center.y)
				{
					character.movement.move = ((character.lookingDirection == Character.LookingDirection.Right) ? Vector2.right : Vector2.left);
					yield return null;
					continue;
				}
				float num = controller.target.transform.position.x - character.transform.position.x;
				if (Mathf.Abs(num) < 0.1f || LookAround(controller))
				{
					yield return idle.CRun(controller);
					base.result = Result.Success;
					break;
				}
				character.movement.move = ((num > 0f) ? Vector2.right : Vector2.left);
				yield return null;
			}
		}
	}
}
