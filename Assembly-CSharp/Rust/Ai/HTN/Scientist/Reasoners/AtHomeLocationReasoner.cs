using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Scientist.Reasoners
{
	public class AtHomeLocationReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistContext scientistContext = npc.AiDomain.NpcContext as ScientistContext;
			if (scientistContext != null)
			{
				if ((scientistContext.BodyPosition - scientistContext.Domain.SpawnPosition).sqrMagnitude < 3f)
				{
					scientistContext.SetFact(Facts.AtLocationHome, true);
				}
				else
				{
					scientistContext.SetFact(Facts.AtLocationHome, false);
				}
			}
		}
	}
}
