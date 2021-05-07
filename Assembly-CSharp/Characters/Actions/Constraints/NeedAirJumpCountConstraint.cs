namespace Characters.Actions.Constraints
{
	public class NeedAirJumpCountConstraint : Constraint
	{
		public override bool Pass()
		{
			if (!_action.owner.movement.controller.isGrounded)
			{
				return _action.owner.movement.currentAirJumpCount < _action.owner.movement.airJumpCount.total;
			}
			return false;
		}

		public override void Consume()
		{
			_action.owner.movement.currentAirJumpCount++;
		}
	}
}
