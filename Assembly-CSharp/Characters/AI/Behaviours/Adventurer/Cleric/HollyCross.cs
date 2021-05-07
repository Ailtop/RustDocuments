using System.Collections;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Cleric
{
	public class HollyCross : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ActionAttack))]
		private ActionAttack _hollyCross;

		[SerializeField]
		private Transform _attackArea;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			Character character = controller.character;
			Character target = controller.target;
			character.ForceToLookAt(target.transform.position.x);
			yield return _hollyCross.CRun(controller);
			base.result = Result.Done;
		}

		private void ShiftAttackArea(Character target)
		{
			Bounds bounds = target.movement.controller.collisionState.lastStandingCollider.bounds;
			_attackArea.position = new Vector2(target.transform.position.x, bounds.max.y);
		}
	}
}
