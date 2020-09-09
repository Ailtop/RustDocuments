using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistJunkpile.Reasoners
{
	public class AnimalReasoner : INpcReasoner
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
			BaseNpc baseNpc = null;
			float num = float.MaxValue;
			for (int i = 0; i < scientistJunkpileContext.AnimalsInRange.Count; i++)
			{
				AnimalInfo animalInfo = scientistJunkpileContext.AnimalsInRange[i];
				if (animalInfo.Animal != null && animalInfo.SqrDistance < num)
				{
					num = animalInfo.SqrDistance;
					baseNpc = animalInfo.Animal;
				}
			}
			if (baseNpc != null && IsNearby(scientistJunkpileContext.Domain, num))
			{
				scientistJunkpileContext.Memory.RememberPrimaryAnimal(baseNpc);
				scientistJunkpileContext.SetFact(Facts.NearbyAnimal, true);
			}
			else
			{
				scientistJunkpileContext.SetFact(Facts.NearbyAnimal, false);
			}
		}

		public static bool IsNearby(ScientistJunkpileDomain domain, float sqrDistance)
		{
			AttackEntity firearm = domain.GetFirearm();
			return sqrDistance < domain.ScientistDefinition.Engagement.SqrCloseRangeFirearm(firearm) + 4f;
		}
	}
}
