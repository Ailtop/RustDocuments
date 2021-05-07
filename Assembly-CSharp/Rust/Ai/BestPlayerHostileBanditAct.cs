using Apex.AI;
using Apex.Serialization;
using UnityEngine;

namespace Rust.Ai
{
	public class BestPlayerHostileBanditAct : OptionScorerBase<BasePlayer>
	{
		[ApexSerialization]
		private float score = 10f;

		[ApexSerialization]
		public float Timeout = 10f;

		public override float Score(IAIContext context, BasePlayer option)
		{
			PlayerTargetContext playerTargetContext = context as PlayerTargetContext;
			if (playerTargetContext != null)
			{
				Scientist scientist = playerTargetContext.Self as Scientist;
				if ((bool)scientist)
				{
					Memory.ExtendedInfo extendedInfo = scientist.AiContext.Memory.GetExtendedInfo(option);
					if (extendedInfo.Entity != null)
					{
						if (Time.time < extendedInfo.LastHurtUsTime + Timeout)
						{
							return score;
						}
						if (!scientist.HostilityConsideration(option))
						{
							return 0f;
						}
						return score;
					}
					if (!scientist.HostilityConsideration(option))
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
