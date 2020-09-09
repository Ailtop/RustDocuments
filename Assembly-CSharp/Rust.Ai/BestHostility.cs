using Apex.AI;
using Apex.Serialization;

namespace Rust.Ai
{
	public class BestHostility : OptionScorerBase<BasePlayer>
	{
		[ApexSerialization]
		private float score = 10f;

		public override float Score(IAIContext context, BasePlayer option)
		{
			PlayerTargetContext playerTargetContext = context as PlayerTargetContext;
			if (playerTargetContext != null)
			{
				NPCPlayerApex nPCPlayerApex = playerTargetContext.Self as NPCPlayerApex;
				if ((bool)nPCPlayerApex)
				{
					if (!nPCPlayerApex.HostilityConsideration(option))
					{
						return 0f;
					}
					return score;
				}
			}
			return 0f;
		}
	}
}
