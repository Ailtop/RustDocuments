using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistAStar.Reasoners
{
	public class AtHomeLocationReasoner : INpcReasoner
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
			ScientistAStarContext scientistAStarContext = npc.AiDomain.NpcContext as ScientistAStarContext;
			if (scientistAStarContext == null)
			{
				return;
			}
			if ((scientistAStarContext.BodyPosition - scientistAStarContext.Domain.SpawnPosition).sqrMagnitude < 3f)
			{
				scientistAStarContext.SetFact(Facts.AtLocationHome, true);
				return;
			}
			BasePathNode closestToPoint = scientistAStarContext.Domain.Path.GetClosestToPoint(scientistAStarContext.Domain.SpawnPosition);
			if (closestToPoint != null && closestToPoint.transform != null && (scientistAStarContext.BodyPosition - closestToPoint.transform.position).sqrMagnitude < 3f)
			{
				scientistAStarContext.SetFact(Facts.AtLocationHome, true);
			}
			scientistAStarContext.SetFact(Facts.AtLocationHome, false);
		}
	}
}
