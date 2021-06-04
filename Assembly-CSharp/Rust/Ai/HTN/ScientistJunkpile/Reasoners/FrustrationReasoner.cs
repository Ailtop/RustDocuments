using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistJunkpile.Reasoners
{
	public class FrustrationReasoner : INpcReasoner
	{
		private float _lastFrustrationDecrementTime;

		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistJunkpileContext scientistJunkpileContext = npc.AiDomain.NpcContext as ScientistJunkpileContext;
			if (scientistJunkpileContext != null && scientistJunkpileContext.IsFact(Facts.Frustration) && time - _lastFrustrationDecrementTime > 5f)
			{
				_lastFrustrationDecrementTime = time;
				scientistJunkpileContext.IncrementFact(Facts.Frustration, -1);
			}
		}
	}
}
