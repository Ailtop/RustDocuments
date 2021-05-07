using System.Collections;
using Characters.Actions;
using UnityEngine;

namespace Characters.AI.Behaviours.Hero
{
	public sealed class Backstep : Behaviour
	{
		[SerializeField]
		private Action _action;

		public override IEnumerator CRun(AIController controller)
		{
			_action.TryStart();
			while (_action.running)
			{
				yield return null;
			}
		}

		private void LookSide(Character character)
		{
			if (!(character.movement.controller.collisionState.lastStandingCollider == null))
			{
				float x = character.movement.controller.collisionState.lastStandingCollider.bounds.center.x;
				if (character.transform.position.x > x)
				{
					character.ForceToLookAt(Character.LookingDirection.Right);
				}
				else
				{
					character.ForceToLookAt(Character.LookingDirection.Left);
				}
			}
		}
	}
}
