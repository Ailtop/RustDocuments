using System.Collections;
using Characters.Actions;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Hero
{
	public class BackDash : Behaviour
	{
		[SerializeField]
		private Action _action;

		[SerializeField]
		private float _flipDirectionDistance = 3f;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			Character target = controller.target;
			Character character = controller.character;
			float num = character.transform.position.x - target.transform.position.x;
			Bounds bounds = character.movement.controller.collisionState.lastStandingCollider.bounds;
			if (num > 0f)
			{
				if (Mathf.Abs(bounds.max.x - character.transform.position.x) < _flipDirectionDistance)
				{
					character.ForceToLookAt(Character.LookingDirection.Right);
				}
				else
				{
					character.ForceToLookAt(Character.LookingDirection.Left);
				}
			}
			else if (Mathf.Abs(bounds.min.x - character.transform.position.x) < _flipDirectionDistance)
			{
				character.ForceToLookAt(Character.LookingDirection.Left);
			}
			else
			{
				character.ForceToLookAt(Character.LookingDirection.Right);
			}
			_action.TryStart();
			while (_action.running)
			{
				yield return null;
			}
			base.result = Result.Done;
		}

		public bool CanUse()
		{
			return _action.canUse;
		}
	}
}
