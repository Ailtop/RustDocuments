using UnityEngine;

namespace Runnables.Chances
{
	public sealed class Constant : Chance
	{
		[SerializeField]
		[Range(0f, 1f)]
		private float _truePercent;

		public override bool IsTrue()
		{
			return MMMaths.Chance(_truePercent);
		}
	}
}
