using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Bear.Reasoners
{
	public class PreferredFightingRangeReasoner : INpcReasoner
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
			NpcPlayerInfo target = bearContext.GetPrimaryEnemyPlayerTarget();
			if (target.Player != null)
			{
				if (IsAtPreferredRange(bearContext, ref target))
				{
					bearContext.SetFact(Facts.AtLocationPreferredFightingRange, 1);
				}
				else
				{
					bearContext.SetFact(Facts.AtLocationPreferredFightingRange, 0);
				}
			}
		}

		public static bool IsAtPreferredRange(BearContext context, ref NpcPlayerInfo target)
		{
			return target.SqrDistance <= context.Body.AiDefinition.Engagement.SqrCloseRange;
		}
	}
}
