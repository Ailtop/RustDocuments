using Apex.AI;
using Apex.Serialization;
using UnityEngine;

namespace Rust.Ai
{
	public class BehaviouralPointDirectnessToTarget : PointDirectnessToTarget
	{
		public enum Guide
		{
			Approach,
			Retreat,
			Flank
		}

		[ApexSerialization]
		[FriendlyName("Minimum Directness", "If Approach guided, this value should be greater than 0 to ensure we are approaching our target, but if Flank guided, we rather want this to be a slight negative number, -0.1 for instance.")]
		private float minDirectness = -0.1f;

		[ApexSerialization]
		[FriendlyName("Maximum Directness", "If Retreat guided, this value should be less than 0 to ensure we are retreating from our target, but if Flank guided, we rather want this to be a slight positive number, 0.1 for instance.")]
		private float maxDirectness = 0.1f;

		[ApexSerialization]
		[FriendlyName("Behaviour Guide", "If Approach guided, min value over 0 should be used.\nIf Retreat guided, max value under 0 should be used.\nIf Flank guided, a min and max value around 0 (min: -0.1, max: 0.1) should be used.")]
		private Guide guide = Guide.Flank;

		public override float GetScore(BaseContext c, Vector3 point)
		{
			if (c.AIAgent.AttackTarget == null)
			{
				return 0f;
			}
			float score = base.GetScore(c, point);
			switch (guide)
			{
			case Guide.Flank:
				if (score >= minDirectness && score <= maxDirectness)
				{
					return 1f;
				}
				break;
			case Guide.Approach:
				if (minDirectness > 0f && score >= minDirectness)
				{
					return 1f;
				}
				break;
			case Guide.Retreat:
				if (maxDirectness < 0f && score <= maxDirectness)
				{
					return 1f;
				}
				break;
			default:
				return 0f;
			}
			return 0f;
		}
	}
}
