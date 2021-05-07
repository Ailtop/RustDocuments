using Apex.Serialization;
using UnityEngine;

namespace Rust.Ai
{
	public sealed class EntityDistance : WeightedScorerBase<BaseEntity>
	{
		[ApexSerialization(defaultValue = 10f)]
		public float DistanceScope = 10f;

		public override float GetScore(BaseContext c, BaseEntity target)
		{
			if (target == null)
			{
				return 1f;
			}
			return Vector3.Distance(target.ServerPosition, c.Position) / DistanceScope;
		}
	}
}
