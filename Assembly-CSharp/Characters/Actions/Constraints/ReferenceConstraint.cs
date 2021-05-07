using UnityEngine;

namespace Characters.Actions.Constraints
{
	public class ReferenceConstraint : Constraint
	{
		[SerializeField]
		private Constraint _constraint;

		public override bool Pass()
		{
			return _constraint.Pass();
		}

		public override void Consume()
		{
			_constraint.Consume();
		}
	}
}
