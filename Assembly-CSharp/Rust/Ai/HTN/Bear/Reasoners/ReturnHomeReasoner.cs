using Rust.Ai.HTN.Reasoning;
using UnityEngine.AI;

namespace Rust.Ai.HTN.Bear.Reasoners
{
	public class ReturnHomeReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			BearContext bearContext = npc.AiDomain.NpcContext as BearContext;
			if (bearContext == null)
			{
				return;
			}
			if (!bearContext.IsFact(Facts.IsReturningHome))
			{
				if (bearContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null || time - bearContext.Memory.PrimaryKnownEnemyPlayer.Time > bearContext.Body.AiDefinition.Memory.NoSeeReturnToSpawnTime)
				{
					bearContext.SetFact(Facts.IsReturningHome, true);
					NavMeshHit hit;
					if (NavMesh.SamplePosition(bearContext.Domain.SpawnPosition, out hit, 1f, bearContext.Domain.NavAgent.areaMask))
					{
						bearContext.Domain.SetDestination(hit.position);
					}
				}
			}
			else if (bearContext.IsFact(Facts.CanSeeEnemy) || time - bearContext.Body.lastAttackedTime < 2f || bearContext.IsFact(Facts.AtLocationHome))
			{
				bearContext.SetFact(Facts.IsReturningHome, false);
			}
		}
	}
}
