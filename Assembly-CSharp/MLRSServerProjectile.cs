public class MLRSServerProjectile : ServerProjectile
{
	public override bool HasRangeLimit => false;

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
