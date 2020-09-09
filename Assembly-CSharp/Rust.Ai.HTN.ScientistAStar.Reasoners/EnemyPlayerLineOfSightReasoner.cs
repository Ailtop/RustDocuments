using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistAStar.Reasoners
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
			ScientistAStarContext scientistAStarContext = npc.AiDomain.NpcContext as ScientistAStarContext;
			if (scientistAStarContext != null)
			{
				scientistAStarContext.SetFact(Facts.CanSeeEnemy, scientistAStarContext.EnemyPlayersInLineOfSight.Count > 0);
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
					scientistAStarContext.Memory.RememberEnemyPlayer(npc, ref info, time, 0f, "SEE!");
				}
				scientistAStarContext.PrimaryEnemyPlayerInLineOfSight = primaryEnemyPlayerInLineOfSight;
				if (primaryEnemyPlayerInLineOfSight.Player != null && (scientistAStarContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null || scientistAStarContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.AudibleScore < num))
				{
					scientistAStarContext.Memory.RememberPrimaryEnemyPlayer(primaryEnemyPlayerInLineOfSight.Player);
					scientistAStarContext.IncrementFact(Facts.Alertness, 2);
				}
			}
		}
	}
}
