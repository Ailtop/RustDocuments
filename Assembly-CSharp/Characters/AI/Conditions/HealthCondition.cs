using UnityEngine;

namespace Characters.AI.Conditions
{
	public class HealthCondition : Condition
	{
		private enum Comparer
		{
			GreaterThan,
			LessThan
		}

		[SerializeField]
		private Comparer _compare;

		[SerializeField]
		[Range(0f, 1f)]
		private float _percent;

		protected override bool Check(AIController controller)
		{
			switch (_compare)
			{
			case Comparer.GreaterThan:
				return controller.character.health.percent >= (double)_percent;
			case Comparer.LessThan:
				return controller.character.health.percent <= (double)_percent;
			default:
				return false;
			}
		}
	}
}
