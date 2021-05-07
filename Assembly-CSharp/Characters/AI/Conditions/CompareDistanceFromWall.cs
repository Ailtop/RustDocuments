using UnityEngine;

namespace Characters.AI.Conditions
{
	public class CompareDistanceFromWall : Condition
	{
		private enum Comparer
		{
			GreaterThan,
			LessThan
		}

		[SerializeField]
		private Comparer _compare;

		[SerializeField]
		private float _distanceFromWall;

		protected override bool Check(AIController controller)
		{
			Collider2D lastStandingCollider = controller.character.movement.controller.collisionState.lastStandingCollider;
			if (lastStandingCollider == null)
			{
				return false;
			}
			Bounds bounds = lastStandingCollider.bounds;
			float num = ((controller.character.transform.position.x > bounds.center.x) ? Mathf.Abs(bounds.max.x - controller.character.transform.position.x) : Mathf.Abs(bounds.min.x - controller.character.transform.position.x));
			switch (_compare)
			{
			case Comparer.GreaterThan:
				return num >= _distanceFromWall;
			case Comparer.LessThan:
				return num <= _distanceFromWall;
			default:
				return false;
			}
		}
	}
}
