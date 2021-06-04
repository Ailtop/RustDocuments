using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistAStar.Reasoners
{
	public class FirearmPoseReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

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
				if (scientistAStarContext.GetFact(Facts.FirearmOrder) == 0)
				{
					hTNPlayer.SetPlayerFlag(BasePlayer.PlayerFlags.Relaxed, true);
				}
				else
				{
					hTNPlayer.SetPlayerFlag(BasePlayer.PlayerFlags.Relaxed, false);
				}
			}
		}
	}
}
