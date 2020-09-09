using Apex.Serialization;
using UnityEngine;

namespace Rust.Ai
{
	public sealed class DistanceFromFoodTarget : BaseScorer
	{
		[ApexSerialization(defaultValue = 10f)]
		public float MaxDistance = 10f;

		public override float GetScore(BaseContext c)
		{
			if (c.AIAgent.FoodTarget == null)
			{
				return 0f;
			}
			return Vector3.Distance(c.Position, c.AIAgent.FoodTarget.transform.localPosition) / MaxDistance;
		}
	}
}
