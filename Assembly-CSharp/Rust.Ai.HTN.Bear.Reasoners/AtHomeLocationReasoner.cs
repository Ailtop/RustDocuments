using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Bear.Reasoners
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
			BearContext bearContext = npc.AiDomain.NpcContext as BearContext;
			if (bearContext != null)
			{
				if ((bearContext.BodyPosition - bearContext.Domain.SpawnPosition).sqrMagnitude < 3f)
				{
					bearContext.SetFact(Facts.AtLocationHome, true);
				}
				else
				{
					bearContext.SetFact(Facts.AtLocationHome, false);
				}
			}
		}
	}
}
