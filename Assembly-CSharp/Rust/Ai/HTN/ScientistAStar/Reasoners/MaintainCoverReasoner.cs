using Rust.Ai.HTN.Reasoning;
using UnityEngine;

namespace Rust.Ai.HTN.ScientistAStar.Reasoners
{
	public class MaintainCoverReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistAStarContext scientistAStarContext = npc.AiDomain.NpcContext as ScientistAStarContext;
			if (scientistAStarContext != null && scientistAStarContext.IsFact(Facts.CanSeeEnemy) && (scientistAStarContext.IsFact(Facts.MaintainCover) || scientistAStarContext.IsFact(Facts.IsReloading) || scientistAStarContext.IsFact(Facts.IsApplyingMedical)) && (scientistAStarContext.ReservedCoverPoint == null || scientistAStarContext.ReservedCoverPoint.IsCompromised || scientistAStarContext.IsFact(Facts.AtLocationCover) || !(time - scientistAStarContext.ReservedCoverTime < 1f)) && ScientistAStarDomain.AStarCanNavigateToCoverLocation.Try(CoverTactic.Retreat, scientistAStarContext))
			{
				Vector3 coverPosition = ScientistAStarDomain.AStarNavigateToCover.GetCoverPosition(CoverTactic.Retreat, scientistAStarContext);
				scientistAStarContext.Domain.SetDestination(coverPosition);
			}
		}
	}
}
