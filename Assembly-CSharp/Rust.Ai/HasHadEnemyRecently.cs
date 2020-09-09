using UnityEngine;

namespace Rust.Ai
{
	public class HasHadEnemyRecently : BaseScorer
	{
		public override float GetScore(BaseContext ctx)
		{
			NPCHumanContext nPCHumanContext = ctx as NPCHumanContext;
			if (nPCHumanContext != null && Time.time - nPCHumanContext.Human.LastHasEnemyTime < nPCHumanContext.Human.Stats.AttackedMemoryTime)
			{
				return 1f;
			}
			return 0f;
		}
	}
}
