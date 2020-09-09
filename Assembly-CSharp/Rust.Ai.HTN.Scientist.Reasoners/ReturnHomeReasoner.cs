using Rust.Ai.HTN.Reasoning;
using UnityEngine.AI;

namespace Rust.Ai.HTN.Scientist.Reasoners
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
			ScientistContext scientistContext = npc.AiDomain.NpcContext as ScientistContext;
			if (scientistContext == null)
			{
				return;
			}
			if (!scientistContext.IsFact(Facts.IsReturningHome))
			{
				if (scientistContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null || time - scientistContext.Memory.PrimaryKnownEnemyPlayer.Time > scientistContext.Body.AiDefinition.Memory.NoSeeReturnToSpawnTime)
				{
					scientistContext.SetFact(Facts.IsReturningHome, true);
					NavMeshHit hit;
					if (NavMesh.SamplePosition(scientistContext.Domain.SpawnPosition, out hit, 1f, scientistContext.Domain.NavAgent.areaMask))
					{
						scientistContext.Domain.SetDestination(hit.position);
						scientistContext.Body.modelState.ducked = false;
						scientistContext.SetFact(Facts.IsDucking, 0, false);
					}
				}
			}
			else if (scientistContext.IsFact(Facts.CanSeeEnemy) || time - scientistContext.Body.lastAttackedTime < 2f || scientistContext.IsFact(Facts.AtLocationHome))
			{
				scientistContext.SetFact(Facts.IsReturningHome, false);
			}
		}
	}
}
