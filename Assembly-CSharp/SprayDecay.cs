using ConVar;

public class SprayDecay : Decay
{
	public override bool ShouldDecay(BaseEntity entity)
	{
		return true;
	}

	public override float GetDecayDelay(BaseEntity entity)
	{
		return 0f;
	}

	public override float GetDecayDuration(BaseEntity entity)
	{
		return Global.SprayDuration;
	}
}
