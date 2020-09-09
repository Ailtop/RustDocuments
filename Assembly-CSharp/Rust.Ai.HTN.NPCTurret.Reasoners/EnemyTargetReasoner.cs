using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.NPCTurret.Reasoners
{
	public class EnemyTargetReasoner : INpcReasoner
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
			NPCTurretContext nPCTurretContext = npc.AiDomain.NpcContext as NPCTurretContext;
			nPCTurretContext?.SetFact(Facts.HasEnemyTarget, nPCTurretContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null);
		}
	}
}
