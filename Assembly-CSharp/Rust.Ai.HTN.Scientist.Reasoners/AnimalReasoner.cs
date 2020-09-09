using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Scientist.Reasoners
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
			ScientistContext scientistContext = npc.AiDomain.NpcContext as ScientistContext;
			if (scientistContext == null)
			{
				return;
			}
			BaseNpc baseNpc = null;
			float num = float.MaxValue;
			for (int i = 0; i < scientistContext.AnimalsInRange.Count; i++)
			{
				AnimalInfo animalInfo = scientistContext.AnimalsInRange[i];
				if (animalInfo.Animal != null && animalInfo.SqrDistance < num)
				{
					num = animalInfo.SqrDistance;
					baseNpc = animalInfo.Animal;
				}
			}
			if (baseNpc != null)
			{
				AttackEntity firearm = scientistContext.Domain.GetFirearm();
				if (num < npc.AiDefinition.Engagement.SqrCloseRangeFirearm(firearm))
				{
					scientistContext.Memory.RememberPrimaryAnimal(baseNpc);
					scientistContext.SetFact(Facts.NearbyAnimal, true);
					return;
				}
			}
			scientistContext.SetFact(Facts.NearbyAnimal, false);
		}
	}
}
