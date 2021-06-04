using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistAStar.Reasoners
{
	public class ExplosivesReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistAStarContext scientistAStarContext = npc.AiDomain.NpcContext as ScientistAStarContext;
			if (scientistAStarContext == null)
			{
				return;
			}
			for (int i = 0; i < scientistAStarContext.Memory.KnownTimedExplosives.Count; i++)
			{
				BaseNpcMemory.EntityOfInterestInfo entityOfInterestInfo = scientistAStarContext.Memory.KnownTimedExplosives[i];
				if (entityOfInterestInfo.Entity != null)
				{
					AttackEntity firearm = scientistAStarContext.Domain.GetFirearm();
					if ((entityOfInterestInfo.Entity.transform.position - scientistAStarContext.BodyPosition).sqrMagnitude < scientistAStarContext.Body.AiDefinition.Engagement.SqrCloseRangeFirearm(firearm))
					{
						scientistAStarContext.SetFact(Facts.NearbyExplosives, true);
						scientistAStarContext.IncrementFact(Facts.Alertness, 2);
						return;
					}
				}
			}
			scientistAStarContext.SetFact(Facts.NearbyExplosives, false);
		}
	}
}
