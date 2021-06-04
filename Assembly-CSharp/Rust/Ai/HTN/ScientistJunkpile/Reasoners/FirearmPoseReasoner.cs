using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistJunkpile.Reasoners
{
	public class FirearmPoseReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistJunkpileContext scientistJunkpileContext = npc.AiDomain.NpcContext as ScientistJunkpileContext;
			if (scientistJunkpileContext == null)
			{
				return;
			}
			HTNPlayer hTNPlayer = npc as HTNPlayer;
			if (!(hTNPlayer == null))
			{
				if (scientistJunkpileContext.GetFact(Facts.FirearmOrder) == 0)
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
