using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.NPCTurret.Reasoners
{
	public class OrientationReasoner : INpcReasoner
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
			if (!(hTNPlayer == null))
			{
				NpcOrientation orientationType = NpcOrientation.Heading;
				if (npc.IsDestroyed || hTNPlayer.IsDead() || hTNPlayer.IsWounded())
				{
					orientationType = NpcOrientation.None;
				}
				else if (nPCTurretContext.Memory.PrimaryKnownAnimal.Animal != null)
				{
					orientationType = ((!(nPCTurretContext.PrimaryEnemyPlayerInLineOfSight.Player != null)) ? NpcOrientation.LookAtAnimal : ((!(nPCTurretContext.Memory.PrimaryKnownAnimal.SqrDistance < nPCTurretContext.PrimaryEnemyPlayerInLineOfSight.SqrDistance)) ? (nPCTurretContext.PrimaryEnemyPlayerInLineOfSight.BodyVisible ? NpcOrientation.PrimaryTargetBody : (nPCTurretContext.PrimaryEnemyPlayerInLineOfSight.HeadVisible ? NpcOrientation.PrimaryTargetHead : NpcOrientation.LastKnownPrimaryTargetLocation)) : NpcOrientation.LookAtAnimal));
				}
				else if (nPCTurretContext.PrimaryEnemyPlayerInLineOfSight.Player != null)
				{
					orientationType = (nPCTurretContext.PrimaryEnemyPlayerInLineOfSight.BodyVisible ? NpcOrientation.PrimaryTargetBody : (nPCTurretContext.PrimaryEnemyPlayerInLineOfSight.HeadVisible ? NpcOrientation.PrimaryTargetHead : NpcOrientation.LastKnownPrimaryTargetLocation));
				}
				else if (hTNPlayer.lastAttacker != null && hTNPlayer.lastAttackedTime > 0f && time - hTNPlayer.lastAttackedTime < 2f)
				{
					orientationType = NpcOrientation.LastAttackedDirection;
				}
				else if (nPCTurretContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null)
				{
					orientationType = NpcOrientation.LastKnownPrimaryTargetLocation;
				}
				nPCTurretContext.OrientationType = orientationType;
			}
		}
	}
}
