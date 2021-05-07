namespace Characters.Operations.Movement
{
	public class JumpDown : CharacterOperation
	{
		public override void Run(Character owner)
		{
			owner.movement.JumpDown();
		}
	}
}
