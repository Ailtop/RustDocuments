using Rust;

public class TunnelDweller : HumanNPC
{
	private const string DWELLER_KILL_STAT = "dweller_kills_while_moving";

	protected override string OverrideCorpseName()
	{
		return "Tunnel Dweller";
	}

	protected override void OnKilledByPlayer(BasePlayer p)
	{
		base.OnKilledByPlayer(p);
		TrainEngine trainEngine;
		if (Rust.GameInfo.HasAchievements && p.GetParentEntity() != null && (object)(trainEngine = p.GetParentEntity() as TrainEngine) != null && trainEngine.CurThrottleSetting != TrainEngine.EngineSpeeds.Zero && trainEngine.IsMovingOrOn)
		{
			p.stats.Add("dweller_kills_while_moving", 1, Stats.All);
		}
	}
}
