using Characters.AI;
using UnityEngine;

namespace Characters.Operations
{
	public class TakeAimTowardsTheFront : CharacterOperation
	{
		[SerializeField]
		private Transform _centerAxisPosition;

		[SerializeField]
		private AIController _controller;

		[SerializeField]
		private bool _platformTarget;

		private float _originalDirection;

		private void Awake()
		{
			_originalDirection = 0f;
		}

		public override void Run(Character owner)
		{
			Character target = _controller.target;
			Vector3 vector = new Vector3(y: (!_platformTarget) ? (target.transform.position.y + target.collider.bounds.extents.y) : target.movement.controller.collisionState.lastStandingCollider.bounds.max.y, x: target.transform.position.x) - _centerAxisPosition.transform.position;
			float num = (num = Mathf.Atan2(vector.y, vector.x) * 57.29578f);
			if (owner.lookingDirection == Character.LookingDirection.Left)
			{
				num = 180f - num;
			}
			_centerAxisPosition.rotation = Quaternion.Euler(0f, 0f, _originalDirection + num);
		}
	}
}
