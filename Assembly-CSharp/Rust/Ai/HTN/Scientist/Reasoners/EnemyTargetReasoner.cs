using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Scientist.Reasoners
{
	public class EnemyTargetReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistContext scientistContext = npc.AiDomain.NpcContext as ScientistContext;
			scientistContext?.SetFact(Facts.HasEnemyTarget, scientistContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null);
		}
	}
}
