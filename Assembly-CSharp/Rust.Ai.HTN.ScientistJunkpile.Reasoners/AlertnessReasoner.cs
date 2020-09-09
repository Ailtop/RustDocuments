using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistJunkpile.Reasoners
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
			ScientistJunkpileContext scientistJunkpileContext = npc.AiDomain.NpcContext as ScientistJunkpileContext;
			if (scientistJunkpileContext != null && scientistJunkpileContext.IsFact(Facts.Alertness))
			{
				if (scientistJunkpileContext.GetFact(Facts.Alertness) > 10)
				{
					scientistJunkpileContext.SetFact(Facts.Alertness, 10, true, false);
				}
				if (time - _lastFrustrationDecrementTime > 1f)
				{
					_lastFrustrationDecrementTime = time;
					scientistJunkpileContext.IncrementFact(Facts.Alertness, -1);
				}
			}
		}
	}
}
