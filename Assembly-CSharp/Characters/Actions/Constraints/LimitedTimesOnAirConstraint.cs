using Characters.Movements;
using UnityEngine;

namespace Characters.Actions.Constraints
{
	public class LimitedTimesOnAirConstraint : Constraint
	{
		[SerializeField]
		private int _maxTimes;

		[SerializeField]
		private bool _resetOnAirJump;

		private int _remain;

		public override bool Pass()
		{
			if (!_action.owner.movement.controller.isGrounded)
			{
				if (!_action.owner.movement.controller.isGrounded)
				{
					return _remain > 0;
				}
				return false;
			}
			return true;
		}

		public override void Consume()
		{
			if (!_action.owner.movement.controller.isGrounded)
			{
				_remain--;
			}
		}

		public override void Initilaize(Action action)
		{
			base.Initilaize(action);
			_remain = _maxTimes;
			_action.owner.movement.onGrounded += OnGrounded;
			if (_resetOnAirJump)
			{
				_action.owner.movement.onJump += OnJump;
			}
		}

		private void OnDestroy()
		{
			_action.owner.movement.onGrounded -= OnGrounded;
			if (_resetOnAirJump)
			{
				_action.owner.movement.onJump -= OnJump;
			}
		}

		private void OnGrounded()
		{
			_remain = _maxTimes;
		}

		private void OnJump(Movement.JumpType jumpType, float jumpHeight)
		{
			_remain = _maxTimes;
		}
	}
}
