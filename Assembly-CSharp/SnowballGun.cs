public class SnowballGun : BaseProjectile
{
	public ItemDefinition OverrideProjectile;

	protected override ItemDefinition PrimaryMagazineAmmo
	{
		get
		{
			if (!(OverrideProjectile != null))
			{
				return base.PrimaryMagazineAmmo;
			}
			return OverrideProjectile;
		}
	}

	protected override bool CanRefundAmmo => false;

	public override bool TryReloadMagazine(IAmmoContainer ammoSource, int desiredAmount = -1)
	{
		desiredAmount = 1;
		TryReload(ammoSource, desiredAmount, CanRefundAmmo);
		SetAmmoCount(primaryMagazine.capacity);
		primaryMagazine.ammoType = OverrideProjectile;
		SendNetworkUpdateImmediate();
		ItemManager.DoRemoves();
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (ownerPlayer != null)
		{
			ownerPlayer.inventory.ServerUpdate(0f);
		}
		return true;
	}
}
