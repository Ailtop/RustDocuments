using Characters;
using Characters.AI;
using UnityEngine;

namespace BT
{
	public class MoveToTarget : Node
	{
		[SerializeField]
		private bool _turnOnEdge;

		protected override NodeState UpdateDeltatime(Context context)
		{
			Character character = context.Get<Character>(Key.Target);
			Character character2 = context.Get<Character>(Key.OwnerCharacter);
			if (character == null)
			{
				return NodeState.Fail;
			}
			Vector2 direction = ((!(character.transform.position.x > character2.transform.position.x)) ? Vector2.left : Vector2.right);
			if (_turnOnEdge)
			{
				character2.movement.TurnOnEdge(ref direction);
			}
			if (Precondition.CanMove(character2))
			{
				character2.movement.Move(direction);
			}
			return NodeState.Success;
		}
	}
}
