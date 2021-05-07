namespace Characters.Operations.LookAtTargets
{
	public sealed class TurnAround : Target
	{
		public override Character.LookingDirection GetDirectionFrom(Character character)
		{
			if (character.lookingDirection != 0)
			{
				return Character.LookingDirection.Right;
			}
			return Character.LookingDirection.Left;
		}
	}
}
