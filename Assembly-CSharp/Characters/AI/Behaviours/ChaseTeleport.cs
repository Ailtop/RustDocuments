using System.Collections;
using Characters.Actions;
using Level;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class ChaseTeleport : Behaviour
	{
		private enum Type
		{
			RandomDestination,
			BackOfTarget
		}

		[SerializeField]
		private Type _type;

		[Teleport.Subcomponent(true)]
		[SerializeField]
		private Behaviour _teleport;

		[SerializeField]
		private Transform _destinationTransform;

		[SerializeField]
		private Collider2D _teleportBoundsCollider;

		[SerializeField]
		private Action _actionForCooldown;

		private Bounds _teleportBounds;

		private void Awake()
		{
			_teleportBounds = _teleportBoundsCollider.bounds;
			_destinationTransform.parent = Map.Instance.transform;
		}

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			_destinationTransform.position = SetDestination(controller);
			yield return _teleport.CRun(controller);
			base.result = Result.Done;
		}

		private Vector3 SetDestination(AIController controller)
		{
			if (_type != 0)
			{
				int num = 1;
				return _destinationTransform.position;
			}
			Character target = controller.target;
			if (target == null)
			{
				return controller.transform.position;
			}
			Bounds bounds = target.movement.controller.collisionState.lastStandingCollider.bounds;
			Vector3 vector = target.transform.position - _teleportBounds.size / 2f;
			Vector3 vector2 = target.transform.position + _teleportBounds.size / 2f;
			return new Vector3(Random.Range(Mathf.Max(bounds.min.x, vector.x), Mathf.Min(bounds.max.x, vector2.x)), bounds.max.y);
		}

		public bool CanUse()
		{
			return _actionForCooldown.cooldown.canUse;
		}
	}
}
