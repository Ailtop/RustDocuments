using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistAStar.Reasoners
{
	public class AtNextAStarWaypointLocationReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistAStarContext scientistAStarContext = npc.AiDomain.NpcContext as ScientistAStarContext;
			if (scientistAStarContext != null)
			{
				if (scientistAStarContext.Domain.IsAtFinalDestination())
				{
					scientistAStarContext.SetFact(Facts.AtLocationNextAStarWaypoint, 1);
				}
				scientistAStarContext.SetFact(Facts.AtLocationNextAStarWaypoint, 0);
			}
		}
	}
}
