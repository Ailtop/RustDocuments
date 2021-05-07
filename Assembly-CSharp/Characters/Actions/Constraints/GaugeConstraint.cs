using Characters.Gear.Weapons.Gauges;
using UnityEngine;

namespace Characters.Actions.Constraints
{
	public class GaugeConstraint : Constraint
	{
		public enum Compare
		{
			GreaterThanOrEqual,
			LessThanOrEqual
		}

		[SerializeField]
		private ValueGauge _gauge;

		[SerializeField]
		private Compare _compare;

		[SerializeField]
		private int _amount;

		public override bool Pass()
		{
			return Pass(_gauge, _compare, _amount);
		}

		public static bool Pass(ValueGauge gauge, Compare compare, int amount)
		{
			if (compare == Compare.GreaterThanOrEqual && gauge.currentValue >= (float)amount)
			{
				return true;
			}
			if (compare == Compare.LessThanOrEqual && gauge.currentValue <= (float)amount)
			{
				return true;
			}
			return false;
		}
	}
}
