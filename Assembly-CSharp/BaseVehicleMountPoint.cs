public class BaseVehicleMountPoint : BaseMountable
{
	public override bool DirectlyMountable()
	{
		return false;
	}

	public BaseVehicle GetVehicleParent()
	{
		return GetParentEntity() as BaseVehicle;
	}

	public override bool BlocksWaterFor(BasePlayer player)
	{
		BaseVehicle vehicleParent = GetVehicleParent();
		if (vehicleParent == null)
		{
			return false;
		}
		return vehicleParent.BlocksWaterFor(player);
	}

	public override float WaterFactorForPlayer(BasePlayer player)
	{
		BaseVehicle vehicleParent = GetVehicleParent();
		if (vehicleParent == null)
		{
			return 0f;
		}
		return vehicleParent.WaterFactorForPlayer(player);
	}

	public override float AirFactor()
	{
		BaseVehicle vehicleParent = GetVehicleParent();
		if (vehicleParent == null)
		{
			return 0f;
		}
		return vehicleParent.AirFactor();
	}
}
