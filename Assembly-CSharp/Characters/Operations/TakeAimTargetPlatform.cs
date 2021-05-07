using Characters.AI;
using UnityEngine;

namespace Characters.Operations
{
	public class TakeAimTargetPlatform : CharacterOperation
	{
		[SerializeField]
		private Transform _centerAxisPosition;

		[SerializeField]
		private Transform _weaponAxisPosition;

		[SerializeField]
		private AIController _controller;

		[SerializeField]
		private LayerMask _layerMask = 18;

		[SerializeField]
		private float _distance = 10f;

		private Vector3 _originalScale;

		private float _originalDirection;

		private void Awake()
		{
			_originalDirection = 0f;
			_originalScale = Vector3.one;
		}

		public override void Run(Character owner)
		{
			RaycastHit2D point;
			if (_controller.target.movement.TryBelowRayCast(_layerMask, out point, _distance))
			{
				Vector2 point2 = point.point;
				point2.y += _controller.target.collider.size.y;
				point.point = point2;
				Vector2 vector = point.point - (Vector2)_weaponAxisPosition.transform.position;
				float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
				_weaponAxisPosition.rotation = Quaternion.Euler(0f, 0f, _originalDirection + num);
			}
		}
	}
}
