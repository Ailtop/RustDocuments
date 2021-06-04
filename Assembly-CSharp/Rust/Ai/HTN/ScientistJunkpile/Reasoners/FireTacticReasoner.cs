using Rust.Ai.HTN.Reasoning;
using UnityEngine;

namespace Rust.Ai.HTN.ScientistJunkpile.Reasoners
{
	public class FireTacticReasoner : INpcReasoner
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
				if (scientistJunkpileContext.PrimaryEnemyPlayerInLineOfSight.Player != null)
				{
					num = scientistJunkpileContext.PrimaryEnemyPlayerInLineOfSight.SqrDistance;
					if (Mathf.Approximately(num, 0f))
					{
						num = float.MaxValue;
					}
				}
				else if (scientistJunkpileContext.Memory.PrimaryKnownAnimal.Animal != null)
				{
					num = scientistJunkpileContext.Memory.PrimaryKnownAnimal.SqrDistance;
					if (Mathf.Approximately(num, 0f))
					{
						num = float.MaxValue;
					}
				}
				value = ((attackEntity.attackLengthMin >= 0f && num <= scientistJunkpileContext.Body.AiDefinition.Engagement.SqrCloseRangeFirearm(ent)) ? FireTactic.FullAuto : ((!(attackEntity.attackLengthMin >= 0f) || !(num <= scientistJunkpileContext.Body.AiDefinition.Engagement.SqrMediumRangeFirearm(ent))) ? FireTactic.Single : FireTactic.Burst));
			}
			scientistJunkpileContext.SetFact(Facts.FireTactic, value);
		}
	}
}
