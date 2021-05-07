using System.Collections;
using Characters.Movements;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class MoveToDestination : Move
	{
		[SerializeField]
		private float _endDistance = 1f;

		public override IEnumerator CRun(AIController controller)
		{
			Character character = controller.character;
			base.result = Result.Doing;
			Vector2 move = ((controller.destination.x - character.transform.position.x > 0f) ? Vector2.right : Vector2.left);
			character.movement.move = move;
			StartCoroutine(CanMove(controller));
			while (base.result.Equals(Result.Doing))
			{
				yield return null;
				if (wander && controller.target != null)
				{
					base.result = Result.Success;
					break;
				}
				if (checkWithinSight && controller.target != null && Precondition.CanChase(character, controller.target))
				{
					base.result = Result.Success;
					break;
				}
				float num = controller.destination.x - character.transform.position.x;
				move = ((num > 0f) ? Vector2.right : Vector2.left);
				if (Mathf.Abs(num) < _endDistance)
				{
					base.result = Result.Done;
					yield return idle.CRun(controller);
					break;
				}
				character.movement.move = move;
			}
		}

		private IEnumerator CanMove(AIController controller)
		{
			Character character = controller.character;
			CharacterController2D characterController = character.GetComponent<CharacterController2D>();
			while (base.result == Result.Doing)
			{
				yield return null;
				if (characterController.velocity.x == 0f)
				{
					base.result = Result.Fail;
					break;
				}
			}
		}
	}
}
