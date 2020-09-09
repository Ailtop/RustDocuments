using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.NPCTurret.Reasoners
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
			NPCTurretContext nPCTurretContext = npc.AiDomain.NpcContext as NPCTurretContext;
			if (nPCTurretContext == null)
			{
				return;
			}
			BaseNpc baseNpc = null;
			float num = float.MaxValue;
			for (int i = 0; i < nPCTurretContext.AnimalsInRange.Count; i++)
			{
				AnimalInfo animalInfo = nPCTurretContext.AnimalsInRange[i];
				if (animalInfo.Animal != null && animalInfo.SqrDistance < num)
				{
					num = animalInfo.SqrDistance;
					baseNpc = animalInfo.Animal;
				}
			}
			if (baseNpc != null)
			{
				AttackEntity firearm = nPCTurretContext.Domain.GetFirearm();
				if (num < npc.AiDefinition.Engagement.SqrCloseRangeFirearm(firearm))
				{
					nPCTurretContext.Memory.RememberPrimaryAnimal(baseNpc);
					nPCTurretContext.SetFact(Facts.NearbyAnimal, true);
					return;
				}
			}
			nPCTurretContext.SetFact(Facts.NearbyAnimal, false);
		}
	}
}
