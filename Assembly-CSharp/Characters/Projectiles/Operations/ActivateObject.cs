using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public class ActivateObject : HitOperation
	{
		[SerializeField]
		private GameObject _target;

		public override void Run(Projectile projectile, RaycastHit2D raycastHit)
		{
			Vector2 point = raycastHit.point;
			Object.Destroy(Object.Instantiate(_target, point, Quaternion.identity), 10f);
		}
	}
}
