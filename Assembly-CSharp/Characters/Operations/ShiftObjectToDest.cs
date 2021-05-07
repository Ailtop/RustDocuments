using UnityEngine;

namespace Characters.Operations
{
	public class ShiftObjectToDest : CharacterOperation
	{
		[SerializeField]
		private Transform _destination;

		[SerializeField]
		private Transform _object;

		[SerializeField]
		private float _offsetY;

		[SerializeField]
		private float _offsetX;

		[SerializeField]
		private bool _fromPlatform;

		public override void Run(Character owner)
		{
			float x = _destination.transform.position.x + _offsetX;
			float num = ((!_fromPlatform) ? _destination.transform.position.y : owner.movement.controller.collisionState.lastStandingCollider.bounds.max.y);
			num += _offsetY;
			_object.position = new Vector2(x, num);
		}
	}
}
