using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistAStar.Reasoners
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
			ScientistAStarContext scientistAStarContext = npc.AiDomain.NpcContext as ScientistAStarContext;
			if (scientistAStarContext == null)
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
				else if (scientistAStarContext.Memory.PrimaryKnownAnimal.Animal != null)
				{
					orientationType = ((!(scientistAStarContext.PrimaryEnemyPlayerInLineOfSight.Player != null)) ? NpcOrientation.LookAtAnimal : ((!(scientistAStarContext.Memory.PrimaryKnownAnimal.SqrDistance < scientistAStarContext.PrimaryEnemyPlayerInLineOfSight.SqrDistance)) ? (scientistAStarContext.PrimaryEnemyPlayerInLineOfSight.BodyVisible ? NpcOrientation.PrimaryTargetBody : (scientistAStarContext.PrimaryEnemyPlayerInLineOfSight.HeadVisible ? NpcOrientation.PrimaryTargetHead : NpcOrientation.LastKnownPrimaryTargetLocation)) : NpcOrientation.LookAtAnimal));
				}
				else if (scientistAStarContext.PrimaryEnemyPlayerInLineOfSight.Player != null)
				{
					orientationType = (scientistAStarContext.PrimaryEnemyPlayerInLineOfSight.BodyVisible ? NpcOrientation.PrimaryTargetBody : (scientistAStarContext.PrimaryEnemyPlayerInLineOfSight.HeadVisible ? NpcOrientation.PrimaryTargetHead : NpcOrientation.LastKnownPrimaryTargetLocation));
				}
				else if (hTNPlayer.lastAttacker != null && hTNPlayer.lastAttackedTime > 0f && time - hTNPlayer.lastAttackedTime < 2f)
				{
					orientationType = NpcOrientation.LastAttackedDirection;
				}
				else if (scientistAStarContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null)
				{
					orientationType = ((scientistAStarContext.GetFact(Facts.IsSearching) > 0 && scientistAStarContext.GetFact(Facts.IsNavigating) == 0) ? NpcOrientation.LookAround : ((scientistAStarContext.GetFact(Facts.IsIdle) <= 0) ? NpcOrientation.LastKnownPrimaryTargetLocation : ((!scientistAStarContext.IsFact(Facts.CanHearEnemy)) ? NpcOrientation.LastKnownPrimaryTargetLocation : NpcOrientation.AudibleTargetDirection)));
				}
				else if (scientistAStarContext.IsFact(Facts.CanHearEnemy))
				{
					orientationType = NpcOrientation.AudibleTargetDirection;
				}
				scientistAStarContext.OrientationType = orientationType;
			}
		}
	}
}
