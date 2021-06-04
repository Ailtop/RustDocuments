using Rust.Ai.HTN.Reasoning;
using UnityEngine;

namespace Rust.Ai.HTN.Scientist.Reasoners
{
	public class EnemyPlayerHearingReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistContext scientistContext = npc.AiDomain.NpcContext as ScientistContext;
			if (scientistContext == null)
			{
				return;
			}
			scientistContext.SetFact(Facts.CanHearEnemy, scientistContext.EnemyPlayersAudible.Count > 0);
			float num = 0f;
			NpcPlayerInfo primaryEnemyPlayerAudible = default(NpcPlayerInfo);
			foreach (NpcPlayerInfo item in npc.AiDomain.NpcContext.EnemyPlayersAudible)
			{
				if (!(item.SqrDistance > npc.AiDefinition.Sensory.SqrHearingRange))
				{
					float num2 = 1f - Mathf.Min(1f, item.SqrDistance / npc.AiDefinition.Sensory.SqrHearingRange);
					float num3 = num2 * 2f;
					if (num3 > num)
					{
						num = num3;
						primaryEnemyPlayerAudible = item;
					}
					NpcPlayerInfo info = item;
					info.AudibleScore = num3;
					scientistContext.Memory.RememberEnemyPlayer(npc, ref info, time, (1f - num2) * 20f, "SOUND!");
				}
			}
			scientistContext.PrimaryEnemyPlayerAudible = primaryEnemyPlayerAudible;
			if (primaryEnemyPlayerAudible.Player != null && (scientistContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null || scientistContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.VisibilityScore < num))
			{
				scientistContext.Memory.RememberPrimaryEnemyPlayer(primaryEnemyPlayerAudible.Player);
				scientistContext.IncrementFact(Facts.Alertness, 1);
			}
		}
	}
}
