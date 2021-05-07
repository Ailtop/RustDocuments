using UnityEngine;

namespace Characters.Projectiles.Operations.Decorator
{
	public class Chance : Operation
	{
		[SerializeField]
		[Range(0f, 1f)]
		private float _successChance = 0.5f;

		[SerializeField]
		[Subcomponent]
		private Operation _onSuccess;

		[SerializeField]
		[Subcomponent]
		private Operation _onFail;

		public override void Run(Projectile projectile)
		{
			if (MMMaths.Chance(_successChance))
			{
				if (!(_onSuccess == null))
				{
					_onSuccess.Run(projectile);
				}
			}
			else if (!(_onFail == null))
			{
				_onFail.Run(projectile);
			}
		}
	}
}
