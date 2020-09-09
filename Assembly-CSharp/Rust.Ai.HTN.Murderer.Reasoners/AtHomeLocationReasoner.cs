using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Murderer.Reasoners
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
			MurdererContext murdererContext = npc.AiDomain.NpcContext as MurdererContext;
			if (murdererContext != null)
			{
				if ((murdererContext.BodyPosition - murdererContext.Domain.SpawnPosition).sqrMagnitude < 3f)
				{
					murdererContext.SetFact(Facts.AtLocationHome, true);
				}
				else
				{
					murdererContext.SetFact(Facts.AtLocationHome, false);
				}
			}
		}
	}
}
