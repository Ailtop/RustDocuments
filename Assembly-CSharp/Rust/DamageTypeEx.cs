namespace Rust;

public static class DamageTypeEx
{
	public static bool IsMeleeType(this DamageType damageType)
	{
		if (damageType != DamageType.Blunt && damageType != DamageType.Slash)
		{
			return damageType == DamageType.Stab;
		}
		return true;
	}

	public static bool IsBleedCausing(this DamageType damageType)
	{
		if (damageType != DamageType.Bite && damageType != DamageType.Slash && damageType != DamageType.Stab && damageType != DamageType.Bullet)
		{
			return damageType == DamageType.Arrow;
		}
		return true;
	}

	public static bool IsConsideredAnAttack(this DamageType damageType)
	{
		if (damageType != DamageType.Decay)
		{
			return damageType != DamageType.Collision;
		}
		return false;
	}
}
