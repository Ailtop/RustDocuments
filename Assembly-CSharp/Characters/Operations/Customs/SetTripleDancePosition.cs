using Characters.AI;
using Level;
using UnityEngine;

namespace Characters.Operations.Customs
{
	public class SetTripleDancePosition : CharacterOperation
	{
		[SerializeField]
		private AIController _controller;

		[SerializeField]
		private Transform _object;

		[SerializeField]
		private float _minDistanceFromSide;

		[SerializeField]
		private float _offsetY;

		[SerializeField]
		private float _offsetX;

		public override void Run(Character owner)
		{
			Character target = _controller.target;
			if (!(target == null))
			{
				Bounds bounds = target.movement.controller.collisionState.lastStandingCollider.bounds;
				float x = target.transform.position.x;
				x += _offsetX;
				Evaluate(ref x);
				float y = bounds.max.y;
				y += _offsetY;
				_object.position = new Vector2(x, y);
			}
		}

		private void Evaluate(ref float x)
		{
			Bounds bounds = Map.Instance.bounds;
			float num = bounds.max.x - _minDistanceFromSide;
			float num2 = bounds.min.x + _minDistanceFromSide;
			if (num < x)
			{
				x = num;
			}
			if (num2 > x)
			{
				x = num2;
			}
		}
	}
}
