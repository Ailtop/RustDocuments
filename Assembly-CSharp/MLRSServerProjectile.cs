public class MLRSServerProjectile : ServerProjectile
{
	protected override int mask => 1235430161;

	protected override bool IsAValidHit(BaseEntity hitEnt)
	{
		if (!base.IsAValidHit(hitEnt))
		{
			return false;
		}
		if (BaseEntityEx.IsValid(hitEnt))
		{
			return !(hitEnt is MLRS);
		}
		return true;
	}
}
