using Rust.Ai.HTN.Reasoning;
using UnityEngine;

namespace Rust.Ai.HTN.Scientist.Reasoners
{
	public class MaintainCoverReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistContext scientistContext = npc.AiDomain.NpcContext as ScientistContext;
			if (scientistContext != null && (scientistContext.IsFact(Facts.MaintainCover) || scientistContext.IsFact(Facts.IsReloading) || scientistContext.IsFact(Facts.IsApplyingMedical)) && (scientistContext.ReservedCoverPoint == null || scientistContext.ReservedCoverPoint.IsCompromised || scientistContext.IsFact(Facts.AtLocationCover) || !(time - scientistContext.ReservedCoverTime < 0.8f)) && (scientistContext.IsFact(Facts.CanSeeEnemy) || !(scientistContext.Body.SecondsSinceAttacked - 1f > time - scientistContext.ReservedCoverTime)) && ScientistDomain.CanNavigateToCoverLocation.Try(CoverTactic.Retreat, scientistContext))
			{
				Vector3 coverPosition = ScientistDomain.NavigateToCover.GetCoverPosition(CoverTactic.Retreat, scientistContext);
				scientistContext.Domain.SetDestination(coverPosition);
				scientistContext.Body.modelState.ducked = false;
				scientistContext.SetFact(Facts.IsDucking, 0, false);
				scientistContext.SetFact(Facts.FirearmOrder, FirearmOrders.FireAtWill, false);
			}
		}
	}
}
