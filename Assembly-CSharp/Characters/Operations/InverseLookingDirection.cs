namespace Characters.Operations
{
	public class InverseLookingDirection : CharacterOperation
	{
		public override void Run(Character owner)
		{
			owner.ForceToLookAt((owner.lookingDirection == Character.LookingDirection.Right) ? Character.LookingDirection.Left : Character.LookingDirection.Right);
		}
	}
}
