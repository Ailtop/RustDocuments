using System.Collections;
using Characters.Actions;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class EscapeTeleport : Behaviour
	{
		[UnityEditor.Subcomponent(typeof(Teleport))]
		[SerializeField]
		private Teleport _teleport;

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
		}

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			Character target = controller.target;
			Bounds bounds = target.movement.controller.collisionState.lastStandingCollider.bounds;
			Vector3 vector = target.transform.position - _teleportBounds.size / 2f;
			Vector3 vector2 = target.transform.position + _teleportBounds.size / 2f;
			float x = Random.Range(Mathf.Max(bounds.min.x, vector.x), Mathf.Min(bounds.max.x, vector2.x));
			_destinationTransform.position = new Vector3(x, bounds.max.y);
			yield return _teleport.CRun(controller);
		}

		public bool CanUse()
		{
			return _actionForCooldown.cooldown.canUse;
		}
	}
}
