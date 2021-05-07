using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistJunkpile.Reasoners
{
	public class EnemyTargetReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistJunkpileContext scientistJunkpileContext = npc.AiDomain.NpcContext as ScientistJunkpileContext;
			scientistJunkpileContext?.SetFact(Facts.HasEnemyTarget, scientistJunkpileContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null);
		}
	}
}
