namespace Characters.Operations.Health
{
	public class Suicide : CharacterOperation
	{
		public override void Run(Character owner)
		{
			owner.health.Kill();
		}
	}
}
