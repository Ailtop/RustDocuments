using FX;
using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public class Bounce : HitOperation
	{
		[SerializeField]
		private LayerMask _terrainLayer = Layers.terrainMaskForProjectile;

		[SerializeField]
		private EffectInfo _terrainLeftHitEffect;

		[SerializeField]
		private EffectInfo _terrainRightHitEffect;

		[SerializeField]
		private EffectInfo _terrainTopHitEffect;

		[SerializeField]
		private EffectInfo _terrainBottomHitEffect;

		public Collider2D lastCollision { get; set; }

		public override void Run(Projectile projectile, RaycastHit2D raycastHit)
		{
			if (raycastHit.collider == lastCollision)
			{
				return;
			}
			lastCollision = raycastHit.collider;
			Vector2 point = raycastHit.point;
			float speed = projectile.speed;
			Vector2 vector = new Vector2(0f - projectile.movement.directionVector.x, projectile.movement.directionVector.y);
			Vector2 vector2 = new Vector2(projectile.movement.directionVector.x, 0f - projectile.movement.directionVector.y);
			if ((bool)Physics2D.Raycast(point, vector, speed, _terrainLayer))
			{
				if (vector2.y > 0f)
				{
					_terrainBottomHitEffect.Spawn(raycastHit.point, projectile.owner);
				}
				else
				{
					_terrainTopHitEffect.Spawn(raycastHit.point, projectile.owner);
				}
				projectile.movement.directionVector = vector2;
			}
			else if ((bool)Physics2D.Raycast(point, vector2, speed, _terrainLayer))
			{
				projectile.movement.directionVector = vector;
				if (vector2.x > 0f)
				{
					_terrainRightHitEffect.Spawn(raycastHit.point, projectile.owner);
				}
				else
				{
					_terrainLeftHitEffect.Spawn(raycastHit.point, projectile.owner);
				}
			}
			else
			{
				projectile.movement.directionVector = new Vector2(0f - projectile.movement.directionVector.x, 0f - projectile.movement.directionVector.y);
			}
		}
	}
}
