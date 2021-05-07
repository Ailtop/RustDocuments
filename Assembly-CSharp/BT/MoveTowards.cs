using Characters;
using Characters.AI;
using UnityEngine;

namespace BT
{
	public sealed class MoveTowards : Node
	{
		[SerializeField]
		[Range(0f, 1f)]
		private float _rightChance;

		[SerializeField]
		private bool _turnOnEdge = true;

		protected override NodeState UpdateDeltatime(Context context)
		{
			Character character = context.Get<Character>(Key.OwnerCharacter);
			Vector2 direction = ((!MMMaths.Chance(_rightChance)) ? Vector2.left : Vector2.right);
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
