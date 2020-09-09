using Apex.Serialization;
using UnityEngine;

namespace Rust.Ai
{
	public sealed class DistanceFromTargetDestination : BaseScorer
	{
		[ApexSerialization(defaultValue = 10f)]
		public float MaxDistance = 10f;

		public override float GetScore(BaseContext c)
		{
			if (!c.AIAgent.IsNavRunning())
			{
				return 1f;
			}
			return Vector3.Distance(c.Entity.ServerPosition, c.AIAgent.GetNavAgent.destination) / MaxDistance;
		}
	}
}
