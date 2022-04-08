using UnityEngine;

public class NPCAutoTurret : AutoTurret
{
	public Transform centerMuzzle;

	public Transform muzzleLeft;

	public Transform muzzleRight;

	private bool useLeftMuzzle;

	[ServerVar(Help = "How many seconds until a sleeping player is considered hostile")]
	public static float sleeperhostiledelay = 1200f;

	public override void ServerInit()
	{
		base.ServerInit();
		SetOnline();
		SetPeacekeepermode(isOn: true);
	}

	public virtual bool HasAmmo()
	{
		return true;
	}

	public override bool CheckPeekers()
	{
		return false;
	}

	public override float TargetScanRate()
	{
		return 1.25f;
	}

	public override bool InFiringArc(BaseCombatEntity potentialtarget)
	{
		return true;
	}

	public override float GetMaxAngleForEngagement()
	{
		return 15f;
	}

	public override bool HasFallbackWeapon()
	{
		return true;
	}

	public override Transform GetCenterMuzzle()
	{
		return centerMuzzle;
	}

	public override void FireGun(Vector3 targetPos, float aimCone, Transform muzzleToUse = null, BaseCombatEntity target = null)
	{
		muzzleToUse = muzzleRight;
		base.FireGun(targetPos, aimCone, muzzleToUse, target);
	}

	protected override bool Ignore(BasePlayer player)
	{
		if (!(player is ScientistNPC))
		{
			return player is BanditGuard;
		}
		return true;
	}

	public override bool IsEntityHostile(BaseCombatEntity ent)
	{
		BasePlayer basePlayer = ent as BasePlayer;
		if (basePlayer != null)
		{
			if (basePlayer.IsNpc)
			{
				if (basePlayer is ScientistNPC || basePlayer is BanditGuard)
				{
					return false;
				}
				if (basePlayer is NPCShopKeeper)
				{
					return false;
				}
				if (basePlayer is BasePet)
				{
					return base.IsEntityHostile(basePlayer);
				}
				return true;
			}
			if (basePlayer.IsSleeping() && basePlayer.secondsSleeping >= sleeperhostiledelay)
			{
				return true;
			}
		}
		return base.IsEntityHostile(ent);
	}
}
