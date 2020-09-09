using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.NPCTurret.Reasoners
{
	public class AlertnessReasoner : INpcReasoner
	{
		private float _lastFrustrationDecrementTime;

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
			if (nPCTurretContext != null && nPCTurretContext.IsFact(Facts.Alertness))
			{
				if (nPCTurretContext.GetFact(Facts.Alertness) > 10)
				{
					nPCTurretContext.SetFact(Facts.Alertness, 10, true, false);
				}
				if (time - _lastFrustrationDecrementTime > 1f)
				{
					_lastFrustrationDecrementTime = time;
					nPCTurretContext.IncrementFact(Facts.Alertness, -1);
				}
			}
		}
	}
}
