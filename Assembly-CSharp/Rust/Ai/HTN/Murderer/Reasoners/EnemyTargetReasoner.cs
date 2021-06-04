using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Murderer.Reasoners
{
	public class EnemyTargetReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			MurdererContext murdererContext = npc.AiDomain.NpcContext as MurdererContext;
			murdererContext?.SetFact(Facts.HasEnemyTarget, murdererContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null);
		}
	}
}
