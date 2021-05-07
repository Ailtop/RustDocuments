using System.Collections;
using Characters.Actions;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.DarkQuartzGolem
{
	public class Rush : Behaviour, IPattern
	{
		[SerializeField]
		private Action _ready;

		[SerializeField]
		private Action _action;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(MoveToDestination))]
		private MoveToDestination _moveToDestination;

		public bool CanUse(AIController controller)
		{
			if (!_action.canUse)
			{
				return false;
			}
			Character target = controller.target;
			Character character = controller.character;
			Collider2D lastStandingCollider = target.movement.controller.collisionState.lastStandingCollider;
			Collider2D lastStandingCollider2 = character.movement.controller.collisionState.lastStandingCollider;
			if (lastStandingCollider == lastStandingCollider2)
			{
				return true;
			}
			return false;
		}

		public bool CanUse()
		{
			return _action.canUse;
		}

		public override IEnumerator CRun(AIController controller)
		{
			Character target = controller.target;
			Character character = controller.character;
			Collider2D ownerPlatform = character.movement.controller.collisionState.lastStandingCollider;
			character.ForceToLookAt(target.transform.position.x);
			_ready.TryStart();
			while (_ready.running)
			{
				yield return null;
			}
			_action.TryStart();
			if (target.transform.position.x > character.transform.position.x)
			{
				SetWalkDestinationToMax(controller, ownerPlatform.bounds);
			}
			else
			{
				SetWalkDestinationToMin(controller, ownerPlatform.bounds);
			}
			yield return _moveToDestination.CRun(controller);
			character.CancelAction();
		}

		private void SetWalkDestinationToMin(AIController controller, Bounds bounds)
		{
			float x = bounds.min.x + controller.character.collider.bounds.size.x;
			float y = bounds.max.y;
			controller.destination = new Vector2(x, y);
		}

		private void SetWalkDestinationToMax(AIController controller, Bounds bounds)
		{
			float x = bounds.max.x - controller.character.collider.bounds.size.x;
			float y = bounds.max.y;
			controller.destination = new Vector2(x, y);
		}
	}
}
