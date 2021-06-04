using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistAStar.Reasoners
{
	public class HealthReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistAStarContext scientistAStarContext = npc.AiDomain.NpcContext as ScientistAStarContext;
			if (scientistAStarContext != null)
			{
				float healthFraction = npc.healthFraction;
				if (healthFraction > 0.9f)
				{
					scientistAStarContext.SetFact(Facts.HealthState, HealthState.FullHealth);
				}
				else if (healthFraction > 0.6f)
				{
					scientistAStarContext.SetFact(Facts.HealthState, HealthState.HighHealth);
				}
				else if (healthFraction > 0.3f)
				{
					scientistAStarContext.SetFact(Facts.HealthState, HealthState.MediumHealth);
				}
				else if (healthFraction > 0f)
				{
					scientistAStarContext.SetFact(Facts.HealthState, HealthState.LowHealth);
				}
				else
				{
					scientistAStarContext.SetFact(Facts.HealthState, HealthState.Dead);
				}
			}
		}
	}
}
