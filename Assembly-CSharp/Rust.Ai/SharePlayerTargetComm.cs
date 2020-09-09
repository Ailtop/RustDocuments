using Apex.AI;
using System.Collections.Generic;

namespace Rust.Ai
{
	public class SharePlayerTargetComm : ActionBase<PlayerTargetContext>
	{
		public override void Execute(PlayerTargetContext c)
		{
			NPCPlayerApex nPCPlayerApex = c.Self as NPCPlayerApex;
			List<AiAnswer_ShareEnemyTarget> answers;
			if (nPCPlayerApex != null && nPCPlayerApex.AskQuestion(default(AiQuestion_ShareEnemyTarget), out answers) > 0)
			{
				foreach (AiAnswer_ShareEnemyTarget item in answers)
				{
					if (item.PlayerTarget != null && item.LastKnownPosition.HasValue && nPCPlayerApex.HostilityConsideration(item.PlayerTarget))
					{
						c.Target = item.PlayerTarget;
						c.Score = 1f;
						c.LastKnownPosition = item.LastKnownPosition.Value;
						Memory.ExtendedInfo extendedInfo;
						nPCPlayerApex.UpdateTargetMemory(c.Target, c.Score, c.LastKnownPosition, out extendedInfo);
						break;
					}
				}
			}
		}
	}
}
