using Apex.Serialization;
using UnityEngine;

namespace Rust.Ai
{
	public class DistanceFromDestination : WeightedScorerBase<Vector3>
	{
		[ApexSerialization]
		public float Range = 50f;

		public override float GetScore(BaseContext c, Vector3 position)
		{
			if (!c.AIAgent.IsStopped)
			{
				return 1f;
			}
			return Vector3.Distance(position, c.AIAgent.Destination) / Range;
		}
	}
}
