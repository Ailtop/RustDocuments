using PhysicsUtils;
using UnityEngine;

namespace Characters.Actions.Constraints
{
	public class EnemyWithinRangeConstraint : Constraint
	{
		private static readonly NonAllocOverlapper _enemyOverlapper;

		[SerializeField]
		private Collider2D _searchRange;

		static EnemyWithinRangeConstraint()
		{
			_enemyOverlapper = new NonAllocOverlapper(1);
			_enemyOverlapper.contactFilter.SetLayerMask(1024);
		}

		public override bool Pass()
		{
			using (new UsingCollider(_searchRange))
			{
				return _enemyOverlapper.OverlapCollider(_searchRange).results.Count > 0;
			}
		}
	}
}
