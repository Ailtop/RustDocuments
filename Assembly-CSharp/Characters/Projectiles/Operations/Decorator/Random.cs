using UnityEngine;

namespace Characters.Projectiles.Operations.Decorator
{
	public class Random : Operation
	{
		[SerializeField]
		[Subcomponent]
		private Subcomponents _toRandom;

		public override void Run(Projectile projectile)
		{
			_toRandom.components.Random().Run(projectile);
		}
	}
}
