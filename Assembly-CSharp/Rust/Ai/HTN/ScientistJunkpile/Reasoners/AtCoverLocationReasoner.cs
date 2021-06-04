using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistJunkpile.Reasoners
{
	public class AtCoverLocationReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistJunkpileContext scientistJunkpileContext = npc.AiDomain.NpcContext as ScientistJunkpileContext;
			if (scientistJunkpileContext != null)
			{
				if (scientistJunkpileContext.ReservedCoverPoint == null)
				{
					scientistJunkpileContext.SetFact(Facts.AtLocationCover, false);
					scientistJunkpileContext.SetFact(Facts.CoverState, CoverState.None);
				}
				else if ((scientistJunkpileContext.ReservedCoverPoint.Position - scientistJunkpileContext.Body.transform.position).sqrMagnitude < 1f)
				{
					scientistJunkpileContext.SetFact(Facts.AtLocationCover, true);
					scientistJunkpileContext.SetFact(Facts.CoverState, (scientistJunkpileContext.ReservedCoverPoint.NormalCoverType == CoverPoint.CoverType.Partial) ? CoverState.Partial : CoverState.Full);
				}
				else
				{
					scientistJunkpileContext.SetFact(Facts.AtLocationCover, false);
					scientistJunkpileContext.SetFact(Facts.CoverState, CoverState.None);
				}
			}
		}
	}
}
