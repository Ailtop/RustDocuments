using Apex.AI;
using UnityEngine;

namespace Rust.Ai
{
	[FriendlyName("Move To Best Position", "Sets a move target based on the scorers and moves towards it")]
	public class MoveToBestPosition : BaseActionWithOptions<Vector3>
	{
		public override void DoExecute(BaseContext c)
		{
			Vector3 best = GetBest(c, c.sampledPositions);
			if (best.sqrMagnitude == 0f)
			{
				return;
			}
			NPCHumanContext nPCHumanContext = c as NPCHumanContext;
			if (nPCHumanContext != null && nPCHumanContext.CurrentCoverVolume != null)
			{
				for (int i = 0; i < nPCHumanContext.sampledCoverPoints.Count; i++)
				{
					CoverPoint coverPoint = nPCHumanContext.sampledCoverPoints[i];
					CoverPoint.CoverType coverType = nPCHumanContext.sampledCoverPointTypes[i];
					if (Vector3Ex.Distance2D(coverPoint.Position, best) < 1f)
					{
						nPCHumanContext.CoverSet.Update(coverPoint, coverPoint, coverPoint);
						break;
					}
				}
			}
			c.AIAgent.UpdateDestination(best);
			c.lastSampledPosition = best;
		}
	}
}
