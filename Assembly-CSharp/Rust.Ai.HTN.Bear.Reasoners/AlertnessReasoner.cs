using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Bear.Reasoners
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
			BearContext bearContext = npc.AiDomain.NpcContext as BearContext;
			if (bearContext != null && bearContext.IsFact(Facts.Alertness))
			{
				if (bearContext.GetFact(Facts.Alertness) > 10)
				{
					bearContext.SetFact(Facts.Alertness, 10, true, false);
				}
				if (time - _lastFrustrationDecrementTime > 1f)
				{
					_lastFrustrationDecrementTime = time;
					bearContext.IncrementFact(Facts.Alertness, -1);
				}
			}
		}
	}
}
