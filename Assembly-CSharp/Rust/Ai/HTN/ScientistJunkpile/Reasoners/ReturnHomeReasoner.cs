using Rust.Ai.HTN.Reasoning;
using UnityEngine.AI;

namespace Rust.Ai.HTN.ScientistJunkpile.Reasoners
{
	public class ReturnHomeReasoner : INpcReasoner
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
			if (!scientistJunkpileContext.IsFact(Facts.IsReturningHome))
			{
				if ((scientistJunkpileContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null || time - scientistJunkpileContext.Memory.PrimaryKnownEnemyPlayer.Time > scientistJunkpileContext.Body.AiDefinition.Memory.NoSeeReturnToSpawnTime) && scientistJunkpileContext.Domain.SqrDistanceToSpawn() > scientistJunkpileContext.Domain.SqrMovementRadius)
				{
					scientistJunkpileContext.SetFact(Facts.IsReturningHome, true);
					NavMeshHit hit;
					if (NavMesh.SamplePosition(scientistJunkpileContext.Domain.SpawnPosition, out hit, 1f, scientistJunkpileContext.Domain.NavAgent.areaMask))
					{
						scientistJunkpileContext.Domain.SetDestination(hit.position);
					}
				}
			}
			else if (scientistJunkpileContext.IsFact(Facts.CanSeeEnemy) || time - scientistJunkpileContext.Body.lastAttackedTime < 2f || scientistJunkpileContext.IsFact(Facts.AtLocationHome))
			{
				scientistJunkpileContext.SetFact(Facts.IsReturningHome, false);
			}
		}
	}
}
