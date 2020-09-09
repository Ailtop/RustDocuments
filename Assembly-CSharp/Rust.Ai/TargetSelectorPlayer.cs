using Apex.AI;
using Apex.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rust.Ai
{
	public class TargetSelectorPlayer : ActionWithOptions<BasePlayer>
	{
		[ApexSerialization]
		private bool allScorersMustScoreAboveZero = true;

		public override void Execute(IAIContext context)
		{
			PlayerTargetContext playerTargetContext = context as PlayerTargetContext;
			if (playerTargetContext != null)
			{
				Evaluate(playerTargetContext, base.scorers, playerTargetContext.Players, playerTargetContext.PlayerCount, allScorersMustScoreAboveZero, out playerTargetContext.Target, out playerTargetContext.Score, out playerTargetContext.Index, out playerTargetContext.LastKnownPosition);
			}
		}

		public static bool Evaluate(PlayerTargetContext context, IList<IOptionScorer<BasePlayer>> scorers, BasePlayer[] options, int numOptions, bool allScorersMustScoreAboveZero, out BasePlayer best, out float bestScore, out int bestIndex, out Vector3 bestLastKnownPosition)
		{
			bestScore = float.MinValue;
			best = null;
			bestIndex = -1;
			bestLastKnownPosition = Vector3.zero;
			for (int i = 0; i < numOptions; i++)
			{
				context.CurrentOptionsIndex = i;
				float num = 0f;
				bool flag = true;
				for (int j = 0; j < scorers.Count; j++)
				{
					if (!scorers[j].isDisabled)
					{
						float num2 = scorers[j].Score(context, options[i]);
						if (allScorersMustScoreAboveZero && num2 <= 0f)
						{
							flag = false;
							break;
						}
						num += num2;
					}
				}
				if (flag)
				{
					Vector3 vector = Vector3.zero;
					BaseContext baseContext = context.Self.GetContext(Guid.Empty) as BaseContext;
					if (baseContext != null)
					{
						NPCPlayerApex nPCPlayerApex = context.Self as NPCPlayerApex;
						Memory.ExtendedInfo extendedInfo;
						vector = baseContext.Memory.Update(options[i], num, context.Direction[i], context.Dot[i], context.DistanceSqr[i], context.LineOfSight[i], nPCPlayerApex != null && nPCPlayerApex.lastAttacker == options[i], (nPCPlayerApex != null) ? nPCPlayerApex.lastAttackedTime : 0f, out extendedInfo).Position;
					}
					if (num > bestScore)
					{
						bestScore = num;
						best = options[i];
						bestIndex = i;
						bestLastKnownPosition = vector;
					}
				}
			}
			return best != null;
		}
	}
}
