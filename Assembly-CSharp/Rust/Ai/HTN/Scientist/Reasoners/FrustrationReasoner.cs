using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Scientist.Reasoners
{
	public class FrustrationReasoner : INpcReasoner
	{
		private float _lastFrustrationDecrementTime;

		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistContext scientistContext = npc.AiDomain.NpcContext as ScientistContext;
			if (scientistContext != null && scientistContext.IsFact(Facts.Frustration) && time - _lastFrustrationDecrementTime > 5f)
			{
				_lastFrustrationDecrementTime = time;
				scientistContext.IncrementFact(Facts.Frustration, -1);
			}
		}
	}
}
