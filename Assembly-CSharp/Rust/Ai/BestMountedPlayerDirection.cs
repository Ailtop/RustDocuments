using Apex.AI;
using Apex.Serialization;
using UnityEngine;

namespace Rust.Ai
{
	public class BestMountedPlayerDirection : OptionScorerBase<BasePlayer>
	{
		[ApexSerialization]
		private float score = 10f;

		public override float Score(IAIContext context, BasePlayer option)
		{
			PlayerTargetContext playerTargetContext = context as PlayerTargetContext;
			if (playerTargetContext != null)
			{
				BasePlayer basePlayer = playerTargetContext.Self as BasePlayer;
				Vector3 dir;
				float dot;
				if ((bool)basePlayer && basePlayer.isMounted && Evaluate(basePlayer, option.ServerPosition, out dir, out dot))
				{
					playerTargetContext.Direction[playerTargetContext.CurrentOptionsIndex] = dir;
					playerTargetContext.Dot[playerTargetContext.CurrentOptionsIndex] = dot;
					return (dot + 1f) * 0.5f * score;
				}
			}
			playerTargetContext.Direction[playerTargetContext.CurrentOptionsIndex] = Vector3.zero;
			playerTargetContext.Dot[playerTargetContext.CurrentOptionsIndex] = 0f;
			return 0f;
		}

		public static bool Evaluate(BasePlayer self, Vector3 optionPosition, out Vector3 dir, out float dot)
		{
			BaseMountable mounted = self.GetMounted();
			dir = (optionPosition - self.ServerPosition).normalized;
			dot = Vector3.Dot(dir, mounted.transform.forward);
			if (dot < -0.1f)
			{
				dot = -1f;
				return false;
			}
			return true;
		}
	}
}
