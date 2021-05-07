using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public class ClearBounceHistory : Operation
	{
		[SerializeField]
		private Bounce _bounce;

		public override void Run(Projectile projectile)
		{
			_bounce.lastCollision = null;
		}
	}
}
