public class SimpleLight : IOEntity
{
	public override void ResetIOState()
	{
		base.ResetIOState();
		SetFlag(Flags.On, false);
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		base.IOStateChanged(inputAmount, inputSlot);
		SetFlag(Flags.On, IsPowered());
	}
}
