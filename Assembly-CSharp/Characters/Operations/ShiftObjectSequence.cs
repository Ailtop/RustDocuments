using Characters.AI;
using UnityEngine;

namespace Characters.Operations
{
	public class ShiftObjectSequence : CharacterOperation
	{
		[SerializeField]
		private AIController _controller;

		[SerializeField]
		private Transform _object;

		[SerializeField]
		private Transform _origin;

		[SerializeField]
		private int _index;

		[SerializeField]
		private float _offsetY;

		[SerializeField]
		private float _offsetX;

		[SerializeField]
		private float _deltaY;

		[SerializeField]
		private float _deltaX;

		[SerializeField]
		private bool _fromPlatform;

		public override void Run(Character owner)
		{
			Character target = _controller.target;
			if (!(target == null))
			{
				Bounds bounds = target.movement.controller.collisionState.lastStandingCollider.bounds;
				float x = _origin.position.x + _offsetX;
				float num = (_fromPlatform ? bounds.max.y : _origin.position.y);
				num += _offsetY;
				_object.position = new Vector2(x, num);
			}
		}
	}
}
