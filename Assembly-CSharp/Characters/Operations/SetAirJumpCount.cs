using UnityEngine;

namespace Characters.Operations
{
	public class SetAirJumpCount : CharacterOperation
	{
		[SerializeField]
		private int _currentAirJumpCount;

		public override void Run(Character target)
		{
			target.movement.currentAirJumpCount = _currentAirJumpCount;
		}
	}
}
