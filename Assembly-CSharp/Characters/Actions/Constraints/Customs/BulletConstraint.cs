using Characters.Gear.Weapons.Gauges;
using UnityEngine;

namespace Characters.Actions.Constraints.Customs
{
	public class BulletConstraint : Constraint
	{
		[SerializeField]
		private Magazine _magazine;

		[SerializeField]
		private int _amount;

		public override bool Pass()
		{
			return Pass(_magazine, _amount);
		}

		public static bool Pass(Magazine magazine, int amount)
		{
			if (magazine.Has(amount))
			{
				return true;
			}
			return false;
		}
	}
}
