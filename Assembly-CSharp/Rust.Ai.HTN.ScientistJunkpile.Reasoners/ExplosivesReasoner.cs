using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistJunkpile.Reasoners
{
	public class ExplosivesReasoner : INpcReasoner
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
			ScientistJunkpileContext scientistJunkpileContext = npc.AiDomain.NpcContext as ScientistJunkpileContext;
			if (scientistJunkpileContext == null)
			{
				return;
			}
			for (int i = 0; i < scientistJunkpileContext.Memory.KnownTimedExplosives.Count; i++)
			{
				BaseNpcMemory.EntityOfInterestInfo entityOfInterestInfo = scientistJunkpileContext.Memory.KnownTimedExplosives[i];
				if (entityOfInterestInfo.Entity != null)
				{
					AttackEntity firearm = scientistJunkpileContext.Domain.GetFirearm();
					if ((entityOfInterestInfo.Entity.transform.position - scientistJunkpileContext.BodyPosition).sqrMagnitude < scientistJunkpileContext.Body.AiDefinition.Engagement.SqrCloseRangeFirearm(firearm))
					{
						scientistJunkpileContext.SetFact(Facts.NearbyExplosives, true);
						scientistJunkpileContext.IncrementFact(Facts.Alertness, 2);
						return;
					}
				}
			}
			scientistJunkpileContext.SetFact(Facts.NearbyExplosives, false);
		}
	}
}
