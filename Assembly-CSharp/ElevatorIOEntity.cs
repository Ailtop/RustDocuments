public class ElevatorIOEntity : IOEntity
{
	public int NonBusyConsumption = 1;

	public int BusyConsumption = 1;

	public override int ConsumptionAmount()
	{
		if (!HasFlag(Flags.Busy))
		{
			return NonBusyConsumption;
		}
		return BusyConsumption;
	}
}
