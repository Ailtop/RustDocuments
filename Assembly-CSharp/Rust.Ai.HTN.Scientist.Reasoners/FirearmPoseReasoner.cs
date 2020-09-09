using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Scientist.Reasoners
{
	public class FirearmPoseReasoner : INpcReasoner
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
			ScientistContext scientistContext = npc.AiDomain.NpcContext as ScientistContext;
			if (scientistContext == null)
			{
				return;
			}
			HTNPlayer hTNPlayer = npc as HTNPlayer;
			if (!(hTNPlayer == null))
			{
				if (scientistContext.GetFact(Facts.FirearmOrder) == 0)
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
