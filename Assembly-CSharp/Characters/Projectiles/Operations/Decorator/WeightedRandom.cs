using UnityEditor;
using UnityEngine;

namespace Characters.Projectiles.Operations.Decorator
{
	public class WeightedRandom : Operation
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(OperationWithWeight))]
		private OperationWithWeight.Subcomponents _toRandom;

		public override void Run(Projectile projectile)
		{
			_toRandom.RunWeightedRandom(projectile);
		}
	}
}
