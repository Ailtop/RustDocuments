using Characters.AI;
using UnityEngine;

namespace Characters.Operations
{
	public class LookTargetOpposition : CharacterOperation
	{
		[SerializeField]
		private AIController _controller;

		public override void Run(Character owner)
		{
			if (!(_controller.target == null))
			{
				Character.LookingDirection lookingDirection = ((owner.transform.position.x < _controller.target.transform.position.x) ? Character.LookingDirection.Left : Character.LookingDirection.Right);
				owner.ForceToLookAt(lookingDirection);
			}
		}
	}
}
