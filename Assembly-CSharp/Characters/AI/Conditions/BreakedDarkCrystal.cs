using UnityEngine;

namespace Characters.AI.Conditions
{
	public sealed class BreakedDarkCrystal : Condition
	{
		[SerializeField]
		private Character _left;

		[SerializeField]
		private Character _right;

		protected override bool Check(AIController controller)
		{
			if (!_left.health.dead)
			{
				return _right.health.dead;
			}
			return true;
		}
	}
}
