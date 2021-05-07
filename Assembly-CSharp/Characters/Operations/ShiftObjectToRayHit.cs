using UnityEngine;

namespace Characters.Operations
{
	public class ShiftObjectToRayHit : CharacterOperation
	{
		[SerializeField]
		private Transform _origin;

		[SerializeField]
		private Transform _object;

		[SerializeField]
		private float _offsetX = -1f;

		[SerializeField]
		private float _rayDistance;

		public override void Run(Character owner)
		{
			Vector2 direction = ((owner.lookingDirection == Character.LookingDirection.Right) ? Vector2.right : Vector2.left);
			RaycastHit2D raycastHit2D = Physics2D.Raycast(_origin.position, direction, _rayDistance, Layers.groundMask);
			if ((bool)raycastHit2D)
			{
				_object.position = new Vector2(raycastHit2D.point.x + _offsetX * direction.x, raycastHit2D.point.y);
				return;
			}
			float num = direction.x * _rayDistance;
			float num2 = direction.y * _rayDistance;
			_object.position = new Vector2(_origin.position.x + num + _offsetX * direction.x, _origin.position.y + num2);
		}
	}
}
