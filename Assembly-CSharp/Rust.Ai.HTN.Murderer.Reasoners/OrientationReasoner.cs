using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Murderer.Reasoners
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
			MurdererContext murdererContext = npc.AiDomain.NpcContext as MurdererContext;
			if (murdererContext == null)
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
				else if (murdererContext.Memory.PrimaryKnownAnimal.Animal != null)
				{
					orientationType = ((!(murdererContext.PrimaryEnemyPlayerInLineOfSight.Player != null)) ? NpcOrientation.LookAtAnimal : ((!(murdererContext.Memory.PrimaryKnownAnimal.SqrDistance < murdererContext.PrimaryEnemyPlayerInLineOfSight.SqrDistance)) ? (murdererContext.PrimaryEnemyPlayerInLineOfSight.BodyVisible ? NpcOrientation.PrimaryTargetBody : (murdererContext.PrimaryEnemyPlayerInLineOfSight.HeadVisible ? NpcOrientation.PrimaryTargetHead : NpcOrientation.LastKnownPrimaryTargetLocation)) : NpcOrientation.LookAtAnimal));
				}
				else if (murdererContext.PrimaryEnemyPlayerInLineOfSight.Player != null)
				{
					orientationType = (murdererContext.PrimaryEnemyPlayerInLineOfSight.BodyVisible ? NpcOrientation.PrimaryTargetBody : (murdererContext.PrimaryEnemyPlayerInLineOfSight.HeadVisible ? NpcOrientation.PrimaryTargetHead : NpcOrientation.LastKnownPrimaryTargetLocation));
				}
				else if (hTNPlayer.lastAttacker != null && hTNPlayer.lastAttackedTime > 0f && time - hTNPlayer.lastAttackedTime < 2f)
				{
					orientationType = NpcOrientation.LastAttackedDirection;
				}
				else if (murdererContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null)
				{
					orientationType = ((murdererContext.GetFact(Facts.IsSearching) > 0 && murdererContext.GetFact(Facts.IsNavigating) == 0) ? NpcOrientation.LookAround : ((murdererContext.GetFact(Facts.IsIdle) <= 0) ? NpcOrientation.LastKnownPrimaryTargetLocation : ((!murdererContext.IsFact(Facts.CanHearEnemy)) ? NpcOrientation.Heading : NpcOrientation.AudibleTargetDirection)));
				}
				else if (murdererContext.IsFact(Facts.CanHearEnemy))
				{
					orientationType = NpcOrientation.AudibleTargetDirection;
				}
				if (murdererContext.IsFact(Facts.IsRoaming) && !murdererContext.IsFact(Facts.HasEnemyTarget))
				{
					orientationType = NpcOrientation.Heading;
				}
				else if (murdererContext.IsFact(Facts.IsReturningHome) && !murdererContext.IsFact(Facts.HasEnemyTarget))
				{
					orientationType = NpcOrientation.Home;
				}
				murdererContext.OrientationType = orientationType;
			}
		}
	}
}
