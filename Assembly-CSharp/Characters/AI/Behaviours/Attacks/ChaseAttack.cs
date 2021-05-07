using System.Collections;
using Characters.Actions;
using Characters.Movements;
using UnityEngine;

namespace Characters.AI.Behaviours.Attacks
{
	public class ChaseAttack : Attack
	{
		[SerializeField]
		private Action _action;

		[SerializeField]
		private float _duration;

		public override IEnumerator CRun(AIController controller)
		{
			Character character = controller.character;
			Character target = controller.target;
			base.result = Result.Doing;
			StartCoroutine(CExpire(controller, _duration));
			_action.TryStart();
			while (base.result == Result.Doing)
			{
				yield return null;
				float num = character.transform.position.x - target.transform.position.x;
				if (Mathf.Abs(num) > 1f)
				{
					character.movement.move = ((num > 0f) ? Vector2.left : Vector2.right);
				}
			}
			character.movement.config.type = Movement.Config.Type.Walking;
		}
	}
}
