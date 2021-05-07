using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistJunkpile.Reasoners
{
	public class AtHomeLocationReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistJunkpileContext scientistJunkpileContext = npc.AiDomain.NpcContext as ScientistJunkpileContext;
			if (scientistJunkpileContext != null)
			{
				if ((scientistJunkpileContext.BodyPosition - scientistJunkpileContext.Domain.SpawnPosition).sqrMagnitude < 3f)
				{
					scientistJunkpileContext.SetFact(Facts.AtLocationHome, true);
				}
				else
				{
					scientistJunkpileContext.SetFact(Facts.AtLocationHome, false);
				}
			}
		}
	}
}
