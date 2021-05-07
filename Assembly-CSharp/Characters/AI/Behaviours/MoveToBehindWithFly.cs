using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class MoveToBehindWithFly : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(MoveToDestinationWithFly))]
		private MoveToDestinationWithFly _moveToDestinationWithFly;

		[SerializeField]
		private float _distanceX;

		[SerializeField]
		private float _midPointHeight;

		public override IEnumerator CRun(AIController controller)
		{
			Character character = controller.character;
			Character target = controller.target;
			float num = target.transform.position.x - character.transform.position.x;
			Vector2 midPoint = new Vector2(target.transform.position.x, target.transform.position.y + _midPointHeight);
			float behindPosition = ((num > 0f) ? (target.transform.position.x - _distanceX) : (target.transform.position.x + _distanceX));
			base.result = Result.Doing;
			while (base.result == Result.Doing)
			{
				yield return null;
				controller.destination = midPoint;
				yield return _moveToDestinationWithFly.CRun(controller);
				controller.destination = new Vector2(behindPosition, target.transform.position.y);
				yield return _moveToDestinationWithFly.CRun(controller);
			}
		}
	}
}
