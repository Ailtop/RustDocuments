using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistJunkpile.Reasoners
{
	public class EnemyPlayerLineOfSightReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistJunkpileContext scientistJunkpileContext = npc.AiDomain.NpcContext as ScientistJunkpileContext;
			if (scientistJunkpileContext == null)
			{
				return;
			}
			scientistJunkpileContext.SetFact(Facts.CanSeeEnemy, scientistJunkpileContext.EnemyPlayersInLineOfSight.Count > 0);
			float num = 0f;
			NpcPlayerInfo primaryEnemyPlayerInLineOfSight = default(NpcPlayerInfo);
			foreach (NpcPlayerInfo item in npc.AiDomain.NpcContext.EnemyPlayersInLineOfSight)
			{
				if (scientistJunkpileContext.Memory.MarkedEnemies.Contains(item.Player))
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
					scientistJunkpileContext.Memory.RememberEnemyPlayer(npc, ref info, time, 0f, "SEE!");
				}
			}
			scientistJunkpileContext.PrimaryEnemyPlayerInLineOfSight = primaryEnemyPlayerInLineOfSight;
			if (primaryEnemyPlayerInLineOfSight.Player != null && (scientistJunkpileContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null || scientistJunkpileContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.AudibleScore < num))
			{
				scientistJunkpileContext.Memory.RememberPrimaryEnemyPlayer(primaryEnemyPlayerInLineOfSight.Player);
				scientistJunkpileContext.IncrementFact(Facts.Alertness, 2);
			}
		}
	}
}
