using System;
using System.Collections.Generic;

namespace Rust.Ai.HTN.Sensors
{
	[Serializable]
	public class EnemyPlayersHearingSensor : INpcSensor
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			TickStatic(npc);
		}

		public static void TickStatic(IHTNAgent npc)
		{
			npc.AiDomain.NpcContext.EnemyPlayersAudible.Clear();
			List<NpcPlayerInfo> enemyPlayersInRange = npc.AiDomain.NpcContext.EnemyPlayersInRange;
			for (int i = 0; i < enemyPlayersInRange.Count; i++)
			{
				NpcPlayerInfo info = enemyPlayersInRange[i];
				if (info.Player == null || info.Player.transform == null || info.Player.IsDestroyed || info.Player.IsDead() || info.Player.IsWounded())
				{
					enemyPlayersInRange.RemoveAt(i);
					i--;
				}
				else
				{
					TickFootstepHearingTest(npc, ref info);
				}
			}
		}

		public static void TickFootstepHearingTest(IHTNAgent npc, ref NpcPlayerInfo info)
		{
			if (!(info.SqrDistance < npc.AiDefinition.Sensory.SqrHearingRange))
			{
				return;
			}
			float estimatedSpeed2D = info.Player.estimatedSpeed2D;
			if (estimatedSpeed2D <= 2f)
			{
				return;
			}
			if (estimatedSpeed2D <= 5f)
			{
				HTNPlayer hTNPlayer = npc as HTNPlayer;
				if ((bool)hTNPlayer)
				{
					AttackEntity ent = hTNPlayer.GetHeldEntity() as AttackEntity;
					if (info.SqrDistance < npc.AiDefinition.Engagement.SqrCloseRangeFirearm(ent))
					{
						npc.AiDomain.NpcContext.EnemyPlayersAudible.Add(info);
					}
				}
				else if (info.SqrDistance < npc.AiDefinition.Engagement.SqrCloseRange)
				{
					npc.AiDomain.NpcContext.EnemyPlayersAudible.Add(info);
				}
			}
			else
			{
				npc.AiDomain.NpcContext.EnemyPlayersAudible.Add(info);
			}
		}
	}
}
