using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Scientist.Reasoners
{
	public class AtCoverLocationReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistContext scientistContext = npc.AiDomain.NpcContext as ScientistContext;
			if (scientistContext != null)
			{
				if (scientistContext.ReservedCoverPoint == null)
				{
					scientistContext.SetFact(Facts.AtLocationCover, false);
					scientistContext.SetFact(Facts.CoverState, CoverState.None);
				}
				else if ((scientistContext.ReservedCoverPoint.Position - scientistContext.Body.transform.position).sqrMagnitude < 1f)
				{
					scientistContext.SetFact(Facts.AtLocationCover, true);
					scientistContext.SetFact(Facts.CoverState, (scientistContext.ReservedCoverPoint.NormalCoverType == CoverPoint.CoverType.Partial) ? CoverState.Partial : CoverState.Full);
				}
				else
				{
					scientistContext.SetFact(Facts.AtLocationCover, false);
					scientistContext.SetFact(Facts.CoverState, CoverState.None);
				}
			}
		}
	}
}
