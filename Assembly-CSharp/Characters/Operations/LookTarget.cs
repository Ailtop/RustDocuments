using Characters.AI;
using UnityEngine;

namespace Characters.Operations
{
	public class LookTarget : CharacterOperation
	{
		[SerializeField]
		private AIController _controller;

		public override void Run(Character owner)
		{
			if (!(_controller.target == null))
			{
				owner.ForceToLookAt(_controller.target.transform.position.x);
			}
		}
	}
}
