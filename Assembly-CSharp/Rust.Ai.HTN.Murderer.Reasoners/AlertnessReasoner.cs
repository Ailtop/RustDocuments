using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Murderer.Reasoners
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
			MurdererContext murdererContext = npc.AiDomain.NpcContext as MurdererContext;
			if (murdererContext != null && murdererContext.IsFact(Facts.Alertness))
			{
				if (murdererContext.GetFact(Facts.Alertness) > 10)
				{
					murdererContext.SetFact(Facts.Alertness, 10, true, false);
				}
				if (time - _lastFrustrationDecrementTime > 1f)
				{
					_lastFrustrationDecrementTime = time;
					murdererContext.IncrementFact(Facts.Alertness, -1);
				}
			}
		}
	}
}
