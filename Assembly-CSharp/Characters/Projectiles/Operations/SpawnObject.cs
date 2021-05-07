using Level;
using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public class SpawnObject : HitOperation
	{
		[SerializeField]
		private GameObject _object;

		[SerializeField]
		private float _lifeTime;

		public override void Run(Projectile projectile, RaycastHit2D raycastHit)
		{
			GameObject obj = Object.Instantiate(_object, raycastHit.point, Quaternion.identity, Map.Instance.transform);
			if (_lifeTime != 0f)
			{
				Object.Destroy(obj, _lifeTime);
			}
		}
	}
}
