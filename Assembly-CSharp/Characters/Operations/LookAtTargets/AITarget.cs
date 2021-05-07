using Characters.AI;
using UnityEngine;

namespace Characters.Operations.LookAtTargets
{
	public sealed class AITarget : Target
	{
		[SerializeField]
		private AIController _aIController;

		public override Character.LookingDirection GetDirectionFrom(Character character)
		{
			Character target = _aIController.target;
			if (target == null)
			{
				return character.lookingDirection;
			}
			if (target.transform.position.x > character.transform.position.x)
			{
				return Character.LookingDirection.Right;
			}
			return Character.LookingDirection.Left;
		}
	}
}
