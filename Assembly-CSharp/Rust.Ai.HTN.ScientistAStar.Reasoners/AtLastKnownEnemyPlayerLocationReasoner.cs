using Rust.Ai.HTN.Reasoning;
using UnityEngine;

namespace Rust.Ai.HTN.ScientistAStar.Reasoners
{
	public class AtLastKnownEnemyPlayerLocationReasoner : INpcReasoner
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
			if (scientistAStarContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null)
			{
				Vector3 destination = ScientistAStarDomain.AStarNavigateToLastKnownLocationOfPrimaryEnemyPlayer.GetDestination(scientistAStarContext);
				BasePathNode closestToPoint = scientistAStarContext.Domain.Path.GetClosestToPoint(destination);
				if (closestToPoint != null && closestToPoint.transform != null && (closestToPoint.transform.position - scientistAStarContext.BodyPosition).sqrMagnitude < 1f)
				{
					scientistAStarContext.SetFact(Facts.AtLocationLastKnownLocationOfPrimaryEnemyPlayer, 1);
					return;
				}
			}
			scientistAStarContext.SetFact(Facts.AtLocationLastKnownLocationOfPrimaryEnemyPlayer, 0);
		}
	}
}
