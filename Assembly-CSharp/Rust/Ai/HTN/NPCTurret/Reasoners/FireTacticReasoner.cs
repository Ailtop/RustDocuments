using Rust.Ai.HTN.Reasoning;
using UnityEngine;

namespace Rust.Ai.HTN.NPCTurret.Reasoners
{
	public class FireTacticReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			NPCTurretContext nPCTurretContext = npc.AiDomain.NpcContext as NPCTurretContext;
			if (nPCTurretContext == null)
			{
				return;
			}
			HTNPlayer hTNPlayer = npc as HTNPlayer;
			if (hTNPlayer == null)
			{
				return;
			}
			FireTactic value = FireTactic.Single;
			AttackEntity attackEntity = hTNPlayer.GetHeldEntity() as AttackEntity;
			if ((bool)attackEntity)
			{
				BaseProjectile ent = attackEntity as BaseProjectile;
				float num = float.MaxValue;
				if (nPCTurretContext.PrimaryEnemyPlayerInLineOfSight.Player != null)
				{
					num = nPCTurretContext.PrimaryEnemyPlayerInLineOfSight.SqrDistance;
					if (Mathf.Approximately(num, 0f))
					{
						num = float.MaxValue;
					}
				}
				value = ((attackEntity.attackLengthMin >= 0f && num <= nPCTurretContext.Body.AiDefinition.Engagement.SqrCloseRangeFirearm(ent)) ? FireTactic.FullAuto : ((!(attackEntity.attackLengthMin >= 0f) || !(num <= nPCTurretContext.Body.AiDefinition.Engagement.SqrMediumRangeFirearm(ent))) ? ((!(attackEntity.attackLengthMin > 0f) || !nPCTurretContext.Domain.BurstAtLongRange || !(num > nPCTurretContext.Body.AiDefinition.Engagement.SqrMediumRangeFirearm(ent))) ? FireTactic.Single : FireTactic.Burst) : FireTactic.Burst));
			}
			nPCTurretContext.SetFact(Facts.FireTactic, value);
		}
	}
}
