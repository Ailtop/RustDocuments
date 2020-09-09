using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Scientist.Reasoners
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
			ScientistContext scientistContext = npc.AiDomain.NpcContext as ScientistContext;
			if (scientistContext != null)
			{
				scientistContext.SetFact(Facts.CanSeeEnemy, scientistContext.EnemyPlayersInLineOfSight.Count > 0);
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
					scientistContext.Memory.RememberEnemyPlayer(npc, ref info, time, 0f, "SEE!");
				}
				scientistContext.PrimaryEnemyPlayerInLineOfSight = primaryEnemyPlayerInLineOfSight;
				if (primaryEnemyPlayerInLineOfSight.Player != null && (scientistContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null || scientistContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.AudibleScore < num))
				{
					scientistContext.Memory.RememberPrimaryEnemyPlayer(primaryEnemyPlayerInLineOfSight.Player);
					scientistContext.IncrementFact(Facts.Alertness, 2);
				}
			}
		}
	}
}
