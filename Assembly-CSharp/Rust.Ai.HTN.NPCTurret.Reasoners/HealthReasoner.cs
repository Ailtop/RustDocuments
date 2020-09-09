using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.NPCTurret.Reasoners
{
	public class HealthReasoner : INpcReasoner
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
			NPCTurretContext nPCTurretContext = npc.AiDomain.NpcContext as NPCTurretContext;
			if (nPCTurretContext != null)
			{
				float healthFraction = npc.healthFraction;
				if (healthFraction > 0.9f)
				{
					nPCTurretContext.SetFact(Facts.HealthState, HealthState.FullHealth);
				}
				else if (healthFraction > 0.6f)
				{
					nPCTurretContext.SetFact(Facts.HealthState, HealthState.HighHealth);
				}
				else if (healthFraction > 0.3f)
				{
					nPCTurretContext.SetFact(Facts.HealthState, HealthState.MediumHealth);
				}
				else if (healthFraction > 0f)
				{
					nPCTurretContext.SetFact(Facts.HealthState, HealthState.LowHealth);
				}
				else
				{
					nPCTurretContext.SetFact(Facts.HealthState, HealthState.Dead);
				}
			}
		}
	}
}
