using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Scientist.Reasoners
{
	public class ExplosivesReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistContext scientistContext = npc.AiDomain.NpcContext as ScientistContext;
			if (scientistContext == null)
			{
				return;
			}
			for (int i = 0; i < scientistContext.Memory.KnownTimedExplosives.Count; i++)
			{
				BaseNpcMemory.EntityOfInterestInfo entityOfInterestInfo = scientistContext.Memory.KnownTimedExplosives[i];
				if (entityOfInterestInfo.Entity != null)
				{
					AttackEntity firearm = scientistContext.Domain.GetFirearm();
					if ((entityOfInterestInfo.Entity.transform.position - scientistContext.BodyPosition).sqrMagnitude < scientistContext.Body.AiDefinition.Engagement.SqrCloseRangeFirearm(firearm))
					{
						scientistContext.SetFact(Facts.NearbyExplosives, true);
						scientistContext.IncrementFact(Facts.Alertness, 2);
						return;
					}
				}
			}
			scientistContext.SetFact(Facts.NearbyExplosives, false);
		}
	}
}
