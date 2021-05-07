using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public class Despawn : HitOperation
	{
		public override void Run(Projectile projectile, RaycastHit2D raycastHit)
		{
			projectile.Despawn();
		}
	}
}
