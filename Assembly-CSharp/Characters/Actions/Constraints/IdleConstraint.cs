namespace Characters.Actions.Constraints
{
	public class IdleConstraint : Constraint
	{
		public override bool Pass()
		{
			return _action.owner.runningMotion == null;
		}
	}
}
