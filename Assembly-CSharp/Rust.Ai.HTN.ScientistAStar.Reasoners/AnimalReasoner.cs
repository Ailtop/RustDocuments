using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistAStar.Reasoners
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
			ScientistAStarContext scientistAStarContext = npc.AiDomain.NpcContext as ScientistAStarContext;
			if (scientistAStarContext == null)
			{
				return;
			}
			BaseNpc baseNpc = null;
			float num = float.MaxValue;
			for (int i = 0; i < scientistAStarContext.AnimalsInRange.Count; i++)
			{
				AnimalInfo animalInfo = scientistAStarContext.AnimalsInRange[i];
				if (animalInfo.Animal != null && animalInfo.SqrDistance < num)
				{
					num = animalInfo.SqrDistance;
					baseNpc = animalInfo.Animal;
				}
			}
			if (baseNpc != null)
			{
				AttackEntity firearm = scientistAStarContext.Domain.GetFirearm();
				if (num < npc.AiDefinition.Engagement.SqrCloseRangeFirearm(firearm))
				{
					scientistAStarContext.Memory.RememberPrimaryAnimal(baseNpc);
					scientistAStarContext.SetFact(Facts.NearbyAnimal, true);
					return;
				}
			}
			scientistAStarContext.SetFact(Facts.NearbyAnimal, false);
		}
	}
}
