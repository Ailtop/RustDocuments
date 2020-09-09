using Apex.AI;
using Apex.Serialization;

namespace Rust.Ai
{
	public class BestPlayerFamily : OptionScorerBase<BasePlayer>
	{
		[ApexSerialization]
		private float score = 10f;

		public override float Score(IAIContext context, BasePlayer option)
		{
			PlayerTargetContext playerTargetContext = context as PlayerTargetContext;
			if (playerTargetContext != null)
			{
				if (option.Family == playerTargetContext.Self.GetStats.Family)
				{
					return 0f;
				}
				return score;
			}
			return 0f;
		}
	}
}
