namespace Characters.Operations
{
	public sealed class SetParentToCharacterAttach : CharacterOperation
	{
		public override void Run(Character owner)
		{
			if (!(owner.attach == null))
			{
				base.transform.SetParent(owner.attach.transform);
			}
		}
	}
}
