using Rust.Ai.HTN.Reasoning;
using UnityEngine;

namespace Rust.Ai.HTN.ScientistJunkpile.Reasoners
{
	public class MaintainCoverReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistJunkpileContext scientistJunkpileContext = npc.AiDomain.NpcContext as ScientistJunkpileContext;
			if (scientistJunkpileContext != null && (scientistJunkpileContext.IsFact(Facts.MaintainCover) || scientistJunkpileContext.IsFact(Facts.IsReloading) || scientistJunkpileContext.IsFact(Facts.IsApplyingMedical)) && (scientistJunkpileContext.ReservedCoverPoint == null || scientistJunkpileContext.ReservedCoverPoint.IsCompromised || scientistJunkpileContext.IsFact(Facts.AtLocationCover) || !(time - scientistJunkpileContext.ReservedCoverTime < 0.8f)) && (scientistJunkpileContext.IsFact(Facts.CanSeeEnemy) || !(scientistJunkpileContext.Body.SecondsSinceAttacked - 1f > time - scientistJunkpileContext.ReservedCoverTime)) && ScientistJunkpileDomain.JunkpileCanNavigateToCoverLocation.Try(CoverTactic.Retreat, scientistJunkpileContext))
			{
				Vector3 coverPosition = ScientistJunkpileDomain.JunkpileNavigateToCover.GetCoverPosition(CoverTactic.Retreat, scientistJunkpileContext);
				scientistJunkpileContext.Domain.SetDestination(coverPosition);
				scientistJunkpileContext.Body.modelState.ducked = false;
				scientistJunkpileContext.SetFact(Facts.IsDucking, 0, false);
				scientistJunkpileContext.SetFact(Facts.FirearmOrder, FirearmOrders.FireAtWill, false);
			}
		}
	}
}
