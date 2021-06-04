using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Scientist.Reasoners
{
	public class HealthReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistContext scientistContext = npc.AiDomain.NpcContext as ScientistContext;
			if (scientistContext != null)
			{
				float healthFraction = npc.healthFraction;
				if (healthFraction > 0.9f)
				{
					scientistContext.SetFact(Facts.HealthState, HealthState.FullHealth);
				}
				else if (healthFraction > 0.6f)
				{
					scientistContext.SetFact(Facts.HealthState, HealthState.HighHealth);
				}
				else if (healthFraction > 0.3f)
				{
					scientistContext.SetFact(Facts.HealthState, HealthState.MediumHealth);
				}
				else if (healthFraction > 0f)
				{
					scientistContext.SetFact(Facts.HealthState, HealthState.LowHealth);
				}
				else
				{
					scientistContext.SetFact(Facts.HealthState, HealthState.Dead);
				}
			}
		}
	}
}
