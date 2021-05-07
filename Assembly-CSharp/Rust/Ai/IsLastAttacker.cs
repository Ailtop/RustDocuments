using Apex.Serialization;

namespace Rust.Ai
{
	public class IsLastAttacker : WeightedScorerBase<BaseEntity>
	{
		[ApexSerialization]
		public float MinScore = 0.1f;

		public override float GetScore(BaseContext context, BaseEntity option)
		{
			NPCHumanContext nPCHumanContext = context as NPCHumanContext;
			if (nPCHumanContext != null)
			{
				if (!(nPCHumanContext.LastAttacker == option))
				{
					return MinScore;
				}
				return 1f;
			}
			return 0f;
		}
	}
}
