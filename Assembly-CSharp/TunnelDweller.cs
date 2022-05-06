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
		if (Rust.GameInfo.HasAchievements && p.GetParentEntity() != null && p.GetParentEntity() is TrainEngine trainEngine && trainEngine.CurThrottleSetting != TrainEngine.EngineSpeeds.Zero && trainEngine.IsMovingOrOn)
		{
			p.stats.Add("dweller_kills_while_moving", 1, Stats.All);
			p.stats.Save(forceSteamSave: true);
		}
	}
}
