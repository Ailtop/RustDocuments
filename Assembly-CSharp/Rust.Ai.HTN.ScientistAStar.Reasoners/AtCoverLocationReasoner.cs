using Rust.Ai.HTN.Reasoning;
using UnityEngine;

namespace Rust.Ai.HTN.ScientistAStar.Reasoners
{
	public class AtCoverLocationReasoner : INpcReasoner
	{
		public float TickFrequency
		{
			get;
			set;
		}

		public float LastTickTime
		{
			get;
			set;
		}

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistAStarContext scientistAStarContext = npc.AiDomain.NpcContext as ScientistAStarContext;
			if (scientistAStarContext == null)
			{
				return;
			}
			if (scientistAStarContext.ReservedCoverPoint == null)
			{
				scientistAStarContext.SetFact(Facts.AtLocationCover, false);
				scientistAStarContext.SetFact(Facts.CoverState, CoverState.None);
				return;
			}
			Vector3 position = scientistAStarContext.ReservedCoverPoint.Position;
			BasePathNode closestToPoint = scientistAStarContext.Domain.Path.GetClosestToPoint(position);
			if (closestToPoint != null && closestToPoint.transform != null && (closestToPoint.transform.position - scientistAStarContext.BodyPosition).sqrMagnitude < 1f)
			{
				scientistAStarContext.SetFact(Facts.AtLocationCover, true);
				scientistAStarContext.SetFact(Facts.CoverState, (scientistAStarContext.ReservedCoverPoint.NormalCoverType == CoverPoint.CoverType.Partial) ? CoverState.Partial : CoverState.Full);
			}
			else
			{
				scientistAStarContext.SetFact(Facts.AtLocationCover, false);
				scientistAStarContext.SetFact(Facts.CoverState, CoverState.None);
			}
		}
	}
}
