using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Murderer.Reasoners
{
	public class EnemyPlayerLineOfSightReasoner : INpcReasoner
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
				murdererContext.SetFact(Facts.CanSeeEnemy, murdererContext.EnemyPlayersInLineOfSight.Count > 0);
				float num = 0f;
				NpcPlayerInfo primaryEnemyPlayerInLineOfSight = default(NpcPlayerInfo);
				foreach (NpcPlayerInfo item in npc.AiDomain.NpcContext.EnemyPlayersInLineOfSight)
				{
					float num2 = 1f - item.SqrDistance / npc.AiDefinition.Engagement.SqrAggroRange;
					float num3 = (item.ForwardDotDir + 1f) * 0.5f;
					float num4 = num2 * 2f + num3;
					if (num4 > num)
					{
						num = num4;
						primaryEnemyPlayerInLineOfSight = item;
					}
					NpcPlayerInfo info = item;
					info.VisibilityScore = num4;
					murdererContext.Memory.RememberEnemyPlayer(npc, ref info, time, 0f, "SEE!");
				}
				murdererContext.PrimaryEnemyPlayerInLineOfSight = primaryEnemyPlayerInLineOfSight;
				if (primaryEnemyPlayerInLineOfSight.Player != null && (murdererContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null || murdererContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.AudibleScore < num))
				{
					murdererContext.Memory.RememberPrimaryEnemyPlayer(primaryEnemyPlayerInLineOfSight.Player);
					murdererContext.IncrementFact(Facts.Alertness, 2);
				}
			}
		}
	}
}
