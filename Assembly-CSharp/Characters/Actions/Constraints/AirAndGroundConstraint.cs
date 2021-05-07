using UnityEngine;

namespace Characters.Actions.Constraints
{
	public class AirAndGroundConstraint : Constraint
	{
		public enum State
		{
			Ground,
			Terrain,
			Platform,
			Air,
			JumpUp,
			Fall
		}

		[SerializeField]
		protected State _state;

		public override bool Pass()
		{
			return Pass(_action.owner, _state);
		}

		public static bool Pass(Character character, State state)
		{
			if (!character.movement.controller.isGrounded || (state != 0 && (state != State.Terrain || !character.movement.controller.onTerrain) && (state != State.Platform || !character.movement.controller.onPlatform)))
			{
				if (!character.movement.controller.isGrounded)
				{
					if (state != State.Air && (state != State.JumpUp || !(character.movement.controller.velocity.y > 0f)))
					{
						if (state == State.Fall)
						{
							return character.movement.controller.velocity.y < 0f;
						}
						return false;
					}
					return true;
				}
				return false;
			}
			return true;
		}
	}
}
