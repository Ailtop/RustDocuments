using Characters;
using Characters.AI;
using UnityEngine;

namespace BT
{
	public sealed class MoveToLookingDirection : Node
	{
		[SerializeField]
		private bool _turnOnEdge = true;

		protected override NodeState UpdateDeltatime(Context context)
		{
			Character character = context.Get<Character>(Key.OwnerCharacter);
			if (character == null)
			{
				return NodeState.Fail;
			}
			Vector2 direction = ((character.lookingDirection == Character.LookingDirection.Right) ? Vector2.right : Vector2.left);
			if (_turnOnEdge)
			{
				character.movement.TurnOnEdge(ref direction);
			}
			if (Precondition.CanMove(character))
			{
				character.movement.Move(direction);
			}
			return NodeState.Success;
		}
	}
}
