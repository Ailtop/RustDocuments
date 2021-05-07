using System.Collections;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class MoveToDestinationWithFly : Move
	{
		[SerializeField]
		private float _minimumDistance = 1f;

		public override IEnumerator CRun(AIController controller)
		{
			Character character = controller.character;
			base.result = Result.Doing;
			while (base.result == Result.Doing)
			{
				yield return null;
				if (Mathf.Abs((controller.destination - (Vector2)character.transform.position).sqrMagnitude) < _minimumDistance)
				{
					base.result = Result.Success;
					yield return idle.CRun(controller);
					break;
				}
				character.movement.MoveTo(controller.destination);
			}
		}
	}
}
