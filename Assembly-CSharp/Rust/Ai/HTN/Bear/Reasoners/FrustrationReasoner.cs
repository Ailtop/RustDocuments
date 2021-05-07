using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Bear.Reasoners
{
	public class FrustrationReasoner : INpcReasoner
	{
		private float _lastFrustrationDecrementTime;

		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			BearContext bearContext = npc.AiDomain.NpcContext as BearContext;
			if (bearContext != null && bearContext.IsFact(Facts.Frustration) && time - _lastFrustrationDecrementTime > 5f)
			{
				_lastFrustrationDecrementTime = time;
				bearContext.IncrementFact(Facts.Frustration, -1);
			}
		}
	}
}
