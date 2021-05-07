using Characters;
using UnityEngine;

namespace BT.Conditions
{
	public class TargetOnOwnerPlatform : Condition
	{
		protected override bool Check(Context context)
		{
			Character character = context.Get<Character>(Key.Target);
			Character character2 = context.Get<Character>(Key.OwnerCharacter);
			if (character == null || character2 == null)
			{
				return false;
			}
			Collider2D lastStandingCollider = character.movement.controller.collisionState.lastStandingCollider;
			Collider2D lastStandingCollider2 = character2.movement.controller.collisionState.lastStandingCollider;
			if (lastStandingCollider != lastStandingCollider2)
			{
				return false;
			}
			return true;
		}
	}
}
