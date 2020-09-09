using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Bear.Reasoners
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
			BearContext bearContext = npc.AiDomain.NpcContext as BearContext;
			bearContext?.SetFact(Facts.HasEnemyTarget, bearContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null);
		}
	}
}
