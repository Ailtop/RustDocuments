using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Murderer.Reasoners
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
			MurdererContext murdererContext = npc.AiDomain.NpcContext as MurdererContext;
			if (murdererContext == null)
			{
				return;
			}
			BaseNpc baseNpc = null;
			float num = float.MaxValue;
			for (int i = 0; i < murdererContext.AnimalsInRange.Count; i++)
			{
				AnimalInfo animalInfo = murdererContext.AnimalsInRange[i];
				if (animalInfo.Animal != null && animalInfo.SqrDistance < num)
				{
					num = animalInfo.SqrDistance;
					baseNpc = animalInfo.Animal;
				}
			}
			if (baseNpc != null)
			{
				AttackEntity firearm = murdererContext.Domain.GetFirearm();
				if (num < npc.AiDefinition.Engagement.SqrCloseRangeFirearm(firearm))
				{
					murdererContext.Memory.RememberPrimaryAnimal(baseNpc);
					murdererContext.SetFact(Facts.NearbyAnimal, true);
					return;
				}
			}
			murdererContext.SetFact(Facts.NearbyAnimal, false);
		}
	}
}
