using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistAStar.Reasoners
{
	public class FrustrationReasoner : INpcReasoner
	{
		private float _lastFrustrationDecrementTime;

		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistAStarContext scientistAStarContext = npc.AiDomain.NpcContext as ScientistAStarContext;
			if (scientistAStarContext != null && scientistAStarContext.IsFact(Facts.Frustration) && time - _lastFrustrationDecrementTime > 5f)
			{
				_lastFrustrationDecrementTime = time;
				scientistAStarContext.IncrementFact(Facts.Frustration, -1);
			}
		}
	}
}
