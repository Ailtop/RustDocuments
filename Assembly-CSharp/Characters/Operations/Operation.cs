namespace Characters.Operations
{
	public abstract class Operation : CharacterOperation
	{
		public abstract void Run();

		public override void Run(Character owner)
		{
			Run();
		}

		public override void Run(Character owner, Character target)
		{
			Run();
		}
	}
}
