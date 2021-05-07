using UnityEngine;

namespace Characters.AI.Conditions
{
	public sealed class TargetIsGrounded : Condition
	{
		[SerializeField]
		private AIController _controller;

		protected override bool Check(AIController controller)
		{
			if (_controller.target == null)
			{
				return true;
			}
			return _controller.target.movement.isGrounded;
		}
	}
}
