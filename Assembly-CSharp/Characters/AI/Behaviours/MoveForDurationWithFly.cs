using System.Collections;
using Level;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class MoveForDurationWithFly : Move
	{
		[MinMaxSlider(0f, 10f)]
		[SerializeField]
		private Vector2 _duration;

		public override IEnumerator CRun(AIController controller)
		{
			Character character = controller.character;
			base.result = Result.Doing;
			StartCoroutine(CExpire(controller, _duration));
			Vector2 direction2 = direction;
			Bounds bounds = Map.Instance.bounds;
			while (base.result == Result.Doing)
			{
				yield return null;
				character.movement.move = direction;
				ChangeDirectionIfBlocked(character, bounds);
			}
			idle.CRun(controller);
		}

		private void ChangeDirectionIfBlocked(Character character, Bounds bounds)
		{
			if (character.transform.position.x + direction.x < bounds.min.x && character.lookingDirection == Character.LookingDirection.Left)
			{
				direction.x = 1f;
			}
			else if (character.transform.position.x + direction.x > bounds.max.x && character.lookingDirection == Character.LookingDirection.Right)
			{
				direction.x = -1f;
			}
			if (character.transform.position.y + direction.y < bounds.min.y)
			{
				direction.y = 1f;
			}
			else if (character.transform.position.y + direction.y > bounds.max.y)
			{
				direction.y = -1f;
			}
		}
	}
}
