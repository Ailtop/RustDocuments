public class TunnelDweller : HumanNPC
{
	private float maxRoamDist = 8f;

	public override float GetMaxRoamDistFromSpawn()
	{
		return maxRoamDist;
	}

	protected override string OverrideCorpseName()
	{
		return "Tunnel Dweller";
	}
}
