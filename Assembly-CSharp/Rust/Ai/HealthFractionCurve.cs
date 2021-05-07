using Apex.Serialization;
using UnityEngine;

namespace Rust.Ai
{
	public sealed class HealthFractionCurve : BaseScorer
	{
		[ApexSerialization]
		private AnimationCurve ResponseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		public override float GetScore(BaseContext c)
		{
			return ResponseCurve.Evaluate(c.Entity.healthFraction);
		}
	}
}
