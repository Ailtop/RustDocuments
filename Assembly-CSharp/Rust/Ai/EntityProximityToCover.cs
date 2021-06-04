using Apex.Serialization;
using UnityEngine;

namespace Rust.Ai
{
	public class EntityProximityToCover : BaseScorer
	{
		[ApexSerialization]
		public float MaxDistance = 20f;

		[ApexSerialization]
		public ProximityToCover.CoverType _coverType;

		[ApexSerialization]
		public AnimationCurve Response = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

		public override float GetScore(BaseContext ctx)
		{
			NPCHumanContext nPCHumanContext = ctx as NPCHumanContext;
			if (nPCHumanContext != null)
			{
				float bestDistance;
				CoverPoint closestCover = ProximityToCover.GetClosestCover(nPCHumanContext, nPCHumanContext.Position, MaxDistance, _coverType, out bestDistance);
				if (closestCover != null)
				{
					return Response.Evaluate(bestDistance / MaxDistance) * closestCover.Score;
				}
			}
			return 0f;
		}
	}
}
