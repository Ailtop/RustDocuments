using Apex.Serialization;
using UnityEngine;

namespace Rust.Ai
{
	public sealed class DistanceFromTargetEntity : BaseScorer
	{
		[ApexSerialization(defaultValue = 10f)]
		public float MaxDistance = 10f;

		public override float GetScore(BaseContext c)
		{
			if (c.AIAgent.AttackTarget == null)
			{
				return 0f;
			}
			return Vector3.Distance(c.AIAgent.AttackPosition, c.AIAgent.AttackTargetMemory.Position) / MaxDistance;
		}
	}
}
