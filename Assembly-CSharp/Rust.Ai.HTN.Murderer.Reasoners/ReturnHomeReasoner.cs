using Rust.Ai.HTN.Reasoning;
using UnityEngine.AI;

namespace Rust.Ai.HTN.Murderer.Reasoners
{
	public class ReturnHomeReasoner : INpcReasoner
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
			MurdererContext murdererContext = npc.AiDomain.NpcContext as MurdererContext;
			if (murdererContext == null)
			{
				return;
			}
			if (!murdererContext.IsFact(Facts.AtLocationHome) && !murdererContext.IsFact(Facts.IsReturningHome) && !murdererContext.IsFact(Facts.IsNavigating) && !murdererContext.IsFact(Facts.IsWaiting))
			{
				if (murdererContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null || time - murdererContext.Memory.PrimaryKnownEnemyPlayer.Time > murdererContext.Body.AiDefinition.Memory.NoSeeReturnToSpawnTime)
				{
					murdererContext.SetFact(Facts.IsReturningHome, true);
					NavMeshHit hit;
					if (NavMesh.SamplePosition(murdererContext.Domain.SpawnPosition, out hit, 1f, murdererContext.Domain.NavAgent.areaMask))
					{
						murdererContext.Domain.SetDestination(hit.position);
					}
				}
			}
			else if (murdererContext.IsFact(Facts.CanSeeEnemy) || murdererContext.IsFact(Facts.CanHearEnemy) || time - murdererContext.Body.lastAttackedTime < 2f || murdererContext.IsFact(Facts.AtLocationHome))
			{
				murdererContext.SetFact(Facts.IsReturningHome, false);
			}
		}
	}
}
