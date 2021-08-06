using Apex.AI;
using Apex.Serialization;
using UnityEngine;

namespace Rust.Ai
{
	public class PointDirectnessToTarget : WeightedScorerBase<Vector3>
	{
		[ApexSerialization]
		[FriendlyName("Use Perfect Position Information", "Should we apply perfect knowledge about the attack target's whereabouts, or the last memorized position.")]
		private bool UsePerfectInfo;

		public override float GetScore(BaseContext c, Vector3 point)
		{
			Vector3 b = ((!UsePerfectInfo) ? c.AIAgent.AttackTargetMemory.Position : c.AIAgent.AttackTarget.ServerPosition);
			float num = Vector3.Distance(c.Position, b);
			float num2 = Vector3.Distance(point, b);
			float num3 = Vector3.Distance(c.Position, point);
			return (num - num2) / num3;
		}
	}
}
