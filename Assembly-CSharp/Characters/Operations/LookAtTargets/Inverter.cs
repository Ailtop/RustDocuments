using UnityEngine;

namespace Characters.Operations.LookAtTargets
{
	public class Inverter : Target
	{
		[SerializeField]
		[Subcomponent]
		private Target _target;

		public override Character.LookingDirection GetDirectionFrom(Character character)
		{
			if (_target.GetDirectionFrom(character) != 0)
			{
				return Character.LookingDirection.Right;
			}
			return Character.LookingDirection.Left;
		}
	}
}
