using BT;
using UnityEngine;

namespace Characters.Operations.LookAtTargets
{
	public sealed class BTTarget : Target
	{
		[SerializeField]
		private BehaviourTreeRunner _bt;

		public override Character.LookingDirection GetDirectionFrom(Character character)
		{
			Character character2 = _bt.context.Get<Character>(BT.Key.Target);
			if (character2 == null)
			{
				return character.lookingDirection;
			}
			if (character2.transform.position.x > character.transform.position.x)
			{
				return Character.LookingDirection.Right;
			}
			return Character.LookingDirection.Left;
		}
	}
}
