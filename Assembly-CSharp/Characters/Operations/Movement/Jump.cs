using UnityEngine;

namespace Characters.Operations.Movement
{
	public class Jump : CharacterOperation
	{
		[SerializeField]
		private float _jumpHeight = 3f;

		public override void Run(Character owner)
		{
			owner.movement.Jump(_jumpHeight);
		}
	}
}
