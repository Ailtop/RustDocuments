using Characters.AI;
using UnityEngine;

namespace Characters.Operations
{
	public class TakeAim : CharacterOperation
	{
		[SerializeField]
		private Transform _centerAxisPosition;

		[SerializeField]
		private AIController _controller;

		[SerializeField]
		private bool _platformTarget;

		[SerializeField]
		private bool _lastStandingCollider = true;

		[SerializeField]
		private LayerMask _groundMask = Layers.groundMask;

		public override void Run(Character owner)
		{
			Character target = _controller.target;
			Collider2D collider;
			Vector3 vector = new Vector3(y: (!_platformTarget) ? (target.transform.position.y + target.collider.bounds.extents.y) : (_lastStandingCollider ? target.movement.controller.collisionState.lastStandingCollider.bounds.max.y : ((!target.movement.TryGetClosestBelowCollider(out collider, _groundMask)) ? target.transform.position.y : collider.bounds.max.y)), x: target.transform.position.x) - _centerAxisPosition.transform.position;
			float z = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			_centerAxisPosition.rotation = Quaternion.Euler(0f, 0f, z);
		}
	}
}
