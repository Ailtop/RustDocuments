using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Bear.Reasoners
{
	public class AnimalReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			BearContext bearContext = npc.AiDomain.NpcContext as BearContext;
			if (bearContext == null)
			{
				return;
			}
			BaseNpc baseNpc = null;
			float num = float.MaxValue;
			for (int i = 0; i < bearContext.AnimalsInRange.Count; i++)
			{
				AnimalInfo animalInfo = bearContext.AnimalsInRange[i];
				if (animalInfo.Animal != null && animalInfo.SqrDistance < num)
				{
					num = animalInfo.SqrDistance;
					baseNpc = animalInfo.Animal;
				}
			}
			if (baseNpc != null && num < npc.AiDefinition.Engagement.SqrMediumRange)
			{
				bearContext.Memory.RememberPrimaryAnimal(baseNpc);
				bearContext.SetFact(Facts.NearbyAnimal, true);
			}
			else
			{
				bearContext.SetFact(Facts.NearbyAnimal, false);
			}
		}
	}
}
