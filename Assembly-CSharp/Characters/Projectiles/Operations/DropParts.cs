using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public class DropParts : HitOperation
	{
		[SerializeField]
		private Collider2D _range;

		[SerializeField]
		private ParticleEffectInfo _particleEffectInfo;

		[SerializeField]
		private Vector2 _direction;

		[SerializeField]
		private float _power = 3f;

		[SerializeField]
		private bool _interpolation;

		public override void Run(Projectile projectile, RaycastHit2D raycastHit)
		{
			_particleEffectInfo.Emit(raycastHit.point, _range.bounds, _direction * _power, _interpolation);
		}
	}
}
