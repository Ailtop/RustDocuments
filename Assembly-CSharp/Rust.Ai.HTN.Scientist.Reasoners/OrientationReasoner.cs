using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Scientist.Reasoners
{
	public class OrientationReasoner : INpcReasoner
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
			HTNPlayer hTNPlayer = npc as HTNPlayer;
			if (!(hTNPlayer == null))
			{
				NpcOrientation orientationType = NpcOrientation.Heading;
				if (npc.IsDestroyed || hTNPlayer.IsDead() || hTNPlayer.IsWounded())
				{
					orientationType = NpcOrientation.None;
				}
				else if (scientistContext.Memory.PrimaryKnownAnimal.Animal != null)
				{
					orientationType = ((!(scientistContext.PrimaryEnemyPlayerInLineOfSight.Player != null)) ? NpcOrientation.LookAtAnimal : ((!(scientistContext.Memory.PrimaryKnownAnimal.SqrDistance < scientistContext.PrimaryEnemyPlayerInLineOfSight.SqrDistance)) ? (scientistContext.PrimaryEnemyPlayerInLineOfSight.BodyVisible ? NpcOrientation.PrimaryTargetBody : (scientistContext.PrimaryEnemyPlayerInLineOfSight.HeadVisible ? NpcOrientation.PrimaryTargetHead : NpcOrientation.LastKnownPrimaryTargetLocation)) : NpcOrientation.LookAtAnimal));
				}
				else if (scientistContext.PrimaryEnemyPlayerInLineOfSight.Player != null)
				{
					orientationType = (scientistContext.PrimaryEnemyPlayerInLineOfSight.BodyVisible ? NpcOrientation.PrimaryTargetBody : (scientistContext.PrimaryEnemyPlayerInLineOfSight.HeadVisible ? NpcOrientation.PrimaryTargetHead : NpcOrientation.LastKnownPrimaryTargetLocation));
				}
				else if (hTNPlayer.lastAttacker != null && hTNPlayer.lastAttackedTime > 0f && time - hTNPlayer.lastAttackedTime < 2f)
				{
					orientationType = NpcOrientation.LastAttackedDirection;
				}
				else if (scientistContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null)
				{
					orientationType = ((scientistContext.GetFact(Facts.IsSearching) > 0 && scientistContext.GetFact(Facts.IsNavigating) == 0) ? NpcOrientation.LookAround : ((scientistContext.GetFact(Facts.IsIdle) <= 0) ? NpcOrientation.LastKnownPrimaryTargetLocation : ((!scientistContext.IsFact(Facts.CanHearEnemy)) ? NpcOrientation.Heading : NpcOrientation.AudibleTargetDirection)));
				}
				else if (scientistContext.IsFact(Facts.CanHearEnemy))
				{
					orientationType = NpcOrientation.AudibleTargetDirection;
				}
				scientistContext.OrientationType = orientationType;
			}
		}
	}
}
