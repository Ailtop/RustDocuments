using UnityEngine;

namespace Characters.Operations.LookAtTargets
{
	public sealed class TargetObject : Target
	{
		[SerializeField]
		private Transform _target;

		public override Character.LookingDirection GetDirectionFrom(Character character)
		{
			if (!(_target.position.x < character.transform.position.x))
			{
				return Character.LookingDirection.Right;
			}
			return Character.LookingDirection.Left;
		}
	}
}
