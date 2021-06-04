using Rust.Ai.HTN.Reasoning;
using UnityEngine;

namespace Rust.Ai.HTN.Scientist.Reasoners
{
	public class FireTacticReasoner : INpcReasoner
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
				if (scientistContext.PrimaryEnemyPlayerInLineOfSight.Player != null)
				{
					num = scientistContext.PrimaryEnemyPlayerInLineOfSight.SqrDistance;
					if (Mathf.Approximately(num, 0f))
					{
						num = float.MaxValue;
					}
				}
				value = ((attackEntity.attackLengthMin >= 0f && num <= scientistContext.Body.AiDefinition.Engagement.SqrCloseRangeFirearm(ent)) ? FireTactic.FullAuto : ((!(attackEntity.attackLengthMin >= 0f) || !(num <= scientistContext.Body.AiDefinition.Engagement.SqrMediumRangeFirearm(ent))) ? FireTactic.Single : FireTactic.Burst));
			}
			scientistContext.SetFact(Facts.FireTactic, value);
		}
	}
}
