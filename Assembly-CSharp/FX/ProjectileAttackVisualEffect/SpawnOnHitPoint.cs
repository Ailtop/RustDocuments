using Characters;
using Characters.Projectiles;
using UnityEngine;

namespace FX.ProjectileAttackVisualEffect
{
	public class SpawnOnHitPoint : ProjectileAttackVisualEffect
	{
		[SerializeField]
		private Transform _spawnPosition;

		[Header("Spawn map")]
		[SerializeField]
		private bool _spawnOnDespawn;

		[SerializeField]
		private bool _spawnOnExpire = true;

		[SerializeField]
		private bool _spawnOnTerrainHit = true;

		[SerializeField]
		private bool _spawnOnCharacterHit = true;

		[SerializeField]
		private bool _spawnOnDamageableHit = true;

		[Header("Effects")]
		[SerializeField]
		private EffectInfo _normal;

		[SerializeField]
		private EffectInfo _critical;

		private void Awake()
		{
			if (_critical.effect == null)
			{
				_critical = _normal;
			}
		}

		public override void SpawnDespawn(Projectile projectile)
		{
			if (_spawnOnDespawn)
			{
				Vector3 position = ((_spawnPosition == null) ? projectile.transform.position : _spawnPosition.position);
				_normal.Spawn(position);
			}
		}

		public override void SpawnExpire(Projectile projectile)
		{
			if (_spawnOnExpire)
			{
				Vector3 position = ((_spawnPosition == null) ? projectile.transform.position : _spawnPosition.position);
				_normal.Spawn(position);
			}
		}

		public override void Spawn(Projectile projectile, Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit)
		{
			if (_spawnOnTerrainHit)
			{
				_normal.Spawn(raycastHit.point, projectile.owner).transform.localScale.Scale(projectile.transform.localScale);
			}
		}

		public override void Spawn(Projectile projectile, Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit, Damage damage, ITarget target)
		{
			if ((_spawnOnCharacterHit && target.character != null) || (_spawnOnDamageableHit && target.damageable != null))
			{
				ReusableChronoSpriteEffect reusableChronoSpriteEffect = (damage.critical ? _critical : _normal).Spawn(raycastHit.point, projectile.owner);
				if (reusableChronoSpriteEffect != null)
				{
					reusableChronoSpriteEffect.transform.localScale.Scale(projectile.transform.localScale);
				}
			}
		}
	}
}
