using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Bear.Reasoners
{
	public class HealthReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			BearContext bearContext = npc.AiDomain.NpcContext as BearContext;
			if (bearContext != null)
			{
				float healthFraction = npc.healthFraction;
				if (healthFraction > 0.9f)
				{
					bearContext.SetFact(Facts.HealthState, HealthState.FullHealth);
				}
				else if (healthFraction > 0.6f)
				{
					bearContext.SetFact(Facts.HealthState, HealthState.HighHealth);
				}
				else if (healthFraction > 0.3f)
				{
					bearContext.SetFact(Facts.HealthState, HealthState.MediumHealth);
				}
				else if (healthFraction > 0f)
				{
					bearContext.SetFact(Facts.HealthState, HealthState.LowHealth);
				}
				else
				{
					bearContext.SetFact(Facts.HealthState, HealthState.Dead);
				}
			}
		}
	}
}
