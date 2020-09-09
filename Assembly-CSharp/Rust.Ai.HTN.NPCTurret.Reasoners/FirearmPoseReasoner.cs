using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.NPCTurret.Reasoners
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
			NPCTurretContext nPCTurretContext = npc.AiDomain.NpcContext as NPCTurretContext;
			if (nPCTurretContext == null)
			{
				return;
			}
			HTNPlayer hTNPlayer = npc as HTNPlayer;
			if (!(hTNPlayer == null))
			{
				if (nPCTurretContext.GetFact(Facts.FirearmOrder) == 0)
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
