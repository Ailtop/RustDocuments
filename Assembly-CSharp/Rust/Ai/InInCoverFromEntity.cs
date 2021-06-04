using Apex.Serialization;

namespace Rust.Ai
{
	public class InInCoverFromEntity : WeightedScorerBase<BaseEntity>
	{
		[ApexSerialization]
		public float CoverArcThreshold = -0.75f;

		public override float GetScore(BaseContext ctx, BaseEntity option)
		{
			if (SingletonComponent<AiManager>.Instance == null || !SingletonComponent<AiManager>.Instance.enabled || !SingletonComponent<AiManager>.Instance.UseCover || ctx.AIAgent.AttackTarget == null)
			{
				return 0f;
			}
			bool flag = ctx is NPCHumanContext;
			return 0f;
		}
	}
}
