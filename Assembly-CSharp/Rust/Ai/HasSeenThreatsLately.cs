using Apex.Serialization;
using UnityEngine;

namespace Rust.Ai
{
	public sealed class HasSeenThreatsLately : BaseScorer
	{
		[ApexSerialization]
		public float WithinSeconds = 10f;

		public override float GetScore(BaseContext c)
		{
			if (c.AIAgent.AttackTargetMemory.Timestamp > 0f && Time.realtimeSinceStartup - c.AIAgent.AttackTargetMemory.Timestamp <= WithinSeconds)
			{
				return 1f;
			}
			return 0f;
		}
	}
}
