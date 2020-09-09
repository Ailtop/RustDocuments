using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Murderer.Reasoners
{
	public class FrustrationReasoner : INpcReasoner
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
			if (murdererContext != null && murdererContext.IsFact(Facts.Frustration) && time - _lastFrustrationDecrementTime > 5f)
			{
				_lastFrustrationDecrementTime = time;
				murdererContext.IncrementFact(Facts.Frustration, -1);
			}
		}
	}
}
