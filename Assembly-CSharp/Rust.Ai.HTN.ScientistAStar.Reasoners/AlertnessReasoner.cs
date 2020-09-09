using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistAStar.Reasoners
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
			ScientistAStarContext scientistAStarContext = npc.AiDomain.NpcContext as ScientistAStarContext;
			if (scientistAStarContext != null && scientistAStarContext.IsFact(Facts.Alertness))
			{
				if (scientistAStarContext.GetFact(Facts.Alertness) > 10)
				{
					scientistAStarContext.SetFact(Facts.Alertness, 10, true, false);
				}
				if (time - _lastFrustrationDecrementTime > 1f)
				{
					_lastFrustrationDecrementTime = time;
					scientistAStarContext.IncrementFact(Facts.Alertness, -1);
				}
			}
		}
	}
}
