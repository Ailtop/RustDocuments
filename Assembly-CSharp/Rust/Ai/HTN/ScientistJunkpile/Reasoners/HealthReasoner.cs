using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistJunkpile.Reasoners
{
	public class HealthReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistJunkpileContext scientistJunkpileContext = npc.AiDomain.NpcContext as ScientistJunkpileContext;
			if (scientistJunkpileContext != null)
			{
				float healthFraction = npc.healthFraction;
				if (healthFraction > 0.9f)
				{
					scientistJunkpileContext.SetFact(Facts.HealthState, HealthState.FullHealth);
				}
				else if (healthFraction > 0.6f)
				{
					scientistJunkpileContext.SetFact(Facts.HealthState, HealthState.HighHealth);
				}
				else if (healthFraction > 0.3f)
				{
					scientistJunkpileContext.SetFact(Facts.HealthState, HealthState.MediumHealth);
				}
				else if (healthFraction > 0f)
				{
					scientistJunkpileContext.SetFact(Facts.HealthState, HealthState.LowHealth);
				}
				else
				{
					scientistJunkpileContext.SetFact(Facts.HealthState, HealthState.Dead);
				}
			}
		}
	}
}
