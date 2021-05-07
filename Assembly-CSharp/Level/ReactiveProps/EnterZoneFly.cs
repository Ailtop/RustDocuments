using PhysicsUtils;
using UnityEngine;

namespace Level.ReactiveProps
{
	public class EnterZoneFly : ReactiveProp
	{
		[GetComponent]
		[SerializeField]
		private Collider2D _collider;

		private static readonly NonAllocOverlapper _playerOverlapper;

		protected static readonly NonAllocOverlapper _enemyOverlapper;

		static EnterZoneFly()
		{
			_playerOverlapper = new NonAllocOverlapper(15);
			_playerOverlapper.contactFilter.SetLayerMask(512);
			_enemyOverlapper = new NonAllocOverlapper(31);
			_enemyOverlapper.contactFilter.SetLayerMask(1024);
		}

		private void Update()
		{
			if (CheckWithinSight() && !_flying)
			{
				Activate();
				Object.Destroy(_collider);
				_collider = null;
			}
		}

		private bool CheckWithinSight()
		{
			if (_collider == null)
			{
				return false;
			}
			NonAllocOverlapper nonAllocOverlapper = _playerOverlapper.OverlapCollider(_collider);
			NonAllocOverlapper nonAllocOverlapper2 = _enemyOverlapper.OverlapCollider(_collider);
			if (nonAllocOverlapper.results.Count > 0 || nonAllocOverlapper2.results.Count > 0)
			{
				return true;
			}
			return false;
		}
	}
}
